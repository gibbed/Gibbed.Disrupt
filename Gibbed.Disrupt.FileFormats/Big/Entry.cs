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

namespace Gibbed.Disrupt.FileFormats.Big
{
    public struct Entry
    {
        public uint NameHash;
        public uint UncompressedSize;
        public uint CompressedSize;
        public long Offset;
        public CompressionScheme CompressionScheme;

        public override string ToString()
        {
            if (this.CompressionScheme == CompressionScheme.None)
            {
                return string.Format("{0:X8} @ {1}, {2} bytes",
                                     this.NameHash,
                                     this.Offset,
                                     this.CompressedSize);
            }

            return string.Format("{0:X8} @ {1}, {2} bytes ({3} compressed bytes, scheme {4})",
                                 this.NameHash,
                                 this.Offset,
                                 this.UncompressedSize,
                                 this.CompressedSize,
                                 this.CompressionScheme);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return (Entry)obj == this;
        }

        public static bool operator ==(Entry a, Entry b)
        {
            return a.NameHash == b.NameHash &&
                   a.UncompressedSize == b.UncompressedSize &&
                   a.CompressedSize == b.CompressedSize &&
                   a.Offset == b.Offset &&
                   a.CompressionScheme == b.CompressionScheme;
        }

        public static bool operator !=(Entry a, Entry b)
        {
            return a.NameHash != b.NameHash ||
                   a.UncompressedSize != b.UncompressedSize ||
                   a.CompressedSize != b.CompressedSize ||
                   a.Offset != b.Offset ||
                   a.CompressionScheme != b.CompressionScheme;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.NameHash.GetHashCode();
                hash = hash * 23 + this.UncompressedSize.GetHashCode();
                hash = hash * 23 + this.CompressedSize.GetHashCode();
                hash = hash * 23 + this.Offset.GetHashCode();
                hash = hash * 23 + this.CompressionScheme.GetHashCode();
                return hash;
            }
        }
    }
}
