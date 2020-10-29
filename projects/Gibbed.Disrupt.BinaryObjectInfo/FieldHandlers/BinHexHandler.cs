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

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers
{
    internal class BinHexHandler : IFieldHandler
    {
        public byte[] Import(FieldDefinition def, FieldType arrayFieldType, XPathNavigator nav)
        {
            using (var reader = new XmlTextReader(new StringReader(nav.OuterXml)))
            {
                reader.MoveToContent();
                var data = new byte[0];
                int read = 0;
                do
                {
                    Array.Resize(ref data, data.Length + 4096);
                    read += reader.ReadBinHex(data, read, 4096);
                }
                while (reader.EOF == false);
                Array.Resize(ref data, read);
                return data;
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
            read = count;
            writer.WriteBinHex(buffer, offset, count);
        }
    }
}
