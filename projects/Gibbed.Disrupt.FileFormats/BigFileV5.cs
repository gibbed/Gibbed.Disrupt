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
using BigDependency = Gibbed.Disrupt.FileFormats.Big.Dependency<ulong>;
using BigEntry = Gibbed.Disrupt.FileFormats.Big.Entry<ulong>;

namespace Gibbed.Disrupt.FileFormats
{
    public class BigFileV5 : Big.IDependentArchive<ulong>
    {
        public const uint Signature = 0x46415435; // 'FAT5'

        #region Fields
        private Endian _Endian;
        private int _Version;
        private Big.Platform _Platform;
        private byte _CompressionVersion;
        private byte _NameHashVersion;
        private ulong _ArchiveHash;
        private readonly List<BigDependency> _Dependencies;
        private readonly List<BigEntry> _Entries;
        #endregion

        public BigFileV5()
        {
            this._Endian = Endian.Little;
            this._Dependencies = new List<BigDependency>();
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

        public Big.Platform Platform
        {
            get { return this._Platform; }
            set { this._Platform = value; }
        }

        public byte CompressionVersion
        {
            get { return this._CompressionVersion; }
            set { this._CompressionVersion = value; }
        }

        public byte NameHashVersion
        {
            get { return this._NameHashVersion; }
            set { this._NameHashVersion = value; }
        }

        public ulong ArchiveHash
        {
            get { return this._ArchiveHash; }
            set { this._ArchiveHash = value; }
        }

        public bool HasArchiveHash
        {
            get { return this.ArchiveHash != ulong.MaxValue; }
        }

        public List<BigDependency> Dependencies
        {
            get { return this._Dependencies; }
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
            var platform = ToPlatform((byte)(flags & 0xFF));
            var compressionVersion = (byte)((flags >> 8) & 0xFF);
            var nameHashVersion = (byte)((flags >> 16) & 0xFF);

            if ((flags & 0xFF000000u) != 0)
            {
                throw new FormatException("unknown flags");
            }

            if (IsKnownVersion(version, platform, compressionVersion, nameHashVersion) == false)
            {
                throw new FormatException("unknown version/platform/CV/NHV combination");
            }

            var archiveHash = input.ReadValueU64(endian);

            var entrySerializer = GetEntrySerializer(version);

            var dependencyCount = input.ReadValueU32(endian);
            var dependencies = new BigDependency[dependencyCount];
            for (uint i = 0; i < dependencyCount; i++)
            {
                // version >=13, 16 bytes (8+8)
                // version <13, 32 bytes (8+8+??)
                if (version == 13)
                {
                    var dependencyArchiveHash = input.ReadValueU64(endian);
                    var dependencyNameHash = input.ReadValueU64(endian);
                    dependencies[i] = new BigDependency(dependencyArchiveHash, dependencyNameHash);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            var entryCount = input.ReadValueU32(endian);
            var entries = new BigEntry[entryCount];
            for (uint i = 0; i < entryCount; i++)
            {
                entrySerializer.Deserialize(input, endian, out var entry);
                entries[i] = entry;
            }

            if (version >= 12)
            {
                var duplicateCount = input.ReadValueU32(endian);
                for (uint i = 0; i < duplicateCount; i++)
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
                SanityCheckEntry(entry, version, platform, compressionVersion);
            }

            this._Endian = endian;
            this._Version = version;
            this._Platform = platform;
            this._CompressionVersion = compressionVersion;
            this._NameHashVersion = nameHashVersion;
            this._ArchiveHash = archiveHash;
            this._Dependencies.Clear();
            this._Dependencies.AddRange(dependencies);
            this._Entries.Clear();
            this._Entries.AddRange(entries);
        }

        internal static void SanityCheckEntry(BigEntry entry, int version, Big.Platform platform, byte compressionVersion)
        {
            var compressionScheme = ToCompressionScheme(entry.CompressionScheme, compressionVersion);

            if (compressionScheme == Big.CompressionScheme.None)
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
            else if (compressionScheme == Big.CompressionScheme.LZ4LW)
            {
                if (entry.CompressedSize == 0 && entry.UncompressedSize > 0)
                {
                    throw new FormatException(
                        "got entry with compression with a zero compressed size and a non-zero uncompressed size");
                }
            }
            else if (compressionScheme == Big.CompressionScheme.LZMA)
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

        private static bool IsKnownVersion(
            int version,
            Big.Platform platform,
            byte compressionVersion,
            byte nameHashVersion)
        {
            var value = MakeKnownVersion(version, platform, compressionVersion, nameHashVersion);
            return _KnownVersions.Contains(value) == true;
        }

        public static Big.Platform ToPlatform(byte id)
        {
            switch (id)
            {
                case 0: return Big.Platform.Any;
                case 1: return Big.Platform.Win64;
                case 3: return Big.Platform.Orbis;
            }
            throw new NotSupportedException("unknown platform");
        }

        public static byte FromPlatform(Big.Platform platform)
        {
            switch (platform)
            {
                case Big.Platform.Any: return 0;
                case Big.Platform.Win64: return 1;
                case Big.Platform.Orbis: return 3;
            }
            throw new NotSupportedException("unknown platform");
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

        private static Big.CompressionScheme ToCompressionScheme(byte id, byte version)
        {
            switch (version)
            {
                case 0: return Big.CompressionSchemeV0.ToCompressionScheme(id);
                case 6: return Big.CompressionSchemeV6.ToCompressionScheme(id);
                case 8: return Big.CompressionSchemeV8.ToCompressionScheme(id);
                case 9: return Big.CompressionSchemeV9.ToCompressionScheme(id);
            }
            throw new NotSupportedException();
        }

        public Big.CompressionScheme ToCompressionScheme(byte id)
        {
            return ToCompressionScheme(id, this._CompressionVersion);
        }

        public byte FromCompressionSCheme(Big.CompressionScheme compressionScheme)
        {
            throw new NotImplementedException();
        }

        private static Big.IEntrySerializer<ulong> GetEntrySerializer(int version)
        {
            return _EntrySerializers.TryGetValue(version, out var entrySerializer) == true
                ? entrySerializer
                : throw new InvalidOperationException("entry serializer is missing");
        }

        private static readonly ReadOnlyCollection<ulong> _KnownVersions;
        private static readonly ReadOnlyDictionary<int, Big.IEntrySerializer<ulong>> _EntrySerializers;

        static BigFileV5()
        {
            _KnownVersions = new ReadOnlyCollection<ulong>(new ulong[]
            {
                // Watch Dogs 2
                MakeKnownVersion(11, Big.Platform.Any, 0, 70),
                MakeKnownVersion(11, Big.Platform.Win64, 6, 70),
                MakeKnownVersion(11, Big.Platform.Orbis, 9, 70),
                // Watch Dogs: Legion
                MakeKnownVersion(13, Big.Platform.Win64, 8, 70),
            });

            _EntrySerializers = new ReadOnlyDictionary<int, Big.IEntrySerializer<ulong>>(
                new Dictionary<int, Big.IEntrySerializer<ulong>>()
                {
                    [11] = new Big.EntrySerializerV11(),
                    [13] = new Big.EntrySerializerV13(),
                });
        }

        private static ulong MakeKnownVersion(int version, Big.Platform platform, byte compressionVersion, byte nameHashVersion)
        {
            ulong value = 0;
            value |= (uint)version;
            value |= ((ulong)compressionVersion) << 32;
            value |= ((ulong)nameHashVersion) << 40;
            value |= ((ulong)platform) << 48;
            return value;
        }
    }
}
