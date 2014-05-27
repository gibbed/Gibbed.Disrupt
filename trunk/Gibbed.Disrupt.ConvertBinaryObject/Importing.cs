/* Copyright (c) 2014 Rick (rick 'at' gibbed 'dot' us)
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
using System.Xml.XPath;
using Gibbed.Disrupt.BinaryObjectInfo;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;
using Gibbed.Disrupt.FileFormats;

namespace Gibbed.Disrupt.ConvertBinaryObject
{
    internal class Importing
    {
        private readonly InfoManager _InfoManager;

        public Importing(InfoManager infoManager)
        {
            this._InfoManager = infoManager;
        }

        public BinaryObject Import(ClassDefinition objectDef,
                                   string basePath,
                                   XPathNavigator nav)
        {
            var root = new BinaryObject();
            ReadNode(root, new BinaryObject[0], objectDef, basePath, nav);
            return root;
        }

        private void ReadNode(BinaryObject node,
                              IEnumerable<BinaryObject> parentChain,
                              ClassDefinition objectDef,
                              string basePath,
                              XPathNavigator nav)
        {
            var chain = parentChain.Concat(new[] { node });

            string className;
            uint classNameHash;

            LoadNameAndHash(nav, out className, out classNameHash);

            if (objectDef != null &&
                objectDef.ClassFieldHash.HasValue == true)
            {
                var hash = GetClassDefinitionByField(objectDef.ClassFieldName, objectDef.ClassFieldHash, nav);
                if (hash.HasValue == false)
                {
                    throw new InvalidOperationException();
                }

                objectDef = this._InfoManager.GetClassDefinition(hash.Value);
            }

            node.NameHash = classNameHash;

            var fields = nav.Select("field");
            while (fields.MoveNext() == true)
            {
                if (fields.Current == null)
                {
                    throw new InvalidOperationException();
                }

                string fieldName;
                uint fieldNameHash;

                LoadNameAndHash(fields.Current, out fieldName, out fieldNameHash);

                FieldType fieldType;
                var fieldTypeName = fields.Current.GetAttribute("type", "");
                if (Enum.TryParse(fieldTypeName, true, out fieldType) == false)
                {
                    throw new InvalidOperationException();
                }

                var arrayFieldType = FieldType.Invalid;
                var arrayFieldTypeName = fields.Current.GetAttribute("array_type", "");
                if (string.IsNullOrEmpty(arrayFieldTypeName) == false)
                {
                    if (Enum.TryParse(arrayFieldTypeName, true, out arrayFieldType) == false)
                    {
                        throw new InvalidOperationException();
                    }
                }

                var fieldDef = objectDef != null ? objectDef.GetFieldDefinition(fieldNameHash, chain) : null;
                var data = FieldTypeSerializers.Serialize(fieldDef, fieldType, arrayFieldType, fields.Current);
                node.Fields.Add(fieldNameHash, data);
            }

            var children = nav.Select("object");
            while (children.MoveNext() == true)
            {
                var child = new BinaryObject();
                LoadChildNode(child, chain, objectDef, basePath, children.Current);
                node.Children.Add(child);
            }
        }

        private void HandleChildNode(BinaryObject node,
                                     IEnumerable<BinaryObject> chain,
                                     ClassDefinition parentDef,
                                     string basePath,
                                     XPathNavigator nav)
        {
            string className;
            uint classNameHash;

            LoadNameAndHash(nav, out className, out classNameHash);

            if (parentDef == null || parentDef.DynamicNestedClasses == false)
            {
                var def = parentDef != null ? parentDef.GetObjectDefinition(classNameHash, chain) : null;
                ReadNode(node, chain, def, basePath, nav);
                return;
            }

            if (parentDef.DynamicNestedClasses == true)
            {
                var def = this._InfoManager.GetClassDefinition(classNameHash);
                ReadNode(node, chain, def, basePath, nav);
                return;
            }

            throw new InvalidOperationException();
        }

        private void LoadChildNode(BinaryObject node,
                                   IEnumerable<BinaryObject> chain,
                                   ClassDefinition objectDef,
                                   string basePath,
                                   XPathNavigator nav)
        {
            var external = nav.GetAttribute("external", "");
            if (string.IsNullOrWhiteSpace(external) == true)
            {
                HandleChildNode(node, chain, objectDef, basePath, nav);
                return;
            }

            var inputPath = Path.Combine(basePath, external);
            using (var input = File.OpenRead(inputPath))
            {
                var nestedDoc = new XPathDocument(input);
                var nestedNav = nestedDoc.CreateNavigator();

                var root = nestedNav.SelectSingleNode("/object");
                if (root == null)
                {
                    throw new InvalidOperationException();
                }

                HandleChildNode(node, chain, objectDef, Path.GetDirectoryName(inputPath), root);
            }
        }

        private static uint? GetClassDefinitionByField(string classFieldName, uint? classFieldHash, XPathNavigator nav)
        {
            uint? hash = null;

            if (string.IsNullOrEmpty(classFieldName) == false)
            {
                var fieldByName = nav.SelectSingleNode("field[@name=\"" + classFieldName + "\"]");
                if (fieldByName != null)
                {
                    uint temp;
                    if (uint.TryParse(fieldByName.Value,
                                      NumberStyles.AllowHexSpecifier,
                                      CultureInfo.InvariantCulture,
                                      out temp) == false)
                    {
                        throw new InvalidOperationException();
                    }
                    hash = temp;
                }
            }

            if (hash.HasValue == false &&
                classFieldHash.HasValue == true)
            {
                var fieldByHash =
                    nav.SelectSingleNode("field[@hash=\"" +
                                         classFieldHash.Value.ToString("X8", CultureInfo.InvariantCulture) +
                                         "\"]");
                if (fieldByHash != null)
                {
                    uint temp;
                    if (uint.TryParse(fieldByHash.Value,
                                      NumberStyles.AllowHexSpecifier,
                                      CultureInfo.InvariantCulture,
                                      out temp) == false)
                    {
                        throw new InvalidOperationException();
                    }
                    hash = temp;
                }
            }

            return hash;
        }

        private static void LoadNameAndHash(XPathNavigator nav, out string name, out uint hash)
        {
            var nameAttribute = nav.GetAttribute("name", "");
            var hashAttribute = nav.GetAttribute("hash", "");

            if (string.IsNullOrWhiteSpace(nameAttribute) == true &&
                string.IsNullOrWhiteSpace(hashAttribute) == true)
            {
                throw new FormatException();
            }

            name = string.IsNullOrWhiteSpace(nameAttribute) == false ? nameAttribute : null;
            hash = name != null ? FileFormats.Hashing.CRC32.Compute(name) : uint.Parse(hashAttribute, NumberStyles.AllowHexSpecifier);
        }
    }
}
