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
using System.IO;

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers.UInts
{
    internal abstract class BaseHandler<T> : ValueHandler<T>
    {
        private static byte[] GetBytes(ulong dummy)
        {
            if (dummy <= byte.MaxValue)
            {
                return new[] { (byte)dummy };
            }

            if (dummy <= ushort.MaxValue)
            {
                return BitConverter.GetBytes((ushort)dummy);
            }

            if (dummy <= uint.MaxValue)
            {
                return BitConverter.GetBytes((uint)dummy);
            }

            return BitConverter.GetBytes(dummy);
        }

        public abstract int MaximumBytes { get; }

        protected abstract ulong ToUInt64(T value);

        public override byte[] Serialize(T value)
        {
            var bytes = GetBytes(this.ToUInt64(value));
            if (bytes.Length > this.MaximumBytes)
            {
                throw new InvalidOperationException();
            }
            return bytes;
        }

        protected abstract T FromUInt8(byte value);
        protected abstract T FromUInt16(ushort value);
        protected abstract T FromUInt32(uint value);
        protected abstract T FromUInt64(ulong value);

        public override T Deserialize(byte[] buffer, int offset, int count, out int read)
        {
            if (count == 0)
            {
                read = 0;
                return default(T);
            }

            if (count == 1)
            {
                read = 1;
                return this.FromUInt8(buffer[offset]);
            }

            if (count == 2)
            {
                read = 2;
                return this.FromUInt16(BitConverter.ToUInt16(buffer, offset));
            }

            if (count == 4)
            {
                read = 4;
                return this.FromUInt32(BitConverter.ToUInt32(buffer, offset));
            }

            if (count == 8)
            {
                read = 8;
                return this.FromUInt64(BitConverter.ToUInt64(buffer, offset));
            }

            throw new EndOfStreamException("bad size for UInt");
        }
    }
}
