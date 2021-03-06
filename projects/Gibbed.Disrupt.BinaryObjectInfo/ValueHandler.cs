﻿/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
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

using System.Xml;
using System.Xml.XPath;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    internal abstract class ValueHandler<T> : IFieldHandler, IValueHandler
    {
        public abstract byte[] Serialize(T value);

        public abstract T Parse(FieldDefinition def, string text);

        public byte[] Import(FieldDefinition def, FieldType arrayFieldType, string text)
        {
            var value = this.Parse(def, text);
            var bytes = this.Serialize(value);
            return bytes;
        }

        public byte[] Import(FieldDefinition def, FieldType arrayFieldType, XPathNavigator nav)
        {
            var value = this.Parse(def, nav.Value);
            var bytes = this.Serialize(value);
            return bytes;
        }

        public abstract T Deserialize(byte[] buffer, int offset, int count, out int read);

        public abstract string Compose(FieldDefinition def, T value);

        public string Compose(FieldDefinition def, byte[] data, int offset, int count, out int read)
        {
            var value = this.Deserialize(data, offset, count, out read);
            return this.Compose(def, value);
        }

        public void Export(
            FieldDefinition def,
            FieldType arrayFieldType,
            byte[] data,
            int offset,
            int count,
            XmlWriter writer,
            out int read)
        {
            var value = this.Deserialize(data, offset, count, out read);
            writer.WriteString(this.Compose(def, value));
        }
    }
}
