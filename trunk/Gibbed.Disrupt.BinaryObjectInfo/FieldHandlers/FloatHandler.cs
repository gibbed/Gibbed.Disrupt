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
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers
{
    internal class FloatHandler : ValueHandler<float>
    {
        public override byte[] Serialize(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public override float Parse(FieldDefinition def, string text)
        {
            float value;
            if (Helpers.TryParseFloat32(text, out value) == false)
            {
                throw new FormatException("failed to parse Float");
            }
            return value;
        }

        public override float Deserialize(byte[] buffer, int offset, int count, out int read)
        {
            if (count == 0)
            {
                read = 0;
                return default(float);
            }

            if (count == 4)
            {
                read = 4;
                return BitConverter.ToSingle(buffer, offset);
            }

            throw new EndOfStreamException("bad size for Float");
        }

        public override string Compose(FieldDefinition def, float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
