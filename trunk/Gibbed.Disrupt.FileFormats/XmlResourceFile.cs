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
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Disrupt.FileFormats
{
    public class XmlResourceFile
    {
        public byte Unknown1;
        public Node Root;

        public void Deserialize(Stream input)
        {
            if (input.ReadValueU8() != 0)
            {
                throw new FormatException("not an xml resource file");
            }
            var endian = Endian.Little;

            this.Unknown1 = input.ReadValueU8();
            var stringTableSize = ReadValuePackedU32(input, endian);
            var totalNodeCount = ReadValuePackedU32(input, endian);
            var totalAttributeCount = ReadValuePackedU32(input, endian);

            uint actualNodeCount = 1, actualAttributeCount = 0;

            this.Root = new Node();
            this.Root.Deserialize(
                input, ref actualNodeCount, ref actualAttributeCount, endian);

            if (actualNodeCount != totalNodeCount ||
                actualAttributeCount != totalAttributeCount)
            {
                throw new FormatException();
            }

            var stringTableData = new byte[stringTableSize];
            input.Read(stringTableData, 0, stringTableData.Length);
            var stringTable = new StringTable();
            stringTable.Deserialize(stringTableData);

            this.Root.ReadStringTable(stringTable);
        }

        public void Serialize(Stream output)
        {
            var endian = Endian.Little;

            var stringTable = new StringTable();
            this.Root.WriteStringTable(stringTable);
            var stringTableData = stringTable.Serialize();

            output.WriteValueU8(0);
            output.WriteValueU8(0);

            using (var data = new MemoryStream())
            {
                uint totalNodeCount = 1, totalAttributeCount = 0;
                this.Root.Serialize(
                    data,
                    ref totalNodeCount,
                    ref totalAttributeCount,
                    endian);

                WriteValuePackedU32(output, (uint)stringTableData.Length, endian);
                WriteValuePackedU32(output, totalNodeCount, endian);
                WriteValuePackedU32(output, totalAttributeCount, endian);

                data.Position = 0;
                output.WriteFromStream(data, data.Length);

                output.Write(stringTableData, 0, stringTableData.Length);
            }
        }

        public class Node
        {
            public string Name;
            public string Value;

            internal uint NameIndex;
            internal uint ValueIndex;

            public List<Attribute> Attributes = new List<Attribute>();
            public List<Node> Children = new List<Node>();

            public void Deserialize(Stream input,
                                    ref uint totalNodeCount,
                                    ref uint totalAttributeCount,
                                    Endian endian)
            {
                this.NameIndex = ReadValuePackedU32(input, endian);
                this.ValueIndex = ReadValuePackedU32(input, endian);

                var attributeCount = ReadValuePackedU32(input, endian);
                var childCount = ReadValuePackedU32(input, endian);

                totalNodeCount += childCount;
                totalAttributeCount += attributeCount;

                this.Attributes.Clear();
                for (uint i = 0; i < attributeCount; i++)
                {
                    var attribute = new Attribute();
                    attribute.Deserialize(input, endian);
                    this.Attributes.Add(attribute);
                }

                this.Children.Clear();
                for (uint i = 0; i < childCount; i++)
                {
                    var child = new Node();
                    child.Deserialize(input,
                                      ref totalNodeCount,
                                      ref totalAttributeCount,
                                      endian);
                    this.Children.Add(child);
                }
            }

            public void Serialize(Stream output,
                                  ref uint totalNodeCount,
                                  ref uint totalAttributeCount,
                                  Endian endian)
            {
                WriteValuePackedU32(output, this.NameIndex, endian);
                WriteValuePackedU32(output, this.ValueIndex, endian);

                totalAttributeCount += (uint)this.Attributes.Count;
                totalNodeCount += (uint)this.Children.Count;

                WriteValuePackedU32(output, (uint)this.Attributes.Count, endian);
                WriteValuePackedU32(output, (uint)this.Children.Count, endian);

                foreach (var attribute in this.Attributes)
                {
                    attribute.Serialize(output, endian);
                }

                foreach (var child in this.Children)
                {
                    child.Serialize(output,
                                    ref totalNodeCount,
                                    ref totalAttributeCount,
                                    endian);
                }
            }

            internal void ReadStringTable(StringTable stringTable)
            {
                this.Name = stringTable.Read(this.NameIndex);
                this.Value = stringTable.Read(this.ValueIndex);

                foreach (var attribute in this.Attributes)
                {
                    attribute.ReadStringTable(stringTable);
                }

                foreach (var child in this.Children)
                {
                    child.ReadStringTable(stringTable);
                }
            }

            internal void WriteStringTable(StringTable stringTable)
            {
                this.NameIndex = stringTable.Write(this.Name);
                this.ValueIndex = stringTable.Write(this.Value);

                foreach (var attribute in this.Attributes)
                {
                    attribute.WriteStringTable(stringTable);
                }

                foreach (var child in this.Children)
                {
                    child.WriteStringTable(stringTable);
                }
            }
        }

        public class Attribute
        {
            public uint Unknown;
            public string Name;
            public string Value;

            internal uint NameIndex;
            internal uint ValueIndex;

            public void Deserialize(Stream input, Endian endian)
            {
                this.Unknown = ReadValuePackedU32(input, endian);

                if (this.Unknown != 0)
                {
                    throw new FormatException();
                }

                this.NameIndex = ReadValuePackedU32(input, endian);
                this.ValueIndex = ReadValuePackedU32(input, endian);
            }

            public void Serialize(Stream output, Endian endian)
            {
                WriteValuePackedU32(output, this.Unknown, endian);
                WriteValuePackedU32(output, this.NameIndex, endian);
                WriteValuePackedU32(output, this.ValueIndex, endian);
            }

            internal void ReadStringTable(StringTable stringTable)
            {
                this.Name = stringTable.Read(this.NameIndex);
                this.Value = stringTable.Read(this.ValueIndex);
            }

            internal void WriteStringTable(StringTable stringTable)
            {
                this.NameIndex = stringTable.Write(this.Name);
                this.ValueIndex = stringTable.Write(this.Value);
            }
        }

        internal class StringTable
        {
            private MemoryStream _Data = new MemoryStream();

            // this is dumb :effort:
            private readonly Dictionary<uint, string> _Offsets = new Dictionary<uint, string>();
            private readonly Dictionary<string, uint> _Values = new Dictionary<string, uint>();

            public string Read(uint index)
            {
                if (this._Offsets.ContainsKey(index) == false)
                {
                    throw new KeyNotFoundException();
                }

                return this._Offsets[index];
            }

            public uint Write(string value)
            {
                if (this._Values.ContainsKey(value) == true)
                {
                    return this._Values[value];
                }

                var offset = (uint)this._Data.Position;
                this._Offsets.Add(offset, value);
                this._Values.Add(value, offset);
                this._Data.WriteStringZ(value, Encoding.UTF8);
                return offset;
            }

            public void Deserialize(byte[] buffer)
            {
                this._Offsets.Clear();
                this._Values.Clear();

                this._Data = new MemoryStream(buffer);
                while (this._Data.Position < this._Data.Length)
                {
                    var offset = (uint)this._Data.Position;
                    var value = this._Data.ReadStringZ(Encoding.UTF8);
                    this._Offsets.Add(offset, value);
                    this._Values.Add(value, offset);
                }
            }

            public byte[] Serialize()
            {
                var buffer = new byte[this._Data.Length];
                Array.Copy(this._Data.GetBuffer(), buffer, buffer.Length);
                return buffer;
            }
        }

        public static uint ReadValuePackedU32(Stream input, Endian endian)
        {
            var value = input.ReadValueU8();
            if (value < 0xFE)
            {
                return value;
            }

            if (value == 0xFE)
            {
                throw new FormatException();
            }

            return input.ReadValueU32(endian);
        }

        public static void WriteValuePackedU32(Stream output, uint value, Endian endian)
        {
            if (value >= 0xFE)
            {
                output.WriteValueU8(0xFF);
                output.WriteValueU32(value, endian);
                return;
            }

            output.WriteValueU8((byte)(value & 0xFF));
        }
    }
}
