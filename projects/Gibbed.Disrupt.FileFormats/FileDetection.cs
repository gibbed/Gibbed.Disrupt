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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Disrupt.FileFormats
{
    public static class FileDetection
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
                guess[4] == 'h' &&
                guess[5] == 'M' &&
                guess[6] == 'v' &&
                guess[7] == 'N')
            {
                return new Tuple<string, string>("gfx", "hMvN");
            }

            if (read >= 8 &&
                guess[4] == 'Q' &&
                guess[5] == 'E' &&
                guess[6] == 'S' &&
                guess[7] == 0)
            {
                return new Tuple<string, string>("game", "cseq");
            }

            if (read >= 20 &&
                guess[16] == 'W' &&
                guess[17] == 0xE0 &&
                guess[18] == 0xE0 &&
                guess[19] == 'W')
            {
                return new Tuple<string, string>("gfx", "hkx");
            }

            if (read >= 8 &&
                guess[0] == 0xEF &&
                guess[1] == 0xBB &&
                guess[2] == 0xBF &&
                guess[3] == '<' &&
                guess[4] == '?' &&
                guess[5] == 'x' &&
                guess[6] == 'm' &&
                guess[7] == 'l')
            {
                return new Tuple<string, string>("misc", "xml");
            }

            if (read >= 20)
            {
                uint magic = BitConverter.ToUInt32(guess, 16);
                var result = DetectMagic(magic) ?? DetectMagic(magic.Swap());
                if (result != null)
                {
                    return result;
                }
            }

            if (read >= 4)
            {
                uint magic = BitConverter.ToUInt32(guess, 0);
                var result = DetectMagic(magic) ?? DetectMagic(magic.Swap());
                if (result != null)
                {
                    return result;
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

            if (read >= 12 && text.StartsWith("<MinimapInfo") == true)
            {
                return new Tuple<string, string>("misc", "MinimapInfo.xml");
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
                return new Tuple<string, string>("game", "seq");
            }

            if (read >= 8 && text.StartsWith("<Binary>") == true)
            {
                return new Tuple<string, string>("pilot", "pnm");
            }

            if (read >= 15 && text.StartsWith("SQLite format 3") == true)
            {
                return new Tuple<string, string>("db", "sqlite3");
            }

            if (read >= 2 && guess[0] == 'p' && guess[1] == 'A')
            {
                return new Tuple<string, string>("animations", "dpax");
            }

            return null;
        }

        private static Tuple<string, string> DetectMagic(uint magic)
        {
            switch (magic)
            {
                case 0x5374726D: /* 'Strm'    */ return new Tuple<string, string>("strm", "bin");
                case 0x00584254: /* '\0XBT'   */ return new Tuple<string, string>("gfx", "xbt");
                case 0x4D455348: /* 'MESH'    */ return new Tuple<string, string>("gfx", "xbg");
                case 0x54414D00: /* '\0MAT'   */ return new Tuple<string, string>("gfx", "material.bin");
                case 0x53504B02: /* 'SPK\2'   */ return new Tuple<string, string>("sfx", "spk");
                case 0x4643626E: /* 'FCbn'    */ return new Tuple<string, string>("game", "fcb");
                case 0x534E644E: /* 'SNdN'    */ return new Tuple<string, string>("game", "rnv");
                case 0x474E5089: /* 'PNG\x89' */ return new Tuple<string, string>("gfx", "png");
                case 0x4D564D00: /* 'MVM\0'   */ return new Tuple<string, string>("gfx", "MvN");
                case 0x61754C1B: /* 'Lua\x1B' */ return new Tuple<string, string>("scripts", "luab");
                case 0x47454F4D: /* 'GEOM'    */ return new Tuple<string, string>("gfx", "xbg");
                case 0x42544348: /* 'BTCH'    */ return new Tuple<string, string>("cbatch", "cbatch");
                case 0x53524852: /* 'SRHR'    */ return new Tuple<string, string>("srhr", "bin");
                case 0x53524C52: /* 'SRLR'    */ return new Tuple<string, string>("srlr", "bin");
                case 0x53435452: /* 'SCTR'    */ return new Tuple<string, string>("sctr", "bin");
                case 0x54524545: /* 'TREE'    */ return new Tuple<string, string>("tree", "bin");
                case 0x50494D47: /* 'PIMG'    */ return new Tuple<string, string>("pimg", "bin");
                case 0x45534142: /* 'BASE'    */ return new Tuple<string, string>("wlu", "fcb");
                case 0x66314130: /* 'f1A0'    */ return new Tuple<string, string>("dialog", "stimuli.dsc.pack");
                case 0x6732424B: /* 'g2BK'    */ return new Tuple<string, string>("bink", "bik");
                case 0x0A6F6E61: /* 'ano\n'   */ return new Tuple<string, string>("annotation", "ano");
                case 0x4C695072: /* 'LiPr'    */ return new Tuple<string, string>("lightprobe", "lipr.bin");
                case 0x4D763211: /* 'Mv2\x11' */ return new Tuple<string, string>("move", "bin");
                case 0x534C4852: /* 'SLHR'    */ return new Tuple<string, string>("roadresources", "hgfx");
                case 0x474D4950: /* 'GMIP'    */ return new Tuple<string, string>("gfx", "xbgmip");
                case 0x4C504D54: /* 'LPMT'    */ return new Tuple<string, string>("bin", "lpmt");
                case 0x424B4844: /* 'BKHD'    */ return new Tuple<string, string>("bin", "bkhd");
                case 0x434B5441: /* 'CKTA'    */ return new Tuple<string, string>("bin", "ckta");
                case 0x4F54544F: /* 'OTTO'    */ return new Tuple<string, string>("bin", "otto");
                case 0x4D475246: /* 'MGRF'    */ return new Tuple<string, string>("bin", "mgrf");
                case 0x00014C53: /*           */ return new Tuple<string, string>("languages", "loc");
                case 0x00032A02: /*           */ return new Tuple<string, string>("sfx", "sbao");
                case 0x0000389C: /*           */ return new Tuple<string, string>("eight", "bin");

            }
            return null;
        }
    }
}
