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
    internal class EntrySerializerV08 : IEntrySerializer<uint>
    {
        // hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
        // uuuuuuuu uuuuuuuu uuuuuuuu uuuuusss
        // oooccccc cccccccc cccccccc cccccccc
        // oooooooo oooooooo oooooooo oooooooo

        // [h] hash = 32 bits
        // [u] uncompressed size = 29 bits
        // [s] compression scheme = 3 bits
        // [o] offset = 35 bits
        // [c] compressed size = 29 bits

        public void Serialize(Stream output, Entry<uint> entry, Endian endian)
        {
            var a = entry.NameHash;

            uint b = 0;
            b |= ((uint)entry.UncompressedSize & 0x1FFFFFFFu) << 3;
            b |= (uint)((FromCompressionScheme(entry.CompressionScheme) << 0) & 0x00000007u);

            uint c = 0;
            c |= (uint)(entry.Offset & 0x00000007u) << 29;
            c |= ((uint)entry.CompressedSize & 0x1FFFFFFFu) << 0;

            var d = (uint)((entry.Offset >> 3) & 0x1FFFFFFF);

            output.WriteValueU32(a, endian);
            output.WriteValueU32(b, endian);
            output.WriteValueU32(c, endian);
            output.WriteValueU32(d, endian);
        }

        public void Deserialize(Stream input, Endian endian, out Entry<uint> entry)
        {
            var a = input.ReadValueU32(endian);
            var b = input.ReadValueU32(endian);
            var c = input.ReadValueU32(endian);
            var d = input.ReadValueU32(endian);

            entry = new Entry<uint>()
            {
                NameHash = a,
                UncompressedSize = (int)((b >> 3) & 0x1FFFFFFFu),
                Offset = (long)d << 3 | ((c >> 29) & 0x7u),
                CompressionScheme = ToCompressionScheme((byte)((b >> 0) & 0x7u)),
                CompressedSize = (int)((c >> 0) & 0x1FFFFFFFu),
            };
        }

        private static byte FromCompressionScheme(CompressionScheme scheme)
        {
            switch (scheme)
            {
                case CompressionScheme.None: return 0;
                case CompressionScheme.LZO1x: return 1;
                case CompressionScheme.Zlib: return 2;
                case CompressionScheme.XMemCompress: return 3;
            }
            throw new NotSupportedException();
        }

        private static CompressionScheme ToCompressionScheme(byte id)
        {
            switch (id)
            {
                case 0: return CompressionScheme.None;
                case 1: return CompressionScheme.LZO1x;
                case 2: return CompressionScheme.Zlib;
                case 3: return CompressionScheme.XMemCompress;
            }
            throw new NotSupportedException();
        }
    }
}
