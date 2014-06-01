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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gibbed.Disrupt.FileFormats;
using NDesk.Options;
using Big = Gibbed.Disrupt.FileFormats.Big;
using EntryDecompression = Gibbed.Disrupt.FileFormats.Big.EntryDecompression;

namespace Gibbed.Disrupt.Unpack
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool extractUnknowns = true;
            bool onlyUnknowns = false;
            bool extractFiles = true;
            string filterPattern = null;
            bool overwriteFiles = false;
            bool verbose = false;

            var options = new OptionSet()
            {
                { "o|overwrite", "overwrite existing files", v => overwriteFiles = v != null },
                { "nf|no-files", "don't extract files", v => extractFiles = v == null },
                { "nu|no-unknowns", "don't extract unknown files", v => extractUnknowns = v == null },
                { "ou|only-unknowns", "only extract unknown files", v => onlyUnknowns = v != null },
                { "f|filter=", "only extract files using pattern", v => filterPattern = v },
                { "v|verbose", "be verbose", v => verbose = v != null },
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

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_fat [output_dir]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Unpack files from a Big File (FAT/DAT pair).");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string fatPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(fatPath, null) + "_unpack";
            string datPath;

            Regex filter = null;
            if (string.IsNullOrEmpty(filterPattern) == false)
            {
                filter = new Regex(filterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            if (Path.GetExtension(fatPath) == ".dat")
            {
                datPath = fatPath;
                fatPath = Path.ChangeExtension(fatPath, ".fat");
            }
            else
            {
                datPath = Path.ChangeExtension(fatPath, ".dat");
            }

            if (verbose == true)
            {
                Console.WriteLine("Loading project...");
            }

            var manager = ProjectData.Manager.Load();
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Warning: no active project loaded.");
            }

            if (verbose == true)
            {
                Console.WriteLine("Reading FAT...");
            }

            BigFile fat;
            using (var input = File.OpenRead(fatPath))
            {
                fat = new BigFile();
                fat.Deserialize(input);
            }

            var hashes = manager.LoadListsFileNames(fat.Version);

            using (var input = File.OpenRead(datPath))
            {
                if (extractFiles == true)
                {
                    Big.Entry[] entries = fat.Entries.OrderBy(e => e.Offset).ToArray();

                    if (entries.Length > 0)
                    {
                        if (verbose == true)
                        {
                            Console.WriteLine("Unpacking files...");
                        }

                        long current = 0;
                        long total = entries.Length;
                        var padding = total.ToString(CultureInfo.InvariantCulture).Length;

                        var duplicates = new Dictionary<ulong, int>();

                        foreach (var entry in entries)
                        {
                            current++;

                            string entryName;
                            if (GetEntryName(input,
                                             fat,
                                             entry,
                                             hashes,
                                             extractUnknowns,
                                             onlyUnknowns,
                                             out entryName) == false)
                            {
                                continue;
                            }

                            if (duplicates.ContainsKey(entry.NameHash) == true)
                            {
                                var number = duplicates[entry.NameHash]++;
                                var e = Path.GetExtension(entryName);
                                var nn =
                                    Path.ChangeExtension(
                                        Path.ChangeExtension(entryName, null) + "__DUPLICATE_" +
                                        number.ToString(CultureInfo.InvariantCulture),
                                        e);
                                entryName = Path.Combine("__DUPLICATE", nn);
                            }
                            else
                            {
                                duplicates[entry.NameHash] = 0;
                            }

                            if (filter != null &&
                                filter.IsMatch(entryName) == false)
                            {
                                continue;
                            }

                            var entryPath = Path.Combine(outputPath, entryName);
                            if (overwriteFiles == false &&
                                File.Exists(entryPath) == true)
                            {
                                continue;
                            }

                            if (verbose == true)
                            {
                                Console.WriteLine("[{0}/{1}] {2}",
                                                  current.ToString(CultureInfo.InvariantCulture).PadLeft(padding),
                                                  total,
                                                  entryName);
                            }

                            input.Seek(entry.Offset, SeekOrigin.Begin);

                            var entryParent = Path.GetDirectoryName(entryPath);
                            if (string.IsNullOrEmpty(entryParent) == false)
                            {
                                Directory.CreateDirectory(entryParent);
                            }

                            using (var output = File.Create(entryPath))
                            {
                                EntryDecompression.Decompress(entry, input, output);
                            }
                        }
                    }
                }
            }
        }

        private static bool GetEntryName(Stream input,
                                         BigFile fat,
                                         Big.Entry entry,
                                         ProjectData.HashList<uint> hashes,
                                         bool extractUnknowns,
                                         bool onlyUnknowns,
                                         out string entryName)
        {
            entryName = hashes[entry.NameHash];

            if (entryName == null)
            {
                if (extractUnknowns == false)
                {
                    return false;
                }

                string type;
                string extension;
                {
                    var guess = new byte[64];
                    int read = 0;

                    if (entry.CompressionScheme == Big.CompressionScheme.None)
                    {
                        if (entry.CompressedSize > 0)
                        {
                            input.Seek(entry.Offset, SeekOrigin.Begin);
                            read = input.Read(guess, 0, (int)Math.Min(entry.CompressedSize, guess.Length));
                        }
                    }
                    else
                    {
                        using (var temp = new MemoryStream())
                        {
                            EntryDecompression.Decompress(entry, input, temp);
                            temp.Position = 0;
                            read = temp.Read(guess, 0, (int)Math.Min(temp.Length, guess.Length));
                        }
                    }

                    var tuple = FileExtensions.Detect(guess, Math.Min(guess.Length, read));
                    type = tuple != null ? tuple.Item1 : "unknown";
                    extension = tuple != null ? tuple.Item2 : null;
                }

                entryName = entry.NameHash.ToString("X8");

                if (string.IsNullOrEmpty(extension) == false)
                {
                    entryName = Path.ChangeExtension(entryName, "." + extension);
                }

                if (string.IsNullOrEmpty(type) == false)
                {
                    entryName = Path.Combine(type, entryName);
                }

                entryName = Path.Combine("__UNKNOWN", entryName);
            }
            else
            {
                if (onlyUnknowns == true)
                {
                    return false;
                }

                entryName = FilterEntryName(entryName);
            }

            return true;
        }

        private static string FilterEntryName(string entryName)
        {
            entryName = entryName.Replace(@"/", @"\");
            if (entryName.StartsWith(@"\") == true)
            {
                entryName = entryName.Substring(1);
            }
            return entryName;
        }
    }
}
