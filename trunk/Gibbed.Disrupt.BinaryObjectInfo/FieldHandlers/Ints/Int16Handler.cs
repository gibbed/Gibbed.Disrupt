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
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers.Ints
{
    internal class Int16Handler : BaseHandler<short>
    {
        public override int MaximumBytes
        {
            get { return 2; }
        }

        protected override long ToInt64(short value)
        {
            return value;
        }

        public override short Parse(FieldDefinition def, string text)
        {
            short value;
            if (Helpers.TryParseInt16(text, out value) == false)
            {
                throw new FormatException("failed to parse Int16");
            }
            return value;
        }

        protected override short FromInt8(sbyte value)
        {
            return value;
        }

        protected override short FromInt16(short value)
        {
            return value;
        }

        protected override short FromInt32(int value)
        {
            return (short)value;
        }

        protected override short FromInt64(long value)
        {
            return (short)value;
        }

        public override string Compose(FieldDefinition def, short value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
