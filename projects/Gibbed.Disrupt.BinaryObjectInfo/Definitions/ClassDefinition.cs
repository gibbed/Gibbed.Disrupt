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
using System.Collections.ObjectModel;
using System.Linq;

namespace Gibbed.Disrupt.BinaryObjectInfo.Definitions
{
    public class ClassDefinition : IHashedDefinition
    {
        private ReadOnlyCollection<FieldDefinition> _Fields;
        private Dictionary<uint, FieldDefinition> _FieldLookup;

        private ReadOnlyCollection<ClassDefinition> _Objects;
        private Dictionary<uint, ClassDefinition> _ObjectLookup;

        public string Name { get; internal set; }
        public uint Hash { get; internal set; }

        public ReadOnlyCollection<FriendDefinition> Friends { get; internal set; }

        public ReadOnlyCollection<FieldDefinition> Fields
        {
            get { return this._Fields; }
            internal set
            {
                this._Fields = value;
                this._FieldLookup = value.ToDictionary(f => f.Hash, f => f);
            }
        }

        public ReadOnlyCollection<ClassDefinition> Objects
        {
            get { return this._Objects; }
            internal set
            {
                this._Objects = value;
                this._ObjectLookup = value.ToDictionary(f => f.Hash, f => f);
            }
        }

        public bool DynamicNestedClasses { get; internal set; }
        public string ClassFieldName { get; internal set; }
        public uint? ClassFieldHash { get; internal set; }

        public FieldDefinition GetFieldDefinition(string name, IEnumerable<FileFormats.BinaryObject> chain)
        {
            return this.GetFieldDefinition(FileFormats.Hashing.CRC32.Compute(name), chain);
        }

        public FieldDefinition GetFieldDefinition(uint hash, IEnumerable<FileFormats.BinaryObject> chain)
        {
            if (this._FieldLookup.TryGetValue(hash, out var value) == true)
            {
                return value;
            }

            var chainArray = chain?.ToArray();

            foreach (var friend in this.Friends)
            {
                if (string.IsNullOrEmpty(friend.ConditionField) == false)
                {
                    if (chain == null)
                    {
                        throw new NotSupportedException();
                    }

                    var hasFieldData = GetFieldData(friend.ConditionField, chainArray, out var fieldData);
                    if (hasFieldData == false)
                    {
                        continue;
                    }

                    var conditionData = FieldHandling.Import(null, friend.ConditionType, friend.ConditionValue);
                    if (fieldData.SequenceEqual(conditionData) == false)
                    {
                        continue;
                    }
                }

                var def = friend.Class.GetFieldDefinition(hash, chainArray);
                if (def != null)
                {
                    return def;
                }
            }

            return null;
        }

        public ClassDefinition GetObjectDefinition(string name, IEnumerable<FileFormats.BinaryObject> chain)
        {
            return this.GetObjectDefinition(FileFormats.Hashing.CRC32.Compute(name), chain);
        }

        public ClassDefinition GetObjectDefinition(uint hash, IEnumerable<FileFormats.BinaryObject> chain)
        {
            if (this._ObjectLookup.TryGetValue(hash, out var value) == true)
            {
                return value;
            }

            var chainArray = chain?.ToArray();

            foreach (var friend in this.Friends)
            {
                if (string.IsNullOrEmpty(friend.ConditionField) == false)
                {
                    if (chain == null)
                    {
                        throw new NotSupportedException();
                    }

                    var hasFieldData = GetFieldData(friend.ConditionField, chainArray, out var fieldData);
                    if (hasFieldData == false)
                    {
                        continue;
                    }

                    var conditionData = FieldHandling.Import(null, friend.ConditionType, friend.ConditionValue);
                    if (fieldData.SequenceEqual(conditionData) == false)
                    {
                        continue;
                    }
                }

                var def = friend.Class.GetObjectDefinition(hash, chainArray);
                if (def != null)
                {
                    return def;
                }
            }

            return null;
        }

        private static bool GetFieldData(string path, IEnumerable<FileFormats.BinaryObject> chain, out byte[] data)
        {
            data = null;

            if (chain == null)
            {
                return false;
            }

            int skip = 0;
            int i;
            for (i = 0; i < path.Length; i++)
            {
                if (path[i] == '^')
                {
                    skip++;
                }
                else
                {
                    break;
                }
            }

            var node = chain.Reverse().Skip(skip).FirstOrDefault();
            if (node == null)
            {
                return false;
            }

            var parts = path.Substring(i).Split('.');

            var children = parts.Take(parts.Length - 1);
            var last = parts.Last();

            foreach (var child in children)
            {
                node = node.Children.FirstOrDefault(c => c.NameHash == FileFormats.Hashing.CRC32.Compute(child));
                if (node == null)
                {
                    return false;
                }
            }

            var hash = FileFormats.Hashing.CRC32.Compute(last);
            return node.Fields.TryGetValue(hash, out data) == true && data != null;
        }
    }
}
