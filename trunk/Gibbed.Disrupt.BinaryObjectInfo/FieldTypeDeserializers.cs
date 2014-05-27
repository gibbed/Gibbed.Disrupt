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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;
using Gibbed.Disrupt.FileFormats;

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    public static class FieldTypeDeserializers
    {
        private static bool HasLeft(byte[] data, int offset, int count, int needCount)
        {
            return data != null &&
                   data.Length >= offset + count &&
                   offset + needCount <= offset + count;
        }

        public static object Deserialize(FieldType fieldType, byte[] data, int offset, int count, out int read)
        {
            switch (fieldType)
            {
                case FieldType.Boolean:
                {
                    if (count == 0)
                    {
                        read = 0;
                        return false;
                    }

                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type Boolean requires 1 byte");
                    }

                    if (data[offset] != 0 &&
                        data[offset] != 1)
                    {
                        throw new FormatException("invalid value for field type Boolean");
                    }

                    read = 1;
                    return data[offset] != 0;
                }

                case FieldType.UInt8:
                {
                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type UInt8 requires 1 byte");
                    }

                    read = 1;
                    return data[offset];
                }

                case FieldType.Int8:
                {
                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type Int8 requires 1 byte");
                    }

                    read = 1;
                    return (sbyte)data[offset];
                }

                case FieldType.UInt16:
                {
                    if (HasLeft(data, offset, count, 2) == false)
                    {
                        throw new FormatException("field type UInt16 requires 2 bytes");
                    }

                    read = 2;
                    return BitConverter.ToUInt16(data, offset);
                }

                case FieldType.Int16:
                {
                    if (HasLeft(data, offset, count, 2) == false)
                    {
                        throw new FormatException("field type Int16 requires 2 bytes");
                    }

                    read = 2;
                    return BitConverter.ToInt16(data, offset);
                }

                case FieldType.UInt32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type UInt32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToUInt32(data, offset);
                }

                case FieldType.Int32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Int32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToInt32(data, offset);
                }

                case FieldType.UInt64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type UInt64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToUInt64(data, offset);
                }

                case FieldType.Int64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Int64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToInt64(data, offset);
                }

                case FieldType.Float32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Float32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToSingle(data, offset);
                }

                case FieldType.Float64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Float64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToDouble(data, offset);
                }

                case FieldType.Vector2:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Vector2 requires 8 bytes");
                    }

                    read = 8;
                    return new Vector2
                    {
                        X = BitConverter.ToSingle(data, offset + 0),
                        Y = BitConverter.ToSingle(data, offset + 4),
                    };
                }

                case FieldType.Vector3:
                {
                    if (HasLeft(data, offset, count, 12) == false)
                    {
                        throw new FormatException("field type Vector3 requires 12 bytes");
                    }

                    read = 12;
                    return new Vector3
                    {
                        X = BitConverter.ToSingle(data, offset + 0),
                        Y = BitConverter.ToSingle(data, offset + 4),
                        Z = BitConverter.ToSingle(data, offset + 8),
                    };
                }

                case FieldType.Vector4:
                {
                    if (HasLeft(data, offset, count, 16) == false)
                    {
                        throw new FormatException("field type Vector4 requires 16 bytes");
                    }

                    read = 16;
                    return new Vector4
                    {
                        X = BitConverter.ToSingle(data, offset + 0),
                        Y = BitConverter.ToSingle(data, offset + 4),
                        Z = BitConverter.ToSingle(data, offset + 8),
                        W = BitConverter.ToSingle(data, offset + 12),
                    };
                }

                case FieldType.String:
                {
                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type String requires at least 1 byte");
                    }

                    int length, o;
                    for (length = 0, o = offset; data[o] != 0 && o < data.Length; length++, o++)
                    {
                    }

                    if (o == data.Length)
                    {
                        throw new FormatException("invalid trailing byte value for field type String");
                    }

                    /*
                    if (data[data.Length - 1] != 0)
                    {
                        throw new FormatException("invalid trailing byte value for field type String");
                    }
                    */

                    read = length + 1;
                    return Encoding.UTF8.GetString(data, offset, length);
                }

                case FieldType.Enum:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Enum requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToInt32(data, offset);
                }

                case FieldType.Hash32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Hash32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToUInt32(data, offset);
                }

                case FieldType.Hash64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Hash64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToUInt64(data, offset);
                }

                case FieldType.Id32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Id32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToUInt32(data, offset);
                }

                case FieldType.Id64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Id64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToUInt64(data, offset);
                }

                default:
                {
                    throw new NotSupportedException("unsupported field type");
                }
            }
        }

        public static TType Deserialize<TType>(FieldType fieldType, byte[] data)
        {
            int read;
            var value = (TType)Deserialize(fieldType, data, 0, data.Length, out read);
            if (read != data.Length)
            {
                throw new FormatException();
            }
            return value;
        }

        public static TType Deserialize<TType>(FieldType fieldType, byte[] data, int offset, int count, out int read)
        {
            return (TType)Deserialize(fieldType, data, offset, count, out read);
        }

        private static void Deserialize(XmlWriter writer,
                                        FieldType fieldType,
                                        byte[] data,
                                        int offset,
                                        int count,
                                        out int read)
        {
            switch (fieldType)
            {
                case FieldType.Boolean:
                {
                    var value = Deserialize<bool>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.UInt8:
                {
                    var value = Deserialize<byte>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Int8:
                {
                    var value = Deserialize<sbyte>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.UInt16:
                {
                    var value = Deserialize<ushort>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Int16:
                {
                    var value = Deserialize<short>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.UInt32:
                {
                    var value = Deserialize<uint>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Int32:
                {
                    var value = Deserialize<int>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.UInt64:
                {
                    var value = Deserialize<ulong>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Int64:
                {
                    var value = Deserialize<long>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Float32:
                {
                    var value = Deserialize<float>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Float64:
                {
                    var value = Deserialize<double>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Vector2:
                {
                    var value = Deserialize<Vector2>(fieldType, data, offset, count, out read);
                    writer.WriteString(string.Format("{0},{1}",
                                                     value.X.ToString(CultureInfo.InvariantCulture),
                                                     value.Y.ToString(CultureInfo.InvariantCulture)));
                    break;
                }

                case FieldType.Vector3:
                {
                    var value = Deserialize<Vector3>(fieldType, data, offset, count, out read);
                    writer.WriteString(string.Format("{0},{1},{2}",
                                                     value.X.ToString(CultureInfo.InvariantCulture),
                                                     value.Y.ToString(CultureInfo.InvariantCulture),
                                                     value.Z.ToString(CultureInfo.InvariantCulture)));
                    break;
                }

                case FieldType.Vector4:
                {
                    var value = Deserialize<Vector4>(fieldType, data, offset, count, out read);
                    writer.WriteString(string.Format("{0},{1},{2},{3}",
                                                     value.X.ToString(CultureInfo.InvariantCulture),
                                                     value.Y.ToString(CultureInfo.InvariantCulture),
                                                     value.Z.ToString(CultureInfo.InvariantCulture),
                                                     value.W.ToString(CultureInfo.InvariantCulture)));
                    break;
                }

                case FieldType.String:
                {
                    var value = Deserialize<string>(fieldType, data, offset, count, out read);
                    writer.WriteString(value);
                    break;
                }

                case FieldType.Hash32:
                {
                    var value = Deserialize<uint>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString("X8", CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Hash64:
                {
                    var value = Deserialize<ulong>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString("X16", CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Id32:
                {
                    var value = Deserialize<uint>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Id64:
                {
                    var value = Deserialize<ulong>(fieldType, data, offset, count, out read);
                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                default:
                {
                    throw new NotSupportedException("unsupported field type");
                }
            }
        }

        public static void Deserialize(XmlWriter writer,
                                       FieldDefinition fieldDef,
                                       byte[] data)
        {
            int read;

            switch (fieldDef.Type)
            {
                case FieldType.BinHex:
                {
                    writer.WriteBinHex(data, 0, data.Length);
                    read = data.Length;
                    break;
                }

                case FieldType.Boolean:
                case FieldType.UInt8:
                case FieldType.Int8:
                case FieldType.UInt16:
                case FieldType.Int16:
                case FieldType.UInt32:
                case FieldType.Int32:
                case FieldType.UInt64:
                case FieldType.Int64:
                case FieldType.Float32:
                case FieldType.Float64:
                case FieldType.Vector2:
                case FieldType.Vector3:
                case FieldType.Vector4:
                case FieldType.String:
                case FieldType.Hash32:
                case FieldType.Hash64:
                case FieldType.Id32:
                case FieldType.Id64:
                {
                    Deserialize(writer, fieldDef.Type, data, 0, data.Length, out read);
                    break;
                }

                case FieldType.Enum:
                {
                    var value = Deserialize<int>(fieldDef.Type, data, 0, data.Length, out read);

                    if (fieldDef.Enum != null)
                    {
                        var enumDef = fieldDef.Enum.Elements.FirstOrDefault(ed => ed.Value == value);
                        if (enumDef != null)
                        {
                            writer.WriteString(enumDef.Name);
                            break;
                        }
                    }

                    writer.WriteString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case FieldType.Rml:
                {
                    if (HasLeft(data, 0, data.Length, 5) == false)
                    {
                        throw new FormatException("field type Rml requires at least 5 bytes");
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
                    break;
                }

                case FieldType.Array32:
                {
                    if (HasLeft(data, 0, data.Length, 4) == false)
                    {
                        throw new FormatException("field type Array32 requires at least 4 bytes");
                    }

                    var itemCount = BitConverter.ToUInt32(data, 0);

                    read = 4;
                    int offset = 4;
                    int remaining = data.Length - offset;
                    for (uint i = 0; i < itemCount; i++)
                    {
                        writer.WriteStartElement("item");
                        int itemRead;
                        Deserialize(writer, fieldDef.ArrayType, data, offset, remaining, out itemRead);
                        offset += itemRead;
                        remaining -= itemRead;
                        writer.WriteEndElement();

                        read += itemRead;
                    }

                    break;
                }

                default:
                {
                    throw new NotSupportedException("unsupported field type");
                }
            }

            if (read != data.Length)
            {
                if (string.IsNullOrEmpty(fieldDef.Name) == false)
                {
                    throw new FormatException(
                        string.Format("did not consume all data for field '{0}' (read {1}, total {2})",
                                      fieldDef.Name,
                                      read,
                                      data.Length));
                }

                throw new FormatException(
                    string.Format("did not consume all data for field 0x{0:X8}  (read {1}, total {2})",
                                  fieldDef.Hash,
                                  read,
                                  data.Length));
            }
        }
    }
}
