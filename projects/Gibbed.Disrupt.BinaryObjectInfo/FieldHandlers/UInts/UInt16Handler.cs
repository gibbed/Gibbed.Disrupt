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

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers.UInts
{
    internal class UInt16Handler : BaseHandler<ushort>
    {
        public override int MaximumBytes
        {
            get { return 2; }
        }

        protected override ulong ToUInt64(ushort value)
        {
            return value;
        }

        public override ushort Parse(FieldDefinition def, string text)
        {
            if (Helpers.TryParseUInt16(text, out var value) == false)
            {
                throw new FormatException("failed to parse UInt16");
            }
            return value;
        }

        protected override ushort FromUInt8(byte value)
        {
            return value;
        }

        protected override ushort FromUInt16(ushort value)
        {
            return value;
        }

        protected override ushort FromUInt32(uint value)
        {
            return (ushort)value;
        }

        protected override ushort FromUInt64(ulong value)
        {
            return (ushort)value;
        }

        public override string Compose(FieldDefinition def, ushort value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
