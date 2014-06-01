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

namespace Gibbed.Disrupt.FileFormats
{
    public struct Vector4 : ICloneable
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}",
                                 this.X,
                                 this.Y,
                                 this.Z,
                                 this.W);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return (Vector4)obj == this;
        }

        public static bool operator !=(Vector4 a, Vector4 b)
        {
            return Equals(a.X, b.X) == false ||
                   Equals(a.Y, b.Y) == false ||
                   Equals(a.Z, b.Z) == false ||
                   Equals(a.W, b.W) == false;
        }

        public static bool operator ==(Vector4 a, Vector4 b)
        {
            return Equals(a.X, b.X) == true &&
                   Equals(a.Y, b.Y) == true &&
                   Equals(a.Z, b.Z) == true &&
                   Equals(a.W, b.W) == true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.X.GetHashCode();
                hash = hash * 23 + this.Y.GetHashCode();
                hash = hash * 23 + this.Z.GetHashCode();
                hash = hash * 23 + this.W.GetHashCode();
                return hash;
            }
        }

        public object Clone()
        {
            return new Vector4(this.X, this.Y, this.Z, this.W);
        }
    }
}
