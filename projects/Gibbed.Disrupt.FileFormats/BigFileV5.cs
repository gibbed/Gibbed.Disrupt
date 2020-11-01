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
using System.Linq;
using Gibbed.IO;
using BigEntry = Gibbed.Disrupt.FileFormats.Big.Entry<ulong>;

namespace Gibbed.Disrupt.FileFormats
{
    public class BigFileV5 : Big.IArchive<ulong>
    {
        public const uint Signature = 0x46415435; // 'FAT5'

        #region Fields
        private Endian _Endian;
        private int _Version;
        private byte _Target;
        private byte _Platform;
        private byte _Unknown70;
        private readonly List<BigEntry> _Entries;
        #endregion

        public BigFileV5()
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

        public List<BigEntry> Entries
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
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException("bad magic");
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var version = input.ReadValueS32(endian);
            if (version != 11 && version != 13)
            {
                throw new FormatException("unsupported version");
            }

            var flags = input.ReadValueU32(endian);
            var target = (byte)(flags & 0xFF);
            var platform = (byte)((flags >> 8) & 0xFF);
            var flags02 = (byte)((flags >> 16) & 0xFF);

            if ((version == 11 && flags != 0x00460000 && flags != 0x00460601) ||
                (version == 13 && flags != 0x00460801))
            {
                throw new FormatException("unknown flags");
            }

            /*
            if (target != 1)
            {
                throw new FormatException("unsupported or invalid platform");
            }
            */

            /*if (IsValidTargetPlatform(target, platform, flags02) == false)
            {
                throw new FormatException("invalid flags");
            }*/

            var unknown50 = input.ReadValueU64(endian); // probably a name hash of some sort

            var entrySerializer = GetEntrySerializer(version);

            var duplicateCount = input.ReadValueU32(endian);
            var duplicates = new KeyValuePair<ulong, ulong>[duplicateCount];
            for (uint i = 0; i < duplicateCount; i++)
            {
                // version >=13, 16 bytes (8+8)
                // version <13, 32 bytes (8+8+??)
                if (version == 13)
                {
                    var key = input.ReadValueU64(endian);
                    var value = input.ReadValueU64(endian);
                    duplicates[i] = new KeyValuePair<ulong, ulong>(key, value);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            var entryCount = input.ReadValueU32(endian);
            var entries = new List<BigEntry>();
            for (uint i = 0; i < entryCount; i++)
            {
                entrySerializer.Deserialize(input, endian, out var entry);
                entries.Add(entry);
            }

            if (version >= 12)
            {
                var unknownCount = input.ReadValueU32(endian);
                for (uint i = 0; i < unknownCount; i++)
                {
                    throw new NotImplementedException();
                }
            }

            var localizationCount = input.ReadValueU32(endian);
            for (uint i = 0; i < localizationCount; i++)
            {
                throw new NotImplementedException();
                var nameLength = input.ReadValueU32(endian);
                if (nameLength > 32)
                {
                    throw new FormatException("bad length for localization name");
                }
                var nameBytes = input.ReadBytes((int)nameLength);
                var unknownValue = input.ReadValueU64(endian);
            }

            foreach (var entry in entries)
            {
                SanityCheckEntry(entry, version, target);
            }

            this._Endian = endian;
            this._Version = version;
            this._Target = target;
            this._Platform = platform;
            this._Unknown70 = flags02;
            this._Entries.Clear();
            this._Entries.AddRange(entries);
        }

        internal static void SanityCheckEntry(BigEntry entry, int version, byte target)
        {
            if (entry.CompressionScheme == Big.CompressionScheme.None)
            {
                if (version < 12)
                {
                    if (entry.UncompressedSize != 0)
                    {
                        throw new FormatException("got entry with no compression with a non-zero uncompressed size");
                    }
                }
                else
                {
                    if (entry.UncompressedSize != entry.CompressedSize)
                    {
                        throw new FormatException("got entry with no compression with mismatched sizes");
                    }
                }
            }
            else if (entry.CompressionScheme == Big.CompressionScheme.LZ4LW)
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
            else if (target == 1) // Win64
            {
                if (platform != 8)
                {
                    return false;
                }

                if (unknown70 != 0x46)
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

        public static ulong ComputeNameHash(string s)
        {
            if (s == null || s.Length == 0)
            {
                return ulong.MaxValue;
            }
            var hash = Hashing.FNV1a64.Compute(s.ToLowerInvariant());
            if (hash == ulong.MaxValue)
            {
                return ulong.MaxValue;
            }
            hash &= 0x1FFFFFFFFFFFFFFFul;
            hash |= 0xA000000000000000ul;
            return hash;
        }

        public static bool TryParseNameHash(string s, out ulong value)
        {
            return ulong.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
        }

        public static string RenderNameHash(ulong value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X16}", value);
        }

        ulong Big.IArchive<ulong>.ComputeNameHash(string s)
        {
            return ComputeNameHash(s);
        }

        bool Big.IArchive<ulong>.TryParseNameHash(string s, out ulong value)
        {
            return TryParseNameHash(s, out value);
        }

        string Big.IArchive<ulong>.RenderNameHash(ulong value)
        {
            return RenderNameHash(value);
        }

        private static Big.IEntrySerializer<ulong> GetEntrySerializer(int version)
        {
            return _EntrySerializers.TryGetValue(version, out var entrySerializer) == true
                ? entrySerializer
                : throw new InvalidOperationException("entry serializer is missing");
        }

        private static readonly ReadOnlyDictionary<int, Big.IEntrySerializer<ulong>> _EntrySerializers;

        static BigFileV5()
        {
            _EntrySerializers = new ReadOnlyDictionary<int, Big.IEntrySerializer<ulong>>(
                new Dictionary<int, Big.IEntrySerializer<ulong>>()
                {
                    [11] = new Big.EntrySerializerV11(),
                    [13] = new Big.EntrySerializerV13(),
                });
        }
    }
}
