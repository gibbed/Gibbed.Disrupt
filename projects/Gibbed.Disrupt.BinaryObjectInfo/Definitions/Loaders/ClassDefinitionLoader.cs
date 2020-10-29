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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Gibbed.Disrupt.BinaryObjectInfo.Definitions.Loaders
{
    internal static class ClassDefinitionLoader
    {
        public static HashedDefinitionDictionary<ClassDefinition> Load(string basePath)
        {
            var raws = LoaderHelper.Load<Raw.ClassDefinition>(GetClassDefinitionPaths(basePath));

            var pairs = new Dictionary<Raw.ClassDefinition, ClassDefinition>();
            foreach (var raw in raws)
            {
                if (pairs.Any(d => d.Value.Name == raw.Name) == true)
                {
                    throw new LoadException($"duplicate class '{raw.Name}'");
                }
                pairs.Add(raw, GetClass(raw));
            }

            var defs = new HashedDefinitionDictionary<ClassDefinition>(pairs.Values);
            foreach (var pair in pairs)
            {
                LoadClass(pair.Value, pair.Key, defs);
            }
            return defs;
        }

        private static ClassDefinition GetClass(Raw.ClassDefinition raw)
        {
            return new ClassDefinition()
            {
                Name = raw.Name,
                Hash = raw.Hash,
            };
        }

        public static ClassDefinition LoadClass(
            Raw.ClassDefinition raw,
            HashedDefinitionDictionary<ClassDefinition> root)
        {
            var def = GetClass(raw);
            LoadClass(def, raw, root);
            return def;
        }

        public static ClassDefinition LoadAnonymousClass(
            Raw.ClassDefinition raw,
            HashedDefinitionDictionary<ClassDefinition> root)
        {
            var def = new ClassDefinition();
            LoadClass(def, raw, root);
            return def;
        }

        private static void LoadClass(
            ClassDefinition def,
            Raw.ClassDefinition raw,
            HashedDefinitionDictionary<ClassDefinition> root)
        {
            def.Friends = new ReadOnlyCollection<FriendDefinition>(LoadFriends(raw.Friends, root));
            def.Fields = new ReadOnlyCollection<FieldDefinition>(LoadFields(raw.Fields));
            def.Objects = new ReadOnlyCollection<ClassDefinition>(LoadObjects(raw.Objects, root));
            def.DynamicNestedClasses = raw.DynamicNestedClasses;
            def.ClassFieldName = raw.ClassFieldName;
            def.ClassFieldHash = raw.ClassFieldHash;
        }

        private static IList<FriendDefinition> LoadFriends(
            IEnumerable<Raw.FriendDefinition> raws,
            HashedDefinitionDictionary<ClassDefinition> root)
        {
            var defs = new List<FriendDefinition>();
            if (raws != null)
            {
                defs.AddRange(raws.Select(raw => LoadFriend(raw, root)));
            }
            return defs;
        }

        private static FriendDefinition LoadFriend(
            Raw.FriendDefinition raw,
            HashedDefinitionDictionary<ClassDefinition> root)
        {
            if (root.ContainsKey(raw.Name) == false)
            {
                throw new LoadException($"could not find object '{raw.Name}'");
            }

            return new FriendDefinition()
            {
                Class = root[raw.Name],
                ConditionField = raw.ConditionField,
                ConditionValue = raw.ConditionValue,
                ConditionType = raw.ConditionType,
            };
        }

        private static IList<FieldDefinition> LoadFields(IEnumerable<Raw.FieldDefinition> raws)
        {
            var defs = new List<FieldDefinition>();
            if (raws != null)
            {
                foreach (var raw in raws)
                {
                    var def = GetField(raw);
                    if (defs.Any(fd => fd.Hash == def.Hash))
                    {
                        throw new LoadException($"duplicate field '{def.Name}'");
                    }
                    defs.Add(def);
                }
            }
            return defs;
        }

        private static FieldDefinition GetField(Raw.FieldDefinition raw)
        {
            if (raw.ArrayType != FieldType.Invalid &&
                raw.Type != FieldType.Array32 &&
                raw.Type != FieldType.Array32)
            {
                throw new LoadException(
                    $"field '{raw.Name}' specifies array type '{raw.ArrayType}' when it is not an array type");
            }

            return new FieldDefinition()
            {
                Name = raw.Name,
                Hash = raw.Hash,
                Type = raw.Type,
                ArrayType = raw.ArrayType,
                Enum = LoadEnum(raw.Enum),
            };
        }

        private static EnumDefinition LoadEnum(Raw.EnumDefinition raw)
        {
            if (raw == null)
            {
                return null;
            }

            return new EnumDefinition()
            {
                Name = raw.Name,
                Elements = new ReadOnlyCollection<EnumElementDefinition>(
                    LoadEnumElements(raw.Elements)),
            };
        }

        private static IList<EnumElementDefinition> LoadEnumElements(IEnumerable<Raw.EnumElementDefinition> raws)
        {
            var defs = new List<EnumElementDefinition>();
            if (raws != null)
            {
                foreach (var raw in raws)
                {
                    var def = GetEnumElement(raw);

                    if (defs.Any(fd => fd.Name == def.Name))
                    {
                        throw new LoadException($"duplicate element name '{def.Name}'");
                    }

                    if (defs.Any(fd => fd.Value == def.Value))
                    {
                        throw new LoadException($"duplicate element value '{def.Value}'");
                    }

                    defs.Add(def);
                }
            }
            return defs;
        }

        private static EnumElementDefinition GetEnumElement(Raw.EnumElementDefinition raw)
        {
            return new EnumElementDefinition()
            {
                Name = raw.Name,
                Value = raw.Value,
            };
        }

        private static IList<ClassDefinition> LoadObjects(
            IEnumerable<Raw.ClassDefinition> raws,
            HashedDefinitionDictionary<ClassDefinition> root)
        {
            var defs = new List<ClassDefinition>();
            if (raws != null)
            {
                foreach (var raw in raws)
                {
                    var def = LoadClass(raw, root);
                    if (defs.Any(fd => fd.Hash == def.Hash))
                    {
                        throw new LoadException($"duplicate object '{def.Name}'");
                    }
                    defs.Add(def);
                }
            }
            return defs;
        }

        private static IEnumerable<string> GetClassDefinitionPaths(string basePath)
        {
            return Directory.GetFiles(basePath, "*.binaryclass.xml", SearchOption.AllDirectories);
        }
    }
}
