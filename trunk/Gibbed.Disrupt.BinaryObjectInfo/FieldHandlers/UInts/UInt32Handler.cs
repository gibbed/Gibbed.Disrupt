﻿/* Copyright (c) 2014 Rick (rick 'at' gibbed 'dot' us)
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
    internal class UInt32Handler : BaseHandler<uint>
    {
        public override int MaximumBytes
        {
            get { return 4; }
        }

        protected override ulong ToUInt64(uint value)
        {
            return value;
        }

        public override uint Parse(FieldDefinition def, string text)
        {
            uint value;
            if (Helpers.TryParseUInt32(text, out value) == false)
            {
                throw new FormatException("failed to parse UInt32");
            }
            return value;
        }

        protected override uint FromUInt8(byte value)
        {
            return value;
        }

        protected override uint FromUInt16(ushort value)
        {
            return value;
        }

        protected override uint FromUInt32(uint value)
        {
            return value;
        }

        protected override uint FromUInt64(ulong value)
        {
            return (uint)value;
        }

        public override string Compose(FieldDefinition def, uint value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
