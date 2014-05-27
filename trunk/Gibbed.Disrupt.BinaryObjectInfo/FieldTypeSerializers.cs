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
using System.Xml.XPath;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;
using Gibbed.Disrupt.FileFormats;
using Gibbed.IO;

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    public static class FieldTypeSerializers
    {
        public static byte[] Serialize(FieldType fieldType, string text)
        {
            switch (fieldType)
            {
                case FieldType.Boolean:
                {
                    bool value;
                    if (bool.TryParse(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return new[] { (byte)(value == true ? 1 : 0) };
                }

                case FieldType.UInt8:
                {
                    byte value;
                    if (TryParseUInt8(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return new[] { value };
                }

                case FieldType.Int8:
                {
                    sbyte value;
                    if (TryParseInt8(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return new[] { (byte)value };
                }

                case FieldType.UInt16:
                {
                    ushort value;
                    if (TryParseUInt16(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Int16:
                {
                    short value;
                    if (TryParseInt16(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.UInt32:
                {
                    uint value;
                    if (TryParseUInt32(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Int32:
                {
                    int value;
                    if (TryParseInt32(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.UInt64:
                {
                    ulong value;
                    if (TryParseUInt64(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Int64:
                {
                    long value;
                    if (TryParseInt64(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Float32:
                {
                    float value;
                    if (TryParseFloat32(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Float64:
                {
                    double value;
                    if (TryParseFloat64(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Vector2:
                {
                    var parts = text.Split(',');
                    if (parts.Length != 2)
                    {
                        throw new FormatException("field type Vector2 requires 2 float values delimited by a comma");
                    }

                    float x, y;

                    if (TryParseFloat32(parts[0], out x) == false)
                    {
                        throw new FormatException();
                    }

                    if (TryParseFloat32(parts[1], out y) == false)
                    {
                        throw new FormatException();
                    }

                    var data = new byte[8];
                    Array.Copy(BitConverter.GetBytes(x), 0, data, 0, 4);
                    Array.Copy(BitConverter.GetBytes(y), 0, data, 4, 4);
                    return data;
                }

                case FieldType.Vector3:
                {
                    var parts = text.Split(',');
                    if (parts.Length != 3)
                    {
                        throw new FormatException("field type Vector3 requires 3 float values delimited by a comma");
                    }

                    float x, y, z;

                    if (TryParseFloat32(parts[0], out x) == false)
                    {
                        throw new FormatException();
                    }

                    if (TryParseFloat32(parts[1], out y) == false)
                    {
                        throw new FormatException();
                    }

                    if (TryParseFloat32(parts[2], out z) == false)
                    {
                        throw new FormatException();
                    }

                    var data = new byte[12];
                    Array.Copy(BitConverter.GetBytes(x), 0, data, 0, 4);
                    Array.Copy(BitConverter.GetBytes(y), 0, data, 4, 4);
                    Array.Copy(BitConverter.GetBytes(z), 0, data, 8, 4);
                    return data;
                }

                case FieldType.Vector4:
                {
                    var parts = text.Split(',');
                    if (parts.Length != 4)
                    {
                        throw new FormatException("field type Vector4 requires 4 float values delimited by a comma");
                    }

                    float x, y, z, w;

                    if (TryParseFloat32(parts[0], out x) == false)
                    {
                        throw new FormatException();
                    }

                    if (TryParseFloat32(parts[1], out y) == false)
                    {
                        throw new FormatException();
                    }

                    if (TryParseFloat32(parts[2], out z) == false)
                    {
                        throw new FormatException();
                    }

                    if (TryParseFloat32(parts[3], out w) == false)
                    {
                        throw new FormatException();
                    }

                    var data = new byte[16];
                    Array.Copy(BitConverter.GetBytes(x), 0, data, 0, 4);
                    Array.Copy(BitConverter.GetBytes(y), 0, data, 4, 4);
                    Array.Copy(BitConverter.GetBytes(z), 0, data, 8, 4);
                    Array.Copy(BitConverter.GetBytes(w), 0, data, 12, 4);
                    return data;
                }

                case FieldType.String:
                {
                    var data = Encoding.UTF8.GetBytes(text);
                    Array.Resize(ref data, data.Length + 1);
                    return data;
                }

                case FieldType.Hash32:
                {
                    uint value;
                    if (TryParseHash32(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Hash64:
                {
                    ulong value;
                    if (TryParseHash64(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Id32:
                {
                    uint value;
                    if (TryParseUInt32(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.Id64:
                {
                    ulong value;
                    if (TryParseUInt64(text, out value) == false)
                    {
                        throw new FormatException();
                    }
                    return BitConverter.GetBytes(value);
                }

                case FieldType.ComputeHash32:
                {
                    var value = FileFormats.Hashing.CRC32.Compute(text);
                    return BitConverter.GetBytes(value);
                }

                case FieldType.ComputeHash64:
                {
                    var value = FileFormats.Hashing.CRC64.Compute(text);
                    return BitConverter.GetBytes(value);
                }
            }

            throw new NotSupportedException("unsupported field type");
        }

        public static byte[] Serialize(FieldDefinition fieldDef,
                                       FieldType fieldType,
                                       FieldType arrayFieldType,
                                       XPathNavigator nav)
        {
            switch (fieldType)
            {
                case FieldType.BinHex:
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
                {
                    return Serialize(fieldType, nav.Value);
                }

                case FieldType.Enum:
                {
                    var enumDef = fieldDef != null ? fieldDef.Enum : null;

                    var text = nav.Value;
                    var elementDef = enumDef != null
                                         ? enumDef.Elements.FirstOrDefault(ed => ed.Name == text)
                                         : null;

                    int value;
                    if (elementDef != null)
                    {
                        value = elementDef.Value;
                    }
                    else
                    {
                        if (TryParseInt32(nav.Value, out value) == false)
                        {
                            if (enumDef == null)
                            {
                                throw new FormatException(
                                    string.Format(
                                        "could not parse enum value '{0}' as an Int32 (perhaps enum definition is missing?)",
                                        nav.Value));
                            }

                            throw new FormatException(
                                string.Format(
                                    "could not parse enum value '{0}' as an Int32 (perhaps enum element definition is missing from {1}?)",
                                    nav.Value,
                                    enumDef.Name));
                        }
                    }

                    return BitConverter.GetBytes(value);
                }

                case FieldType.Hash32:
                case FieldType.Hash64:
                case FieldType.Id32:
                case FieldType.Id64:
                {
                    return Serialize(fieldType, nav.Value);
                }

                case FieldType.Rml:
                {
                    throw new NotImplementedException();
                    /*
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
                    */
                }

                case FieldType.ComputeHash32:
                {
                    return Serialize(fieldType, nav.Value);
                }

                case FieldType.ComputeHash64:
                {
                    return Serialize(fieldType, nav.Value);
                }

                case FieldType.Array32:
                {
                    using (var temp = new MemoryStream())
                    {
                        var items = nav.Select("item");
                        temp.WriteValueS32(items.Count);
                        while (items.MoveNext() == true)
                        {
                            temp.WriteBytes(Serialize(arrayFieldType, items.Current.Value));
                        }
                        return temp.ToArray();
                    }
                }
            }

            throw new NotSupportedException("unsupported field type");
        }

        private static bool TryParseUInt8(string value, out byte result)
        {
            return byte.TryParse(value,
                                 NumberStyles.Number,
                                 CultureInfo.InvariantCulture,
                                 out result);
        }

        private static bool TryParseInt8(string value, out sbyte result)
        {
            return sbyte.TryParse(value,
                                  NumberStyles.Number,
                                  CultureInfo.InvariantCulture,
                                  out result);
        }

        private static bool TryParseUInt16(string value, out ushort result)
        {
            return ushort.TryParse(value,
                                   NumberStyles.Number,
                                   CultureInfo.InvariantCulture,
                                   out result);
        }

        private static bool TryParseInt16(string value, out short result)
        {
            return short.TryParse(value,
                                  NumberStyles.Number,
                                  CultureInfo.InvariantCulture,
                                  out result);
        }

        private static bool TryParseUInt32(string value, out uint result)
        {
            return uint.TryParse(value,
                                 NumberStyles.Number,
                                 CultureInfo.InvariantCulture,
                                 out result);
        }

        private static bool TryParseInt32(string value, out int result)
        {
            return int.TryParse(value,
                                NumberStyles.Number,
                                CultureInfo.InvariantCulture,
                                out result);
        }

        private static bool TryParseUInt64(string value, out ulong result)
        {
            return ulong.TryParse(value,
                                  NumberStyles.Number,
                                  CultureInfo.InvariantCulture,
                                  out result);
        }

        private static bool TryParseInt64(string value, out long result)
        {
            return long.TryParse(value,
                                 NumberStyles.Number,
                                 CultureInfo.InvariantCulture,
                                 out result);
        }

        private static bool TryParseFloat32(string value, out float result)
        {
            return float.TryParse(value,
                                  NumberStyles.Float | NumberStyles.AllowThousands,
                                  CultureInfo.InvariantCulture,
                                  out result);
        }

        private static bool TryParseFloat64(string value, out double result)
        {
            return double.TryParse(value,
                                   NumberStyles.Float | NumberStyles.AllowThousands,
                                   CultureInfo.InvariantCulture,
                                   out result);
        }

        private static bool TryParseHash32(string value, out uint result)
        {
            return uint.TryParse(value,
                                 NumberStyles.AllowHexSpecifier,
                                 CultureInfo.InvariantCulture,
                                 out result);
        }

        private static bool TryParseHash64(string value, out ulong result)
        {
            return ulong.TryParse(value,
                                  NumberStyles.AllowHexSpecifier,
                                  CultureInfo.InvariantCulture,
                                  out result);
        }
    }
}
