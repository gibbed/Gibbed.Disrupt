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

namespace Gibbed.Disrupt.FileFormats
{
    public struct Vector3 : ICloneable
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return $"{this.X},{this.Y},{this.Z}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return (Vector3)obj == this;
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return Equals(a.X, b.X) == false ||
                   Equals(a.Y, b.Y) == false ||
                   Equals(a.Z, b.Z) == false;
        }

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return Equals(a.X, b.X) == true &&
                   Equals(a.Y, b.Y) == true &&
                   Equals(a.Z, b.Z) == true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.X.GetHashCode();
                hash = hash * 23 + this.Y.GetHashCode();
                hash = hash * 23 + this.Z.GetHashCode();
                return hash;
            }
        }

        public object Clone()
        {
            return new Vector3(this.X, this.Y, this.Z);
        }
    }
}
