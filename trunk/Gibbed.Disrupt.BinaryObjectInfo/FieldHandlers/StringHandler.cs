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
using System.Text;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers
{
    internal class StringHandler : ValueHandler<string>
    {
        public override byte[] Serialize(string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            Array.Resize(ref data, data.Length + 1);
            return data;
        }

        public override string Parse(FieldDefinition def, string text)
        {
            return text;
        }

        public override string Deserialize(byte[] buffer, int offset, int count, out int read)
        {
            if (Helpers.HasLeft(buffer, offset, count, 1) == false)
            {
                throw new FormatException("String requires at least 1 byte");
            }

            int length, o;
            for (length = 0, o = offset; buffer[o] != 0 && o < buffer.Length; length++, o++)
            {
            }

            if (o == buffer.Length)
            {
                throw new FormatException("invalid trailing byte value for field type String");
            }

            /*
            if (buffer[buffer.Length - 1] != 0)
            {
                throw new FormatException("invalid trailing byte value for field type String");
            }
            */

            read = length + 1;
            return Encoding.UTF8.GetString(buffer, offset, length);
        }

        public override string Compose(FieldDefinition def, string value)
        {
            return value;
        }
    }
}
