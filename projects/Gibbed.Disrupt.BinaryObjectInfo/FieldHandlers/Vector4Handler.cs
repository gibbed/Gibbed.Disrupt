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
using System.Globalization;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;
using Gibbed.Disrupt.FileFormats;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers
{
    internal class Vector4Handler : ValueHandler<Vector4>
    {
        public override byte[] Serialize(Vector4 value)
        {
            var data = new byte[16];
            Array.Copy(BitConverter.GetBytes(value.X), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(value.Y), 0, data, 4, 4);
            Array.Copy(BitConverter.GetBytes(value.Z), 0, data, 8, 4);
            Array.Copy(BitConverter.GetBytes(value.W), 0, data, 12, 4);
            return data;
        }

        public override Vector4 Parse(FieldDefinition def, string text)
        {
            var parts = text.Split(',');
            if (parts.Length != 4)
            {
                throw new FormatException("field type Vector4 requires 4 float values delimited by a comma");
            }

            if (Helpers.TryParseFloat32(parts[0], out var x) == false)
            {
                throw new FormatException("failed to parse Float X");
            }

            if (Helpers.TryParseFloat32(parts[1], out var y) == false)
            {
                throw new FormatException("failed to parse Float Y");
            }

            if (Helpers.TryParseFloat32(parts[2], out var z) == false)
            {
                throw new FormatException("failed to parse Float Z");
            }

            if (Helpers.TryParseFloat32(parts[3], out var w) == false)
            {
                throw new FormatException("failed to parse Float W");
            }

            return new Vector4(x, y, z, w);
        }

        public override Vector4 Deserialize(byte[] buffer, int offset, int count, out int read)
        {
            if (Helpers.HasLeft(buffer, offset, count, 16) == false)
            {
                throw new FormatException("field type Vector4 requires 16 bytes");
            }

            read = 16;
            return new Vector4
            {
                X = BitConverter.ToSingle(buffer, offset + 0),
                Y = BitConverter.ToSingle(buffer, offset + 4),
                Z = BitConverter.ToSingle(buffer, offset + 8),
                W = BitConverter.ToSingle(buffer, offset + 12),
            };
        }

        public override string Compose(FieldDefinition def, Vector4 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3}",
                value.X,
                value.Y,
                value.Z,
                value.W);
        }
    }
}
