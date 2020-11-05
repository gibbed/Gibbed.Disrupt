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

using System.Collections.Generic;
using System.IO;

namespace Gibbed.Disrupt.FileFormats.Big
{
    public delegate bool TryGetHashOverride<T>(ulong hash, out T hashOverride);
    public delegate T ComputeNameHash<T>(string s, TryGetHashOverride<T> tryGetHashOverride);

    public interface IArchive<T>
    {
        int Version { get; set; }
        Platform Platform { get; set; }
        byte CompressionVersion { get; set; }
        byte NameHashVersion { get; set; }

        List<Entry<T>> Entries { get; }

        void Serialize(Stream output);
        void Deserialize(Stream input);

        T ComputeNameHash(string s, TryGetHashOverride<T> getOverride);
        bool TryParseNameHash(string s, out T value);
        string RenderNameHash(T value);

        CompressionScheme ToCompressionScheme(byte id);
        byte FromCompressionSCheme(CompressionScheme compressionScheme);
    }
}
