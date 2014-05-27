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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Disrupt.FileFormats
{
    public class BigFile
    {
        private const uint _Signature = 0x46415433; // 'FAT3'

        #region Fields
        private int _Version;
        private Big.Target _Target;
        private Big.Platform _Platform;
        private byte _Unknown70;
        private readonly List<Big.Entry> _Entries;
        #endregion

        public BigFile()
        {
            this._Entries = new List<Big.Entry>();
        }

        #region Properties
        public Endian Endian
        {
            get
            {
                return this.Target == Big.Target.Win32 ||
                       this.Target == Big.Target.Win64 ||
                       this.Target == Big.Target.Any
                           ? Endian.Little
                           : Endian.Big;
            }
        }

        public int Version
        {
            get { return this._Version; }
            set { this._Version = value; }
        }

        public Big.Target Target
        {
            get { return this._Target; }
            set { this._Target = value; }
        }

        public Big.Platform Platform
        {
            get { return this._Platform; }
            set { this._Platform = value; }
        }

        public byte Unknown70
        {
            get { return this._Unknown70; }
            set { this._Unknown70 = value; }
        }

        public List<Big.Entry> Entries
        {
            get { return this._Entries; }
        }
        #endregion

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != _Signature)
            {
                throw new FormatException("bad magic");
            }

            var version = input.ReadValueS32(Endian.Little);
            if (version < 7 || version > 8)
            {
                throw new FormatException("unsupported version");
            }

            var flags = input.ReadValueU32(Endian.Little);
            var target = (Big.Target)(flags & 0xFF);
            var platform = (Big.Platform)((flags >> 8) & 0xFF);
            var flags02 = (byte)((flags >> 16) & 0xFF);

            if ((flags & ~0xFFFFFFu) != 0)
            {
                throw new FormatException("unknown flags");
            }

            if (target != Big.Target.Any &&
                target != Big.Target.Win32 &&
                target != Big.Target.Xbox360 &&
                target != Big.Target.PS3 &&
                target != Big.Target.Win64)
            {
                throw new FormatException("unsupported or invalid platform");
            }

            if (IsValidTargetPlatform(target, platform, flags02) == false)
            {
                throw new FormatException("invalid flags");
            }

            this._Version = version;
            this._Target = target;
            this._Platform = platform;
            this._Unknown70 = flags02;

            var endian = this.Endian;
            var entrySerializer = this.GetEntrySerializer();

            this._Entries.Clear();
            uint entryCount = input.ReadValueU32(Endian.Little);
            for (uint i = 0; i < entryCount; i++)
            {
                Big.Entry entry;
                entrySerializer.Deserialize(input, endian, out entry);
                this._Entries.Add(entry);
            }

            uint localizationCount = input.ReadValueU32(Endian.Little);
            for (uint i = 0; i < localizationCount; i++)
            {
                throw new NotImplementedException();

                var nameLength = input.ReadValueU32(Endian.Little);
                if (nameLength > 32)
                {
                    throw new ArgumentOutOfRangeException();
                }
                var nameBytes = input.ReadBytes(nameLength);
                var unknownValue = input.ReadValueU64(Endian.Little);
            }

            foreach (var entry in this.Entries)
            {
                SanityCheckEntry(entry, target);
            }
        }

        internal static void SanityCheckEntry(Big.Entry entry, Big.Target platform)
        {
            if (entry.CompressionScheme == Big.CompressionScheme.None)
            {
                if (platform != Big.Target.Xbox360 &&
                    entry.UncompressedSize != 0)
                {
                    throw new FormatException("got entry with no compression with a non-zero uncompressed size");
                }
            }
            else if (entry.CompressionScheme == Big.CompressionScheme.LZO1x ||
                     entry.CompressionScheme == Big.CompressionScheme.Zlib)
            {
                if (entry.CompressedSize == 0 &&
                    entry.UncompressedSize > 0)
                {
                    throw new FormatException(
                        "got entry with compression with a zero compressed size and a non-zero uncompressed size");
                }
            }
            else if (entry.CompressionScheme == Big.CompressionScheme.Xbox)
            {
                if (entry.CompressedSize == 0 &&
                    entry.UncompressedSize > 0)
                {
                    throw new FormatException(
                        "got entry with compression with a zero compressed size and a non-zero uncompressed size");
                }
            }
            else
            {
                throw new FormatException("got entry with unsupported compression scheme");
            }
        }

        private static bool IsValidTargetPlatform(Big.Target target, Big.Platform platform, byte unknown70)
        {
            if (target == Big.Target.Any)
            {
                if (platform != Big.Platform.Any)
                {
                    return false;
                }

                if (unknown70 != 0x32)
                {
                    return false;
                }
            }
            else if (target == Big.Target.Win64)
            {
                if (platform != (Big.Platform)5)
                {
                    return false;
                }

                if (unknown70 != 0x32)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private Big.IEntrySerializer GetEntrySerializer()
        {
            if (this.Version >= _EntrySerializers.Count ||
                _EntrySerializers[this.Version] == null)
            {
                throw new InvalidOperationException("entry serializer is missing");
            }
            return _EntrySerializers[this.Version];
        }

        private static readonly ReadOnlyCollection<Big.IEntrySerializer> _EntrySerializers;

        static BigFile()
        {
            _EntrySerializers = new ReadOnlyCollection<Big.IEntrySerializer>(new Big.IEntrySerializer[]
            {
                null, // 0
                null, // 1
                null, // 2
                null, // 3
                null, // 4
                null, // 5
                null, // 6
                new Big.EntrySerializerV7(), // 7
                new Big.EntrySerializerV8(), // 8
            });
        }
    }
}
