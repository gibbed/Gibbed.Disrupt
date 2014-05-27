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
using System.Text;

namespace Gibbed.Disrupt.FileFormats
{
    public static class FileExtensions
    {
        public static Tuple<string, string> Detect(byte[] guess, int read)
        {
            if (read == 0)
            {
                return new Tuple<string, string>("null", null);
            }

            if (read >= 5 &&
                guess[0] == 'M' &&
                guess[1] == 'A' &&
                guess[2] == 'G' &&
                guess[3] == 'M' &&
                guess[4] == 'A')
            {
                return new Tuple<string, string>("ui", "mgb");
            }

            if (read >= 3 &&
                guess[0] == 'B' &&
                guess[1] == 'I' &&
                guess[2] == 'K')
            {
                return new Tuple<string, string>("gfx", "bik");
            }

            if (read >= 3 &&
                guess[0] == 'U' &&
                guess[1] == 'E' &&
                guess[2] == 'F')
            {
                return new Tuple<string, string>("ui", "feu");
            }

            if (read >= 3 &&
                guess[0] == 0 &&
                guess[1] == 0 &&
                guess[2] == 0xFF)
            {
                return new Tuple<string, string>("misc", "maybe.rml");
            }

            if (read >= 8 &&
                guess[4] == 0x68 &&
                guess[5] == 0x4D &&
                guess[6] == 0x76 &&
                guess[7] == 0x4E)
            {
                return new Tuple<string, string>("gfx", "hMvN");
            }

            if (read >= 20 &&
                guess[16] == 0x57 &&
                guess[17] == 0xE0 &&
                guess[18] == 0xE0 &&
                guess[19] == 0x57)
            {
                return new Tuple<string, string>("gfx", "hkx");
            }

            if (read >= 4)
            {
                uint magic = BitConverter.ToUInt32(guess, 0);

                if (magic == 0x00584254 || magic == 0x54425800) // '\0XBT'
                {
                    return new Tuple<string, string>("gfx", "xbt");
                }

                if (magic == 0x4D455348) // 'MESH'
                {
                    return new Tuple<string, string>("gfx", "xbg");
                }

                if (magic == 0x54414D00 || magic == 0x004D4154) // '\0MAT'
                {
                    return new Tuple<string, string>("gfx", "material.bin");
                }

                if (magic == 0x53504B02) // 'SPK\2'
                {
                    return new Tuple<string, string>("sfx", "spk");
                }

                if (magic == 0x4643626E) // 'FCbn'
                {
                    return new Tuple<string, string>("game", "fcb");
                }

                if (magic == 0x534E644E) // 'SNdN'
                {
                    return new Tuple<string, string>("game", "rnv");
                }

                if (magic == 0x474E5089) // 'PNG\x89'
                {
                    return new Tuple<string, string>("gfx", "png");
                }

                if (magic == 0x4D564D00)
                {
                    return new Tuple<string, string>("gfx", "MvN");
                }

                if (magic == 0x61754C1B)
                {
                    return new Tuple<string, string>("scripts", "luab");
                }

                if (magic == 0x47454F4D)
                {
                    return new Tuple<string, string>("gfx", "geom");
                }

                if (magic == 0x00014C53)
                {
                    return new Tuple<string, string>("misc", "ls");
                }
            }

            string text = Encoding.ASCII.GetString(guess, 0, read);

            if (read >= 3 && text.StartsWith("-- ") == true)
            {
                return new Tuple<string, string>("scripts", "lua");
            }

            if (read >= 6 && text.StartsWith("<root>") == true)
            {
                return new Tuple<string, string>("misc", "root.xml");
            }

            if (read >= 9 && text.StartsWith("<package>") == true)
            {
                return new Tuple<string, string>("ui", "mbg.desc");
            }

            if (read >= 12 && text.StartsWith("<NewPartLib>") == true)
            {
                return new Tuple<string, string>("misc", "NewPartLib.xml");
            }

            if (read >= 14 && text.StartsWith("<BarkDataBase>") == true)
            {
                return new Tuple<string, string>("misc", "BarkDataBase.xml");
            }

            if (read >= 13 && text.StartsWith("<BarkManager>") == true)
            {
                return new Tuple<string, string>("misc", "BarkManager.xml");
            }

            if (read >= 17 && text.StartsWith("<ObjectInventory>") == true)
            {
                return new Tuple<string, string>("misc", "ObjectInventory.xml");
            }

            if (read >= 21 && text.StartsWith("<CollectionInventory>") == true)
            {
                return new Tuple<string, string>("misc", "CollectionInventory.xml");
            }

            if (read >= 14 && text.StartsWith("<SoundRegions>") == true)
            {
                return new Tuple<string, string>("misc", "SoundRegions.xml");
            }

            if (read >= 11 && text.StartsWith("<MovieData>") == true)
            {
                return new Tuple<string, string>("misc", "MovieData.xml");
            }

            if (read >= 8 && text.StartsWith("<Profile") == true)
            {
                return new Tuple<string, string>("misc", "Profile.xml");
            }

            if (read >= 12 && text.StartsWith("<stringtable") == true)
            {
                return new Tuple<string, string>("text", "xml");
            }

            if (read >= 5 && text.StartsWith("<?xml") == true)
            {
                return new Tuple<string, string>("misc", "xml");
            }

            if (read >= 1 && text.StartsWith("<Sequence>") == true)
            {
                return new Tuple<string, string>("game", "cseq");
            }

            return null;
        }
    }
}
