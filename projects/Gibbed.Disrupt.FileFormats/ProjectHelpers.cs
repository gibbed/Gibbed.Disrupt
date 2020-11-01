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
using System.Linq;

namespace Gibbed.Disrupt.FileFormats
{
    public static class ProjectHelpers
    {
        private static readonly Dictionary<ulong, uint> _KnownHashOverrideLookup;
        private static readonly Dictionary<ulong, uint> _UnknownHashOverrideLookup;

        private static ulong BaseHasher(string s)
        {
            return Hashing.FNV1a64.Compute(Modifier(s).ToLowerInvariant());
        }

        static ProjectHelpers()
        {
            // TODO: move this to project config?

            var overrides = new Dictionary<uint, string>()
            {
                { 0xFFFF0000u, @"domino\user\windycity\family_missions\act_04\family_02\a04_f02.a04_f02.lua" },
                { 0xFFFF0001u, @"domino\user\windycity\family_missions\act_04\family_02\a04_f02.a04_f02.debug.lua" },
            };

            _KnownHashOverrideLookup = overrides.ToDictionary(kv => BaseHasher(kv.Value), kv => kv.Key);
            _UnknownHashOverrideLookup = new Dictionary<ulong, uint>()
            {
                { 0x8B6FAB1F509C3FC9ul, 0xFFFF0002u },
                { 0xD56830C5D5399138ul, 0xFFFF0003u },
                { 0x41FA735B390C34B5ul, 0xFFFF0004u },
                { 0x44736FF39DBF15BCul, 0xFFFF0005u },
                { 0xB2DE63D263F39249ul, 0xFFFF0006u },
                { 0xB40711AFDDF925BFul, 0xFFFF0007u },
                { 0xF3C8F840F061FD63ul, 0xFFFF0008u },
                { 0x1BAA11D3042B6BB3ul, 0xFFFF0009u },
                { 0xD333C51C19C87D26ul, 0xFFFF000Au },
                { 0xAD71530850CBA951ul, 0xFFFF000Bu },
                { 0x51CF928F5EDE8E84ul, 0xFFFF000Cu },
                { 0xBCAF53BAFF5815C9ul, 0xFFFF000Du },
                { 0x16C668EC5CA617E7ul, 0xFFFF000Eu },
                { 0x93F094FAC7252DEBul, 0xFFFF000Fu },
                { 0xD5A78D3E9BFC66EFul, 0xFFFF0010u },
                { 0xDD5B186ED34FBDEDul, 0xFFFF0011u },
                { 0xF0CD369DB950D9DCul, 0xFFFF0012u },
                { 0xD2C4D725C62866BDul, 0xFFFF0013u },
                { 0x865895A91CF030ABul, 0xFFFF0014u },
                { 0x309EA84F2136BF72ul, 0xFFFF0015u },
                { 0xE10399BDDEF63E4Cul, 0xFFFF0016u },
                { 0x7855FFB981CE085Bul, 0xFFFF0017u },
                { 0xCE89466A5C46258Cul, 0xFFFF0018u },
                { 0x87936DAFCC98E7E5ul, 0xFFFF0019u },
                { 0x7475B046AFBE8D0Aul, 0xFFFF001Au },
                { 0x1206D272AED35316ul, 0xFFFF001Bu },
                { 0x0D2B966041E0FF3Cul, 0xFFFF001Cu },
                { 0x52E6DDE38FDD5C74ul, 0xFFFF001Du },
                { 0xC7B3CF732BE042FFul, 0xFFFF001Eu },
                { 0xDEFA925E68910A07ul, 0xFFFF001Fu },
                { 0xAE9D01A4ABB4E123ul, 0xFFFF0020u },
                { 0xF2CBE65C80CE76C1ul, 0xFFFF0021u },
                { 0x87A63D7512200200ul, 0xFFFF0022u },
                { 0xAACD6ED58D28E841ul, 0xFFFF0023u },
                { 0x8EB640CF8BEF52ADul, 0xFFFF0024u },
                { 0xA77153A7BE47546Eul, 0xFFFF0025u },
                { 0x8F79A46A5E3803D0ul, 0xFFFF0026u },
                { 0x79FB8C63D9752A1Dul, 0xFFFF0027u },
                { 0x3829652084C11CA6ul, 0xFFFF0028u },
                { 0xEAE26BBA6A5A9AF5ul, 0xFFFF0029u },
                { 0x64A2DE1001235CC4ul, 0xFFFF002Au },
                { 0x66E75775F76D1A50ul, 0xFFFF002Bu },
                { 0xA7388FEB12720DAEul, 0xFFFF002Cu },
                { 0xF6A3417C4BBF5DD9ul, 0xFFFF002Du },
                { 0x6855002A3274136Ful, 0xFFFF002Eu },
                { 0x8B40EFFB56B0C069ul, 0xFFFF002Fu },
                { 0x788E0E407D456FF6ul, 0xFFFF0030u },
                { 0x1465B330626DB9A6ul, 0xFFFF0031u },
                { 0x59F373F59DBBFE8Ful, 0xFFFF0032u },
                { 0x3D60AB7304786C75ul, 0xFFFF0033u },
                { 0xB8072F5CE68EA30Aul, 0xFFFF0034u },
                { 0xFF86355CF8D84F9Aul, 0xFFFF0035u },
                { 0x35DDC81B8F5B98E7ul, 0xFFFF0036u },
                { 0x592A8F59C3D9B23Bul, 0xFFFF0037u },
                { 0x8C1A9BA11173E918ul, 0xFFFF0038u },
                { 0x3B78C2A2DB4460A0ul, 0xFFFF0039u },
                { 0x5C4E23E01B504DE0ul, 0xFFFF003Au },
                { 0xE5C375170CDAE46Eul, 0xFFFF003Bu },
                { 0x2B99C6F44D11C876ul, 0xFFFF003Cu },
                { 0x6E7847AC9111EBC8ul, 0xFFFF003Du },
                { 0x6BACBB6EF8AB3495ul, 0xFFFF003Eu },
                { 0xA9C8049FC4D43BF1ul, 0xFFFF003Fu },
                { 0x41A036B67C0C618Bul, 0xFFFF0040u },
                { 0xEA7269AFAD61ECABul, 0xFFFF0041u },
                { 0xD8942E0374ADF6FAul, 0xFFFF0042u },
                { 0xEC4E5B0B5DAE503Dul, 0xFFFF0043u },
                { 0x15047F863547B6DDul, 0xFFFF0044u },
                { 0x0C7770AD5E429310ul, 0xFFFF0045u },
                { 0x22B07F5E7950FD2Dul, 0xFFFF0046u },
                { 0xD1422DAA367053EEul, 0xFFFF0047u },
                { 0x9675E64494816EBAul, 0xFFFF0048u },
                { 0xE4016E21B562C52Ful, 0xFFFF0049u },
                { 0xCF9B679CA1A66D21ul, 0xFFFF004Au },
                { 0xDA619E74D36E39B4ul, 0xFFFF004Bu },
                { 0x811F1D56DAA00EEEul, 0xFFFF004Cu },
                { 0xF67AADA9E51F9880ul, 0xFFFF004Du },
                { 0x5F534B9F30CF8D53ul, 0xFFFF004Eu },
                { 0x34CAA7CB6C8D6BA9ul, 0xFFFF004Fu },
                { 0x8E7339CAC373E8CDul, 0xFFFF0050u },
                { 0x55E0A0E9175972B5ul, 0xFFFF0051u },
                { 0xC1FEBB9A5055DCC6ul, 0xFFFF0052u },
                { 0x222783FD41714328ul, 0xFFFF0053u },
                { 0xB41205BE7C6928C6ul, 0xFFFF0054u },
                { 0xDF326521AB6C1246ul, 0xFFFF0055u },
                { 0x43BF1ED948DA7970ul, 0xFFFF00C8u },
                { 0x7D3F78A7EE2F35F7ul, 0xFFFF00C9u },
                { 0x035200779DBBFE8Ful, 0xFFFF00CAu },
                { 0x5C5497EB23458E13ul, 0xFFFF01F4u },
                { 0x28942F8BD278FE88ul, 0xFFFF01F5u },
                { 0x0B166253861447DCul, 0xFFFF01F6u },
            };
        }

