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
    public struct Dependency<T> : IEquatable<Dependency<T>>
    {
        public T ArchiveHash { get; set; }
        public T NameHash { get; set; }

        public Dependency(T archiveHash, T nameHash)
        {
            this.ArchiveHash = archiveHash;
            this.NameHash = nameHash;
        }

        public override string ToString()
        {
            return $"{this.ArchiveHash:X} {this.NameHash:X}";
        }

        public bool Equals(Dependency<T> other)
        {
            return this.ArchiveHash.Equals(other.ArchiveHash) == true &&
                   this.NameHash.Equals(other.NameHash) == true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return (Dependency<T>)obj == this;
        }

        public static bool operator ==(Dependency<T> a, Dependency<T> b)
        {
            return a.Equals(b) == true;
        }

        public static bool operator !=(Dependency<T> a, Dependency<T> b)
        {
            return a.Equals(b) == false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.ArchiveHash.GetHashCode();
                hash = hash * 23 + this.NameHash.GetHashCode();
                return hash;
            }
        }
    }
}
