/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.Disrupt.FileFormats;
using Gibbed.ProjectData;
using NDesk.Options;
using Big = Gibbed.Disrupt.FileFormats.Big;

namespace Gibbed.Disrupt.Packing
{
    public static class RebuildFileLists<TArchive, THash>
        where TArchive : Big.IArchive<THash>, new()
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static string GetListPath(string installPath, string inputPath)
        {
            installPath = installPath.ToLowerInvariant();
            inputPath = inputPath.ToLowerInvariant();

            if (inputPath.StartsWith(installPath) == false)
            {
                return null;
            }

            var baseName = inputPath.Substring(installPath.Length + 1);

            string outputPath;
            outputPath = Path.Combine("files", baseName);
            outputPath = Path.ChangeExtension(outputPath, ".filelist");
            return outputPath;
        }

        public static void Main(string[] args, string projectName)
        {
            Main(args, projectName, null);
        }

        public static void Main(string[] args, string projectName, Big.TryGetHashOverride<THash> tryGetHashOverride)
        {
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count != 0 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            Console.WriteLine("Loading project...");

            var manager = Manager.Load(projectName);
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Nothing to do: no active project loaded.");
                return;
            }

            var project = manager.ActiveProject;
            byte? nameHashVersion = null;
            HashList<THash> knownHashes = null;

            var installPath = project.InstallPath;
            var listsPath = project.ListsPath;

            if (installPath == null)
            {
                Console.WriteLine("Could not detect install path.");
                return;
            }

            if (listsPath == null)
            {
                Console.WriteLine("Could not detect lists path.");
                return;
            }

            Console.WriteLine("Searching for archives...");
            var fatPaths = new List<string>();
            fatPaths.AddRange(Directory.GetFiles(installPath, "*.fat.bak", SearchOption.AllDirectories));
            foreach (var fatPath in Directory.GetFiles(installPath, "*.fat", SearchOption.AllDirectories))
            {
                if (fatPaths.Contains(fatPath + ".bak") == false)
                {
                    fatPaths.Add(fatPath);
                }
            }
            fatPaths.Sort();

            var outputPaths = new List<string>();

            var breakdown = new Breakdown();
            var tracking = new Tracking();

            Console.WriteLine("Processing...");
            for (int i = 0; i < fatPaths.Count; i++)
            {
                var fatPath = fatPaths[i];
                var inputPath = fatPath;
                if (fatPath.EndsWith(".bak") == true)
                {
                    fatPath = fatPath.Substring(0, fatPath.Length - 4);
                }

                var outputPath = GetListPath(installPath, fatPath);
                if (outputPath == null)
                {
                    throw new InvalidOperationException();
                }

                Console.WriteLine(outputPath);
                outputPath = Path.Combine(listsPath, outputPath);

                if (outputPaths.Contains(outputPath) == true)
                {
                    throw new InvalidOperationException();
                }

                outputPaths.Add(outputPath);

                var fat = new TArchive();
                using (var input = File.OpenRead(inputPath))
                {
                    fat.Deserialize(input);
                }

                if (nameHashVersion == null)
                {
                    nameHashVersion = fat.NameHashVersion;

                    Console.WriteLine("Loading file lists for version {0}...", nameHashVersion);

                    THash wrappedComputeNameHash(string s) =>
                        fat.ComputeNameHash(s, tryGetHashOverride);
                    manager.LoadListsFileNames(wrappedComputeNameHash, out knownHashes);
                }
                else if (nameHashVersion != fat.NameHashVersion)
                {
                    throw new InvalidOperationException();
                }

                if (knownHashes == null)
                {
                    throw new InvalidOperationException();
                }

                HandleEntries(
                    fat,
                    knownHashes,
                    tracking,
                    outputPath);
            }

            var statusPath = Path.Combine(listsPath, "files", "status.txt");
            using (var output = new StreamWriter(statusPath, false, new UTF8Encoding(false)))
            {
                output.WriteLine(
                    "{0}",
                    new Breakdown()
                    {
                        Known = tracking.Names.Distinct().Count(),
                        Total = tracking.Hashes.Distinct().Count(),
                    });

                // TODO(gibbed): breakdown all archives individually
            }
        }

        private static void HandleEntries(
            TArchive fat,
            HashList<THash> knownHashes,
            Tracking tracking,
            string outputPath)
        {
            var localBreakdown = new Breakdown();

            var localNames = new List<string>();
            var localHashes = fat.Entries
                .Select(e => e.NameHash)
                .Concat(GetDependentHashes(fat))
                .Distinct()
                .ToArray();
            foreach (var hash in localHashes)
            {
                var name = knownHashes[hash];
                if (name != null)
                {
                    localNames.Add(name);
                }
                localBreakdown.Total++;
            }

            tracking.Hashes.AddRange(localHashes);
            tracking.Names.AddRange(localNames);

            var distinctLocalNames = localNames.Distinct().ToArray();
            localBreakdown.Known += distinctLocalNames.Length;

            var outputParent = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputParent) == false)
            {
                Directory.CreateDirectory(outputParent);
            }

            using (var writer = new StringWriter())
            {
                writer.WriteLine("; {0}", localBreakdown);
                if (fat is Big.IDependentArchive<THash> dependentFat)
                {
                    if (dependentFat.HasArchiveHash == true || dependentFat.Dependencies.Count > 0)
                    {
                        writer.WriteLine(";");
                    }
                    if (dependentFat.HasArchiveHash == true)
                    {
                        writer.WriteLine("; archive={0}",
                            knownHashes[dependentFat.ArchiveHash] ??
                                fat.RenderNameHash(dependentFat.ArchiveHash));
                    }
                    foreach (var dependency in dependentFat.Dependencies)
                    {
                        writer.WriteLine("; dependency={0} @ {1}",
                            knownHashes[dependency.ArchiveHash] ??
                                fat.RenderNameHash(dependency.ArchiveHash),
                            knownHashes[dependency.NameHash] ??
                                fat.RenderNameHash(dependency.NameHash));
                    }
                }
                foreach (string name in distinctLocalNames.OrderBy(dn => dn))
                {
                    writer.WriteLine(name);
                }
                writer.Flush();

                using (var output = new StreamWriter(outputPath, false, new UTF8Encoding(false)))
                {
                    output.Write(writer.GetStringBuilder());
                }
            }
        }

        private static IEnumerable<THash> GetDependentHashes(TArchive fat)
        {
            if (fat is Big.IDependentArchive<THash> dependentFat)
            {
                if (dependentFat.HasArchiveHash == true)
                {
                    yield return dependentFat.ArchiveHash;
                }

                foreach (var dependency in dependentFat.Dependencies)
                {
                    yield return dependency.ArchiveHash;
                    yield return dependency.NameHash;
                }
            }
        }

        internal class Tracking
        {
            public readonly List<THash> Hashes = new List<THash>();
            public readonly List<string> Names = new List<string>();
        }

        internal class Breakdown
        {
            public long Known = 0;
            public long Total = 0;

            public int Percent
            {
                get
                {
                    return this.Total == 0
                        ? 0
                        : (int)Math.Floor(((float)this.Known / this.Total) * 100.0f);
                }
            }

            public override string ToString()
            {
                return $"{this.Known}/{this.Total} ({this.Percent}%)";
            }
        }
    }
}
