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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Gibbed.IO;
using BigEntry = Gibbed.Disrupt.FileFormats.Big.Entry<uint>;

namespace Gibbed.Disrupt.FileFormats
{
    public class BigFileV3 : Big.IArchive<uint>
    {
        public const uint Signature = 0x46415433; // 'FAT3'

        #region Fields
        private Endian _Endian;
        private int _Version;
        private byte _Target;
        private byte _Platform;
        private byte _Unknown70;
        private readonly List<BigEntry> _Entries;
        #endregion

        public BigFileV3()
        {
            this._Endian = Endian.Little;
            this._Entries = new List<BigEntry>();
        }

        #region Properties
        public Endian Endian
        {
            get { return this._Endian; }
            set { this._Endian = value; }
        }

        public int Version
        {
            get { return this._Version; }
            set { this._Version = value; }
        }

        // Any = 0
        // Win32 = 1
        // Xbox360 = 2
        // PS3 = 3
        // Win64 = 4
        public byte Target
        {
            get { return this._Target; }
            set { this._Target = value; }
        }

        public byte Platform
        {
            get { return this._Platform; }
            set { this._Platform = value; }
        }

        public byte Unknown70
        {
            get { return this._Unknown70; }
            set { this._Unknown70 = value; }
        }

        public List<BigEntry> Entries => this._Entries;
        #endregion

        public void Serialize(Stream output)
        {
            var endian = this.Endian;
            var version = this.Version;

            if (version < 7 || version > 8)
            {
                throw new FormatException("unsupported version");
            }

            var target = this._Target;
            var platform = this._Platform;
            var flags02 = this._Unknown70;

            if (target != 0 && // Any
                target != 1 && // Win32
                target != 2 && // Xbox360
                target != 3 && // PS3
                target != 4) // Win64
            {
                throw new FormatException("unsupported or invalid platform");
            }

            if (IsValidTargetPlatform(target, platform, flags02) == false)
            {
                throw new FormatException("invalid flags");
            }

            output.WriteValueU32(Signature, endian);
            output.WriteValueS32(version, endian);

            uint flags = 0;
            flags |= (uint)target << 0;
            flags |= (uint)platform << 8;
            flags |= (uint)flags02 << 16;
            output.WriteValueU32(flags, endian);

            var entrySerializer = GetEntrySerializer(version);
            output.WriteValueS32(this.Entries.Count, endian);
            foreach (var entry in this.Entries)
            {
                entrySerializer.Serialize(output, entry, endian);
            }

            output.WriteValueU32(0, endian);
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException("bad magic");
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var version = input.ReadValueS32(endian);
            if (version < 7 || version > 8)
            {
                throw new FormatException("unsupported version");
            }

            var flags = input.ReadValueU32(endian);
            var target = (byte)(flags & 0xFF);
            var platform = (byte)((flags >> 8) & 0xFF);
            var flags02 = (byte)((flags >> 16) & 0xFF);

            if ((flags & ~0xFFFFFFu) != 0)
            {
                throw new FormatException("unknown flags");
            }

            if (target != 0 && // Any
                target != 1 && // Win32
                target != 2 && // Xbox360
                target != 3 && // PS3
                target != 4) // Win64
            {
                throw new FormatException("unsupported or invalid platform");
            }

            if (IsValidTargetPlatform(target, platform, flags02) == false)
            {
                throw new FormatException("invalid flags");
            }

            var entrySerializer = GetEntrySerializer(version);

            var entryCount = input.ReadValueU32(endian);
            var entries = new List<BigEntry>();
            for (uint i = 0; i < entryCount; i++)
            {
                entrySerializer.Deserialize(input, endian, out var entry);
                entries.Add(entry);
            }

            uint localizationCount = input.ReadValueU32(endian);
            for (uint i = 0; i < localizationCount; i++)
            {
                var nameLength = input.ReadValueU32(endian);
                if (nameLength > 32)
                {
                    throw new FormatException("bad length for localization name");
                }
                var nameBytes = input.ReadBytes((int)nameLength);
                var unknownValue = input.ReadValueU64(endian);
            }

            foreach (var entry in this.Entries)
            {
                SanityCheckEntry(entry, target);
            }

            this._Endian = endian;
            this._Version = version;
            this._Target = target;
            this._Platform = platform;
            this._Unknown70 = flags02;
            this._Entries.Clear();
            this._Entries.AddRange(entries);
        }

        internal static void SanityCheckEntry(BigEntry entry, byte platform)
        {
            if (entry.CompressionScheme == Big.CompressionScheme.None)
            {
                if (platform != 2 && entry.UncompressedSize != 0)
                {
                    throw new FormatException("got entry with no compression with a non-zero uncompressed size");
                }
            }
            else if (entry.CompressionScheme == Big.CompressionScheme.LZO1x ||
                     entry.CompressionScheme == Big.CompressionScheme.Zlib)
            {
                if (entry.CompressedSize == 0 && entry.UncompressedSize > 0)
                {
                    throw new FormatException(
                        "got entry with compression with a zero compressed size and a non-zero uncompressed size");
                }
            }
            else if (entry.CompressionScheme == Big.CompressionScheme.XMemCompress)
            {
                if (entry.CompressedSize == 0 && entry.UncompressedSize > 0)
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

        private static bool IsValidTargetPlatform(byte target, byte platform, byte unknown70)
        {
            if (target == 0) // Any
            {
                if (platform != 0) // Any
                {
                    return false;
                }

                if (unknown70 != 0x32)
                {
                    return false;
                }
            }
            else if (target == 4) // Win64
            {
                if (platform != 5)
                {
                    return false;
                }

                if (unknown70 != 0x32)
                {
                    return false;
                }
            }
            else if (target == 2) // Xbox360
            {
                if (platform != 5)
                {
                    return false;
                }

                if (unknown70 != 0x37)
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

        public uint ComputeNameHash(string s)
        {
            if (s == null || s.Length == 0)
            {
                return 0xFFFFFFFFu;
            }
            return (uint)Hashing.FNV1a64.Compute(s.ToLowerInvariant());
        }

        public bool TryParseNameHash(string s, out uint value)
        {
            return uint.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
        }

        public string RenderNameHash(uint value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X8}", value);
        }

        private static Big.IEntrySerializer<uint> GetEntrySerializer(int version)
        {
            return _EntrySerializers.TryGetValue(version, out var entrySerializer) == true
                ? entrySerializer
                : throw new InvalidOperationException("entry serializer is missing");
        }

        private static readonly ReadOnlyDictionary<int, Big.IEntrySerializer<uint>> _EntrySerializers;

        static BigFileV3()
        {
            _EntrySerializers = new ReadOnlyDictionary<int, Big.IEntrySerializer<uint>>(
                new Dictionary<int, Big.IEntrySerializer<uint>>()
            {
                [7] = new Big.EntrySerializerV07(),
                [8] = new Big.EntrySerializerV08(),
            });
        }
    }
}
