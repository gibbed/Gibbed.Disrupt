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
    internal class Vector2Handler : ValueHandler<Vector2>
    {
        public override byte[] Serialize(Vector2 value)
        {
            var data = new byte[8];
            Array.Copy(BitConverter.GetBytes(value.X), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(value.Y), 0, data, 4, 4);
            return data;
        }

        public override Vector2 Parse(FieldDefinition def, string text)
        {
            var parts = text.Split(',');
            if (parts.Length != 2)
            {
                throw new FormatException("field type Vector2 requires 2 float values delimited by a comma");
            }

            if (Helpers.TryParseFloat32(parts[0], out var x) == false)
            {
                throw new FormatException("failed to parse Float X");
            }

            if (Helpers.TryParseFloat32(parts[1], out var y) == false)
            {
                throw new FormatException("failed to parse Float Y");
            }

            return new Vector2(x, y);
        }

        public override Vector2 Deserialize(byte[] buffer, int offset, int count, out int read)
        {
            if (Helpers.HasLeft(buffer, offset, count, 8) == false)
            {
                throw new System.IO.EndOfStreamException("Vector2 requires 8 bytes");
            }

            read = 8;
            return new Vector2
            {
                X = BitConverter.ToSingle(buffer, offset + 0),
                Y = BitConverter.ToSingle(buffer, offset + 4),
            };
        }

        public override string Compose(FieldDefinition def, Vector2 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1}",
                value.X,
                value.Y);
        }
    }
}
