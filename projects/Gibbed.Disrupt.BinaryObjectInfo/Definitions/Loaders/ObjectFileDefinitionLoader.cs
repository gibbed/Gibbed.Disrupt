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

using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gibbed.Disrupt.BinaryObjectInfo.Definitions.Loaders
{
    internal static class ObjectFileDefinitionLoader
    {
        public static NamedDefinitionDictionary<ObjectFileDefinition> Load(
            string basePath,
            HashedDefinitionDictionary<ClassDefinition> classDefs)
        {
            var raws = LoaderHelper.Load<Raw.ObjectFileDefinition>(GetObjectFilePaths(basePath));
            var defs = new List<ObjectFileDefinition>();
            var names = new List<string>();
            foreach (var raw in raws)
            {
                if (raw.Aliases.Any(alias => names.Contains(alias) == true))
                {
                    throw new LoadException($"duplicate binary object file '{raw.Name}'");
                }

                defs.Add(LoadObjectFileDefinition(raw, classDefs));
                names.AddRange(raw.Aliases);
            }
            return new NamedDefinitionDictionary<ObjectFileDefinition>(defs);
        }

        private static ObjectFileDefinition LoadObjectFileDefinition(
            Raw.ObjectFileDefinition raw,
            HashedDefinitionDictionary<ClassDefinition> classDefs)
        {
            if (string.IsNullOrEmpty(raw.Name) == true)
            {
                throw new LoadException("binary object file missing name?");
            }

            ClassDefinition classDef = null;

            if (raw.Object != null)
            {
                classDef = ClassDefinitionLoader.LoadClass(raw.Object, classDefs);
            }

            return new ObjectFileDefinition()
            {
                Name = raw.Name,
                Aliases = new ReadOnlyCollection<string>(
                    new[] { raw.Name.ToLowerInvariant() }.Concat(raw.Aliases.Select(a => a.ToLowerInvariant()))
                                                         .Distinct()
                                                         .ToList()),
                Object = classDef,
            };
        }

        private static IEnumerable<string> GetObjectFilePaths(string basePath)
        {
            return Directory.GetFiles(basePath, "*.binaryobjectfile.xml", SearchOption.AllDirectories);
        }
    }
}
