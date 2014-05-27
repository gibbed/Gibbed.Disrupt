﻿/* Copyright (c) 2014 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.Disrupt.FileFormats;
using Gibbed.ProjectData;
using NDesk.Options;

namespace RebuildFileLists
{
    internal class Program
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

        public static void Main(string[] args)
        {
            bool showHelp = false;
            string currentProject = null;

            var options = new OptionSet()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
                { "p|project=", "override current project", v => currentProject = v },
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

            var manager = Manager.Load(currentProject);
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Nothing to do: no active project loaded.");
                return;
            }

            var project = manager.ActiveProject;
            var version = -1;
            HashList<uint> knownHashes = null;

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
            fatPaths.AddRange(Directory.GetFiles(installPath, "*.fat", SearchOption.AllDirectories));

            var outputPaths = new List<string>();

            var breakdown = new Breakdown();
            var tracking = new Tracking();

            Console.WriteLine("Processing...");
            for (int i = 0; i < fatPaths.Count; i++)
            {
                var fatPath = fatPaths[i];

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

                if (File.Exists(fatPath + ".bak") == true)
                {
                    fatPath += ".bak";
                }

                var fat = new BigFile();
                using (var input = File.OpenRead(fatPath))
                {
                    fat.Deserialize(input);
                }

                if (version == -1)
                {
                    version = fat.Version;
                    knownHashes = manager.LoadListsFileNames(fat.Version);
                }
                else if (version != fat.Version)
                {
                    throw new InvalidOperationException();
                }

                if (knownHashes == null)
                {
                    throw new InvalidOperationException();
                }

                HandleEntries(fat.Entries.Select(e => e.NameHash).Distinct(),
                              knownHashes,
                              tracking,
                              breakdown,
                              outputPath);
            }

            using (var output = new StreamWriter(Path.Combine(Path.Combine(listsPath, "files"), "status.txt")))
            {
                output.WriteLine("{0}",
                                 new Breakdown()
                                 {
                                     Known = tracking.Names.Distinct().Count(),
                                     Total = tracking.Hashes.Distinct().Count(),
                                 });
            }
        }

        private static void HandleEntries(IEnumerable<uint> entries,
                                          HashList<uint> knownHashes,
                                          Tracking tracking,
                                          Breakdown breakdown,
                                          string outputPath)
        {
            var localBreakdown = new Breakdown();

            var localNames = new List<string>();
            var localHashes = entries.ToArray();
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

            breakdown.Known += localBreakdown.Known;
            breakdown.Total += localBreakdown.Total;

            var outputParent = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputParent) == false)
            {
                Directory.CreateDirectory(outputParent);
            }

            using (var writer = new StringWriter())
            {
                writer.WriteLine("; {0}", localBreakdown);

                foreach (string name in distinctLocalNames.OrderBy(dn => dn))
                {
                    writer.WriteLine(name);
                }

                writer.Flush();

                using (var output = new StreamWriter(outputPath))
                {
                    output.Write(writer.GetStringBuilder());
                }
            }
        }
    }
}
