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
using Gibbed.Disrupt.FileFormats.Big;
using Gibbed.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using LZO = MiniLZO.LZO;
using LZOErrorCode = MiniLZO.ErrorCode;

namespace Gibbed.Disrupt.Packing
{
    internal static class EntryDecompression
    {
        public static void Decompress<T>(IArchive<T> archive, IEntry entry, Stream input, Stream output)
        {
            input.Seek(entry.Offset, SeekOrigin.Begin);

            var compressionScheme = archive.ToCompressionScheme(entry.CompressionScheme);
            if (compressionScheme == CompressionScheme.None)
            {
                output.WriteFromStream(input, entry.CompressedSize);
            }
            else if (compressionScheme == CompressionScheme.LZO1x)
            {
                DecompressLZO(entry, input, output);
            }
            else if (compressionScheme == CompressionScheme.Zlib)
            {
                DecompressZlib(entry, input, output);
            }
            else if (compressionScheme == CompressionScheme.XMemCompress)
            {
                DecompressXMemCompress(entry, input, output);
            }
            else if (compressionScheme == CompressionScheme.LZ4LW)
            {
                DecompressLZ4LW(entry, input, output);
            }
            else
            {
                throw new NotImplementedException("unimplemented compression scheme");
            }
        }

        private static void DecompressLZO(IEntry entry, Stream input, Stream output)
        {
            var compressedBytes = new byte[entry.CompressedSize];
            if (input.Read(compressedBytes, 0, compressedBytes.Length) != compressedBytes.Length)
            {
                throw new EndOfStreamException("could not read all compressed bytes");
            }

            var uncompressedBytes = new byte[entry.UncompressedSize];
            int actualUncompressedLength = uncompressedBytes.Length;

            var result = LZO.Decompress(
                compressedBytes,
                0,
                compressedBytes.Length,
                uncompressedBytes,
                0,
                ref actualUncompressedLength);
            if (result != LZOErrorCode.Success)
            {
                throw new InvalidOperationException($"LZO decompression failure ({result})");
            }

            if (actualUncompressedLength != uncompressedBytes.Length)
            {
                throw new InvalidOperationException("LZO decompression failure (uncompressed size mismatch)");
            }

            output.Write(uncompressedBytes, 0, uncompressedBytes.Length);
        }

        private static void DecompressZlib(IEntry entry, Stream input, Stream output)
        {
            if (entry.CompressedSize < 16)
            {
                throw new EndOfStreamException("not enough data for zlib compressed data");
            }

            var sizes = new ushort[8];
            for (int i = 0; i < 8; i++)
            {
                sizes[i] = input.ReadValueU16(Endian.Little);
            }

            var blockCount = sizes[0];
            var maximumUncompressedBlockSize = 16 * (sizes[1] + 1);

            long left = entry.UncompressedSize;
            for (int i = 0, c = 2; i < blockCount; i++, c++)
            {
                if (c == 8)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        sizes[j] = input.ReadValueU16(Endian.Little);
                    }

                    c = 0;
                }

