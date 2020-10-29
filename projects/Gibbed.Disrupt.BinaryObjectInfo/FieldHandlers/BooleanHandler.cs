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
using System.IO;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers
{
    internal class BooleanHandler : ValueHandler<bool>
    {
        public override byte[] Serialize(bool value)
        {
            if (value == false)
            {
                return new byte[0];
            }

            return new[] { (byte)1 };
        }

        public override bool Parse(FieldDefinition def, string text)
        {
            if (bool.TryParse(text, out var value) == false)
            {
                throw new FormatException("failed to parse Boolean");
            }
            return value;
        }

        public override bool Deserialize(byte[] buffer, int offset, int count, out int read)
        {
            if (count == 0)
            {
                read = 0;
                return false;
            }

            if (count == 1)
            {
                if (buffer[offset] != 0 &&
                    buffer[offset] != 1)
                {
                    throw new FormatException("invalid value for Boolean");
                }

                read = 1;
                return buffer[offset] != 0;
            }

            throw new EndOfStreamException("bad size for Boolean");
        }

        public override string Compose(FieldDefinition def, bool value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
