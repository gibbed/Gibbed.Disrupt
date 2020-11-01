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
using System.IO;
using Gibbed.IO;

namespace Gibbed.Disrupt.FileFormats.Big
{
    internal class EntrySerializerV11 : IEntrySerializer<ulong>
    {
        // <a> hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
        // <a> hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
        // <b> oocccccc cccccccc cccccccc cccccccc
        // <c> oooooooo oooooooo oooooooo oooooooo
        // <d> uuuuuuuu uuuuuuuu uuuuuuuu uuuuuuss

        // [h] hash = 64 bits
        // [c] compressed size = 30 bits
        // [o] offset = 34 bits
        // [u] uncompressed size = 30 bits
        // [s] compression scheme = 2 bits

        public void Serialize(Stream output, Entry<ulong> entry, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian, out Entry<ulong> entry)
        {
            var a = input.ReadValueU64(endian);
            var b = input.ReadValueU32(endian);
            var c = input.ReadValueU32(endian);
            var d = input.ReadValueU32(endian);

            entry = new Entry<ulong>()
            {
                NameHash = a,
                CompressedSize = (int)((b >> 0) & 0x3FFFFFFFu),
                Offset = (long)c << 2 | ((b >> 30) & 0x3u),
                UncompressedSize = (int)((d >> 2) & 0x3FFFFFFFu),
                CompressionScheme = ToCompressionScheme((byte)((d >> 0) & 0x3u)),
            };
        }

        private static CompressionScheme ToCompressionScheme(byte id)
        {
            switch (id)
            {
                case 0: return CompressionScheme.None;
                case 1: return CompressionScheme.LZMA;
                case 2: return CompressionScheme.LZ4LW;
            }
            throw new NotSupportedException();
        }
    }
}
