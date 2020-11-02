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

namespace Gibbed.Disrupt.FileFormats.Big
{
    internal class CompressionSchemeV0
    {
        public static CompressionScheme ToCompressionScheme(byte id)
        {
            switch (id)
            {
                case 0: return CompressionScheme.None;
            }
            throw new NotSupportedException();
        }

        public static byte FromCompressionScheme(CompressionScheme scheme)
        {
            switch (scheme)
            {
                case CompressionScheme.None: return 0;
            }
            throw new NotSupportedException();
        }
    }
}
