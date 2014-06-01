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
using System.IO;
using System.Xml;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;
using Gibbed.Disrupt.FileFormats;
using Gibbed.IO;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers
{
    internal class RmlHandler : IFieldHandler
    {
        public byte[] Import(FieldDefinition def, FieldType arrayFieldType, System.Xml.XPath.XPathNavigator nav)
        {
            var rml = new XmlResourceFile
            {
                Root = ConvertXml.Program.ReadNode(nav.SelectSingleNode("rml/*"))
            };

            using (var temp = new MemoryStream())
            {
                rml.Serialize(temp);
                temp.Position = 0;
                return temp.ReadBytes((uint)temp.Length);
            }
        }

        public void Export(FieldDefinition fieldDef,
                           FieldType arrayFieldType,
                           byte[] data,
                           int offset,
                           int count,
                           XmlWriter writer,
                           out int read)
        {
            if (Helpers.HasLeft(data, 0, data.Length, 5) == false)
            {
                throw new FormatException("Rml requires at least 5 bytes");
            }

            var rez = new XmlResourceFile();
            using (var input = new MemoryStream(data, 0, data.Length, false))
            {
                rez.Deserialize(input);
                read = data.Length;
            }

            writer.WriteStartElement("rml");
            ConvertXml.Program.WriteNode(writer, rez.Root);
            writer.WriteEndElement();
        }
    }
}
