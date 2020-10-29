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

namespace Gibbed.Disrupt.BinaryObjectInfo.FieldHandlers.Ints
{
    internal abstract class BaseHandler<T> : ValueHandler<T>
    {
        private static byte[] GetBytes(long value)
        {
            if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                return new[] { (byte)((sbyte)value) };
            }

            if (value >= short.MinValue && value <= short.MaxValue)
            {
                return BitConverter.GetBytes((short)value);
            }

            if (value >= int.MinValue && value <= int.MaxValue)
            {
                return BitConverter.GetBytes((int)value);
            }

            return BitConverter.GetBytes(value);
        }

        public abstract int MaximumBytes { get; }

        protected abstract long ToInt64(T value);

        public override byte[] Serialize(T value)
        {
            var bytes = GetBytes(this.ToInt64(value));
            if (bytes.Length > this.MaximumBytes)
            {
                throw new InvalidOperationException("too many bytes for type");
            }
            return bytes;
        }

        protected abstract T FromInt8(sbyte value);
        protected abstract T FromInt16(short value);
        protected abstract T FromInt32(int value);
        protected abstract T FromInt64(long value);

        public override T Deserialize(byte[] buffer, int offset, int count, out int read)
        {
            if (count > this.MaximumBytes)
            {
                throw new InvalidOperationException("too many bytes for type");
            }

            if (count == 0)
            {
                read = 0;
                return default;
            }

            if (count == 1)
            {
                read = 1;
                return this.FromInt8((sbyte)buffer[offset]);
            }

            if (count == 2)
            {
                read = 2;
                return this.FromInt16(BitConverter.ToInt16(buffer, offset));
            }

            if (count == 4)
            {
                read = 4;
                return this.FromInt32(BitConverter.ToInt32(buffer, offset));
            }

            if (count == 8)
            {
                read = 8;
                return this.FromInt64(BitConverter.ToInt64(buffer, offset));
            }

            throw new EndOfStreamException("bad size for Int");
        }
    }
}