        private static uint Hasher32(string s)
        {
            if (s == null || s.Length == 0)
            {
                return 0xFFFFFFFFu;
            }

            var hash64 = Hashing.FNV1a64.Compute(s.ToLowerInvariant());
            if (_KnownHashOverrideLookup.ContainsKey(hash64) == true)
            {
                return _KnownHashOverrideLookup[hash64];
            }
            if (_UnknownHashOverrideLookup.ContainsKey(hash64) == true)
            {
                return _UnknownHashOverrideLookup[hash64];
            }

            var hash32 = (uint)hash64;
            if ((hash32 & 0xFFFF0000) == 0xFFFF0000)
            {
                return hash32 & ~(1u << 16);
            }

            return hash32;
        }

        public static string Modifier(string s)
        {
            return s.Replace(@"/", @"\");
        }

        public static void LoadListsFileNames<T>(
            this ProjectData.Manager manager,
            int bigVersion,
            Func<string, T> hasher,
            out ProjectData.HashList<T> hashList)
        {
            hashList = manager.LoadLists("*.filelist", hasher, Modifier);
        }

        public static void LoadListsFileNames<T>(
            this ProjectData.Project project,
            int bigVersion,
            Func<string, T> hasher,
            out ProjectData.HashList<T> hashList)
        {
            hashList = project.LoadLists("*.filelist", hasher, Modifier);
        }
    }
}
