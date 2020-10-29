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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;
using Gibbed.IO;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers
{
    internal class Array32Handler : IFieldHandler
    {
        public byte[] Import(FieldDefinition def, FieldType arrayFieldType, XPathNavigator nav)
        {
            using (var temp = new MemoryStream())
            {
                var items = nav.Select("item");
                temp.WriteValueS32(items.Count);
                while (items.MoveNext() == true)
                {
                    temp.WriteBytes(FieldHandling.Import(null, arrayFieldType, FieldType.Invalid, items.Current));
                }
                return temp.ToArray();
            }
        }

        public void Export(
            FieldDefinition def,
            FieldType arrayFieldType,
            byte[] buffer,
            int offset,
            int count,
            XmlWriter writer,
            out int read)
        {
            if (Helpers.HasLeft(buffer, offset, buffer.Length, 4) == false)
            {
                throw new FormatException("Array32 requires at least 4 bytes");
            }

            var itemCount = BitConverter.ToUInt32(buffer, offset);
            read = 4;
            offset += 4;
            int remaining = buffer.Length - offset;
            for (uint i = 0; i < itemCount; i++)
            {
                writer.WriteStartElement("item");
                FieldHandling.Export(
                    null,
                    arrayFieldType,
                    FieldType.Invalid,
                    buffer,
                    offset,
                    remaining,
                    writer,
                    out var itemRead);
                offset += itemRead;
                remaining -= itemRead;
                writer.WriteEndElement();
                read += itemRead;
            }
        }
    }
}
