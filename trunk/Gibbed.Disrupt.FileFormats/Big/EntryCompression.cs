﻿/* Copyright (c) 2014 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.Disrupt.FileFormats.Big
{
    public static class EntryCompression
    {
        public static void Compress(Target target,
                                    ref Entry entry,
                                    Stream input,
                                    bool compress,
                                    Stream output)
        {
            if (input.Length == 0)
            {
                entry.CompressionScheme = CompressionScheme.None;
                entry.UncompressedSize = 0;
                entry.CompressedSize = 0;
            }
            else if (compress == false)
            {
                entry.CompressionScheme = CompressionScheme.None;
                entry.UncompressedSize = 0;
                entry.CompressedSize = (uint)input.Length;
                output.WriteFromStream(input, input.Length);
            }
            else
            {
                if (target == Target.Win64)
                {
                    throw new NotImplementedException();
                    //CompressLzo(ref entry, input, output);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private static void CompressLzo(ref Entry entry,
                                        Stream input,
                                        Stream output)
        {
            var uncompressedData = input.ReadBytes((uint)input.Length);
            var uncompressedSize = (uint)uncompressedData.Length;

            var compressedData = new byte[uncompressedData.Length +
                                          (uncompressedData.Length / 16) + 64 + 3];
            var actualCompressedSize = compressedData.Length;

            var result = Compression.LZO.Compress(uncompressedData,
                                                  0,
                                                  uncompressedData.Length,
                                                  compressedData,
                                                  0,
                                                  ref actualCompressedSize);
            if (result != Compression.LZO.ErrorCode.Success)
            {
                throw new InvalidOperationException("compression error " + result.ToString());
            }

            if (actualCompressedSize < uncompressedSize)
            {
                entry.CompressionScheme = CompressionScheme.LZO1x;
                entry.UncompressedSize = uncompressedSize;
                entry.CompressedSize = (uint)actualCompressedSize;
                output.Write(compressedData, 0, actualCompressedSize);
            }
            else
            {
                input.Seek(0, SeekOrigin.Begin);
                entry.CompressionScheme = CompressionScheme.None;
                entry.UncompressedSize = 0;
                entry.CompressedSize = (uint)input.Length;
                output.WriteFromStream(input, input.Length);
            }
        }
    }
}
