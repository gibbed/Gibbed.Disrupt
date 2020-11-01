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

namespace Gibbed.Disrupt.FileFormats.Big
{
    public struct Entry<T> : IEquatable<Entry<T>>, IEntry
    {
        public T NameHash { get; set; }
        public int UncompressedSize { get; set; }
        public long Offset { get; set; }
        public int CompressedSize { get; set; }
        public CompressionScheme CompressionScheme { get; set; }

        public override string ToString()
        {
            return this.CompressionScheme == CompressionScheme.None
                ? $"{this.NameHash:X} @{this.Offset}, {this.CompressedSize} bytes"
                : $"{this.NameHash:X} @{this.Offset}, {this.UncompressedSize} bytes ({this.CompressedSize} compressed bytes, scheme {this.CompressionScheme})";
        }

        public bool Equals(Entry<T> other)
        {
            return this.NameHash.Equals(other) == true &&
                   this.UncompressedSize == other.UncompressedSize &&
                   this.Offset == other.Offset &&
                   this.CompressedSize == other.CompressedSize &&
                   this.CompressionScheme == other.CompressionScheme;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return (Entry<T>)obj == this;
        }

        public static bool operator ==(Entry<T> a, Entry<T> b)
        {
            return a.Equals(b) == true;
        }

        public static bool operator !=(Entry<T> a, Entry<T> b)
        {
            return a.Equals(b) == false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.NameHash.GetHashCode();
                hash = hash * 23 + this.UncompressedSize.GetHashCode();
                hash = hash * 23 + this.Offset.GetHashCode();
                hash = hash * 23 + this.CompressedSize.GetHashCode();
                hash = hash * 23 + this.CompressionScheme.GetHashCode();
                return hash;
            }
        }
    }
}