                uint compressedBlockSize = sizes[c];
                if (compressedBlockSize != 0)
                {
                    var uncompressedBlockSize = i + 1 < blockCount
                        ? Math.Min(maximumUncompressedBlockSize, left)
                        : left;
                    //var uncompressedBlockSize = Math.Min(maximumUncompressedBlockSize, left);

                    using (var temp = input.ReadToMemoryStream((int)compressedBlockSize))
                    {
                        var zlib = new InflaterInputStream(temp, new Inflater(true));
                        output.WriteFromStream(zlib, uncompressedBlockSize);
                        left -= uncompressedBlockSize;
                    }

                    var padding = (16 - (compressedBlockSize % 16)) % 16;
                    if (padding > 0)
                    {
                        input.Seek(padding, SeekOrigin.Current);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (left > 0)
            {
                throw new InvalidOperationException("did not decompress enough data");
            }
        }

        private static void DecompressXMemCompress(IEntry entry, Stream input, Stream output)
        {
            var magic = input.ReadValueU32(Endian.Big);
            if (magic != 0x0FF512EE)
            {
                throw new FormatException("invalid magic");
            }

            var version = input.ReadValueU32(Endian.Big);
            if (version != 0x01030000)
            {
                throw new FormatException("invalid version");
            }

            var unknown08 = input.ReadValueU32(Endian.Big);
            if (unknown08 != 0)
            {
                throw new FormatException("don't know how to handle a non-zero unknown08");
            }

            var unknown0C = input.ReadValueU32(Endian.Big);
            if (unknown0C != 0)
            {
                throw new FormatException("don't know how to handle a non-zero unknown0C");
            }

            var windowSize = input.ReadValueU32(Endian.Big);
            var chunkSize = input.ReadValueU32(Endian.Big);

            var uncompressedSize = input.ReadValueS64(Endian.Big);
            var compressedSize = input.ReadValueS64(Endian.Big);
            var largestUncompressedChunkSize = input.ReadValueS32(Endian.Big);
            var largestCompressedChunkSize = input.ReadValueS32(Endian.Big);

            if (uncompressedSize < 0 ||
                compressedSize < 0 ||
                largestUncompressedChunkSize < 0 ||
                largestCompressedChunkSize < 0)
            {
                throw new FormatException("bad size value");
            }

            if (uncompressedSize != entry.UncompressedSize)
            {
                throw new InvalidOperationException("uncompressed size mismatch");
            }

            var uncompressedBytes = new byte[largestUncompressedChunkSize];
            var compressedBytes = new byte[largestCompressedChunkSize];

            var remaining = uncompressedSize;
            while (remaining > 0)
            {
                using (var context = new XCompression.DecompressionContext(windowSize, chunkSize))
                {
                    var compressedChunkSize = input.ReadValueS32(Endian.Big);
                    if (compressedChunkSize < 0 ||
                        compressedChunkSize > largestCompressedChunkSize)
                    {
                        throw new InvalidOperationException("compressed size mismatch");
                    }

                    if (input.Read(compressedBytes, 0, compressedChunkSize) != compressedChunkSize)
                    {
                        throw new EndOfStreamException("could not read all compressed bytes");
                    }

                    var uncompressedChunkSize = (int)Math.Min(largestUncompressedChunkSize, remaining);
                    var actualUncompressedChunkSize = uncompressedChunkSize;
                    var actualCompressedChunkSize = compressedChunkSize;

                    var result = context.Decompress(
                        compressedBytes,
                        0,
                        ref actualCompressedChunkSize,
                        uncompressedBytes,
                        0,
                        ref actualUncompressedChunkSize);
                    if (result != XCompression.ErrorCode.None)
                    {
                        throw new InvalidOperationException($"XCompression decompression failure ({result})");
                    }

                    if (actualUncompressedChunkSize != uncompressedChunkSize)
                    {
                        throw new InvalidOperationException("XCompression decompression failure (uncompressed size mismatch)");
                    }

                    output.Write(uncompressedBytes, 0, actualUncompressedChunkSize);

                    remaining -= actualUncompressedChunkSize;
                }
            }
        }

        private static void DecompressLZ4LW(IEntry entry, Stream input, Stream output)
        {
            var maybeTailSize = ReadPackedS32(input, out var headerSize);

            var decoder = new LZ4LW.LZ4LWDecoderStream(input, entry.CompressedSize - headerSize);

            var remaining = entry.UncompressedSize;

            var buffer = new byte[2048];
            while (remaining > 0)
            {
                int read;
                try
                {
                    read = decoder.Read(buffer, 0, buffer.Length);
                }
                catch (Exception)
                {
                    break;
                }
                if (read == 0)
                {
                    break;
                }
                output.Write(buffer, 0, read);
                remaining -= read;
            }
            output.Flush();
            if (remaining != 0)
            {
                //throw new InvalidOperationException();
            }
        }

        private static int ReadPackedS32(Stream input, out int read)
        {
            read = 1;
            byte b = input.ReadValueU8();
            int value = b & 0x7F;
            int shift = 7;
            while ((b & 0x80) != 0)
            {
                if (shift > 21)
                {
                    throw new InvalidOperationException();
                }
                read++;
                b = input.ReadValueU8();
                value |= (b & 0x7F) << shift;
            }
            return value;
        }
    }
}
