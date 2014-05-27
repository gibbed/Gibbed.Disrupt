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
using Gibbed.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Gibbed.Disrupt.FileFormats.Big
{
    public static class EntryDecompression
    {
        public static void Decompress(Entry entry, Stream input, Stream output)
        {
            input.Seek(entry.Offset, SeekOrigin.Begin);

            if (entry.CompressionScheme == CompressionScheme.None)
            {
                output.WriteFromStream(input, entry.CompressedSize);
            }
            else if (entry.CompressionScheme == CompressionScheme.LZO1x)
            {
                DecompressLzo(entry, input, output);
            }
            else if (entry.CompressionScheme == CompressionScheme.Zlib)
            {
                DecompressZlib(entry, input, output);
            }
            else if (entry.CompressionScheme == CompressionScheme.Xbox)
            {
                DecompressXMemCompress(entry, input, output);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void DecompressLzo(Entry entry, Stream input, Stream output)
        {
            var compressedBytes = new byte[entry.CompressedSize];
            if (input.Read(compressedBytes, 0, compressedBytes.Length) != compressedBytes.Length)
            {
                throw new EndOfStreamException();
            }

            var uncompressedBytes = new byte[entry.UncompressedSize];
            int actualUncompressedLength = uncompressedBytes.Length;

            var result = Compression.LZO.Decompress(compressedBytes,
                                                    0,
                                                    compressedBytes.Length,
                                                    uncompressedBytes,
                                                    0,
                                                    ref actualUncompressedLength);
            if (result != Compression.LZO.ErrorCode.Success)
            {
                throw new FormatException(string.Format("LZO decompression failure ({0})", result));
            }

            if (actualUncompressedLength != uncompressedBytes.Length)
            {
                throw new FormatException("LZO decompression failure (uncompressed size mismatch)");
            }

            output.Write(uncompressedBytes, 0, uncompressedBytes.Length);
        }

        private static void DecompressZlib(Entry entry, Stream input, Stream output)
        {
            if (entry.CompressedSize < 16)
            {
                throw new FormatException();
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

                    using (var temp = input.ReadToMemoryStream(compressedBlockSize))
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
                throw new InvalidOperationException();
            }
        }

        private static void DecompressXMemCompress(Entry entry, Stream input, Stream output)
        {
            var magic = input.ReadValueU32(Endian.Big);
            if (magic != 0x0FF512EE)
            {
                throw new FormatException();
            }

            var version = input.ReadValueU32(Endian.Big);
            if (version != 0x01030000)
            {
                throw new FormatException();
            }

            var unknown08 = input.ReadValueU32(Endian.Big);
            if (unknown08 != 0)
            {
                throw new FormatException();
            }

            var unknown0C = input.ReadValueU32(Endian.Big);
            if (unknown0C != 0)
            {
                throw new FormatException();
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
                throw new FormatException();
            }

            if (uncompressedSize != entry.UncompressedSize)
            {
                throw new FormatException();
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
                        throw new FormatException();
                    }

                    if (input.Read(compressedBytes, 0, compressedChunkSize) != compressedChunkSize)
                    {
                        throw new EndOfStreamException();
                    }

                    var uncompressedChunkSize = (int)Math.Min(largestUncompressedChunkSize, remaining);
                    var actualUncompressedChunkSize = uncompressedChunkSize;
                    var actualCompressedChunkSize = compressedChunkSize;

                    var result = context.Decompress(compressedBytes,
                                                    0,
                                                    ref actualCompressedChunkSize,
                                                    uncompressedBytes,
                                                    0,
                                                    ref actualUncompressedChunkSize);
                    if (result != XCompression.ErrorCode.None)
                    {
                        throw new FormatException(string.Format("XCompression decompression failure ({0})", result));
                    }

                    if (actualUncompressedChunkSize != uncompressedChunkSize)
                    {
                        throw new FormatException("XCompression decompression failure (uncompressed size mismatch)");
                    }

                    output.Write(uncompressedBytes, 0, actualUncompressedChunkSize);

                    remaining -= actualUncompressedChunkSize;
                }
            }
        }
    }
}
