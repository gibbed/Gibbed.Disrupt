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
        private Big.Platform _Platform;
        private byte _CompressionVersion;
        private byte _NameHashVersion;
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

            var platform = this._Platform;
            var compressionVersion = this._CompressionVersion;
            var nameHashVersion = this._NameHashVersion;

            if (IsKnownVersion(version, platform, compressionVersion, nameHashVersion) == false)
            {
                throw new FormatException("unknown version/platform/CV/NHV combination");
            }

            output.WriteValueU32(Signature, endian);
            output.WriteValueS32(version, endian);

            uint flags = 0;
            flags |= (uint)FromPlatform(platform) << 0;
            flags |= (uint)compressionVersion << 8;
            flags |= (uint)nameHashVersion << 16;
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
                SanityCheckEntry(entry, platform, compressionVersion);
            }

            this._Endian = endian;
            this._Version = version;
            this._Platform = platform;
            this._CompressionVersion = compressionVersion;
            this._NameHashVersion = nameHashVersion;
            this._Entries.Clear();
            this._Entries.AddRange(entries);
        }

        internal static void SanityCheckEntry(BigEntry entry, Big.Platform platform, byte compressionVersion)
        {
            var compressionScheme = ToCompressionScheme(entry.CompressionScheme, compressionVersion);

            if (compressionScheme == Big.CompressionScheme.None)
            {
                if (platform != Big.Platform.Xenon && entry.UncompressedSize != 0)
                {
                    throw new FormatException("got entry with no compression with a non-zero uncompressed size");
                }
            }
            else if (compressionScheme == Big.CompressionScheme.LZO1x ||
                     compressionScheme == Big.CompressionScheme.Zlib)
            {
                if (entry.CompressedSize == 0 && entry.UncompressedSize > 0)
                {
                    throw new FormatException(
                        "got entry with compression with a zero compressed size and a non-zero uncompressed size");
                }
            }
            else if (compressionScheme == Big.CompressionScheme.XMemCompress)
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
                case 1: return Big.Platform.Win32;
                case 2: return Big.Platform.Xenon;
                case 3: return Big.Platform.PS3;
                case 4: return Big.Platform.Win64;
                case 8: return Big.Platform.WiiU;
            }
            throw new NotSupportedException("unknown platform");
        }

        public static byte FromPlatform(Big.Platform platform)
        {
            switch (platform)
            {
                case Big.Platform.Any: return 0;
                case Big.Platform.Win32: return 1;
                case Big.Platform.Xenon: return 2;
                case Big.Platform.PS3: return 3;
                case Big.Platform.Win64: return 4;
                case Big.Platform.WiiU: return 8;
            }
            throw new NotSupportedException("unknown platform");
        }

        public static uint ComputeNameHash(string s)
        {
            if (s == null || s.Length == 0)
            {
                return 0xFFFFFFFFu;
            }
            return (uint)Hashing.FNV1a64.Compute(s.ToLowerInvariant());
        }

        public static bool TryParseNameHash(string s, out uint value)
        {
            return uint.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
        }

        public static string RenderNameHash(uint value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X8}", value);
        }

        uint Big.IArchive<uint>.ComputeNameHash(string s)
        {
            return ComputeNameHash(s);
        }

        bool Big.IArchive<uint>.TryParseNameHash(string s, out uint value)
        {
            return TryParseNameHash(s, out value);
        }

        string Big.IArchive<uint>.RenderNameHash(uint value)
        {
            return RenderNameHash(value);
        }

        private static Big.CompressionScheme ToCompressionScheme(byte id, byte version)
        {
            switch (version)
            {
                case 0: return Big.CompressionSchemeV0.ToCompressionScheme(id);
                case 4: return Big.CompressionSchemeV4.ToCompressionScheme(id);
                case 5: return Big.CompressionSchemeV5.ToCompressionScheme(id);
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

        private static Big.IEntrySerializer<uint> GetEntrySerializer(int version)
        {
            return _EntrySerializers.TryGetValue(version, out var entrySerializer) == true
                ? entrySerializer
                : throw new InvalidOperationException("entry serializer is missing");
        }

        private static readonly ReadOnlyCollection<ulong> _KnownVersions;
        private static readonly ReadOnlyDictionary<int, Big.IEntrySerializer<uint>> _EntrySerializers;

        static BigFileV3()
        {
            _KnownVersions = new ReadOnlyCollection<ulong>(new ulong[]
            {
                // Watch Dogs, Windows 64-bit
                MakeKnownVersion(8, Big.Platform.Any, 0, 50),
                MakeKnownVersion(8, Big.Platform.Win64, 5, 50),
                // Watch Dogs, Xbox 360 and PS3
                MakeKnownVersion(8, Big.Platform.Any, 0, 55),
                MakeKnownVersion(8, Big.Platform.Xenon, 5, 55),
                MakeKnownVersion(8, Big.Platform.PS3, 4, 55),
                // Watch Dogs, Wii U
                MakeKnownVersion(8, Big.Platform.Any, 0, 58),
                MakeKnownVersion(8, Big.Platform.WiiU, 5, 58),
            });

            _EntrySerializers = new ReadOnlyDictionary<int, Big.IEntrySerializer<uint>>(
                new Dictionary<int, Big.IEntrySerializer<uint>>()
            {
                [7] = new Big.EntrySerializerV07(),
                [8] = new Big.EntrySerializerV08(),
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
