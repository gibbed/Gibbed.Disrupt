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

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers.Ids
{
    internal abstract class Base64Handler : Ints.Int64Handler
    {
        protected abstract ulong Hash(string text);

        public override long Parse(FieldDefinition def, string text)
        {
            if (text.StartsWith("0x") == false)
            {
                return (long)this.Hash(text);
            }
            if (ulong.TryParse(
                text.Substring(2),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture,
                out var value) == false)
            {
                throw new FormatException("failed to parse hex Id");
            }
            return (long)value;
        }

        public override string Compose(FieldDefinition def, long value)
        {
            return "0x" + ((ulong)value).ToString("X16", CultureInfo.InvariantCulture);
        }
    }
}
