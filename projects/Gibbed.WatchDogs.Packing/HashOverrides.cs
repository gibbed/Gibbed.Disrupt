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

#undef SANITY_CHECK_OVERRIDES

#if SANITY_CHECK_OVERRIDES
using System;
#endif
using System.Collections.Generic;

namespace Gibbed.WatchDogs.Packing
{
    public static class HashOverrides
    {
#if SANITY_CHECK_OVERRIDES
        private static readonly HashOverrideList _HashOverrideList;
#endif
        private static readonly Dictionary<ulong, uint> _HashOverrideLookup;

        public static bool TryGet(ulong hash, out uint hashOverride)
        {
#if SANITY_CHECK_OVERRIDES
            var result = _HashOverrideLookup.TryGetValue(hash, out hashOverride);
            if (result == true)
            {
                var index = _HashOverrideList.FindIndex(m => m.Hash == hash);
                if (index >= 0 && _HashOverrideList[index].Path == null)
                {
                    throw new KeyNotFoundException();
                }
            }
            return result;
#else
            return _HashOverrideLookup.TryGetValue(hash, out hashOverride);
#endif
        }

        static HashOverrides()
        {
            // TODO: move this to project config?
            var hashOverrideList = new HashOverrideList()
            {
                { 0xB2F136599DFB477Eul, 0xFFFF0000u, @"domino\user\windycity\family_missions\act_04\family_02\a04_f02.a04_f02.lua" },
                { 0x6C1042BAC18FC195ul, 0xFFFF0001u, @"domino\user\windycity\family_missions\act_04\family_02\a04_f02.a04_f02.debug.lua" },
                { 0x8B6FAB1F509C3FC9ul, 0xFFFF0002u },
                { 0xD56830C5D5399138ul, 0xFFFF0003u },
                { 0x41FA735B390C34B5ul, 0xFFFF0004u, @"graphics\objects\_interior\_iraqplace\ip_fanvent_sas.high.xbgmip" },
                { 0x44736FF39DBF15BCul, 0xFFFF0005u },
                { 0xB2DE63D263F39249ul, 0xFFFF0006u, @"animations\facial\facefx\generated\000c42ad.dpax" },
                { 0xB40711AFDDF925BFul, 0xFFFF0007u, @"graphics\_materials\jmarcoux-m-20130711174135.material.bin" },
                { 0xF3C8F840F061FD63ul, 0xFFFF0008u, @"graphics\characters\kits\lowincome\female\torso\low_f_tor_leajcket01\low_f_tor_leajcket01_cloth_d.xbt" },
                { 0x1BAA11D3042B6BB3ul, 0xFFFF0009u, @"worlds\windy_city\generated\building\{998a0e22-dd06-4aa3-bff4-e386224946cf}_building_15947_lowres.xbg" },
                { 0xD333C51C19C87D26ul, 0xFFFF000Au },
                { 0xAD71530850CBA951ul, 0xFFFF000Bu, @"graphics\buildings\facade\facade_parkersquare_base_door_8x4_04a.xbg" },
                { 0x51CF928F5EDE8E84ul, 0xFFFF000Cu },
                { 0xBCAF53BAFF5815C9ul, 0xFFFF000Du },
                { 0x16C668EC5CA617E7ul, 0xFFFF000Eu },
                { 0x93F094FAC7252DEBul, 0xFFFF000Fu },
                { 0xD5A78D3E9BFC66EFul, 0xFFFF0010u },
                { 0xDD5B186ED34FBDEDul, 0xFFFF0011u, @"animations\ltrain\70-00-021_mcivi_l-train-lean-on-bar-side-hack-stop-v3_090r_norm_na_02.mab" },
                { 0xF0CD369DB950D9DCul, 0xFFFF0012u },
                { 0xD2C4D725C62866BDul, 0xFFFF0013u },
                { 0x865895A91CF030ABul, 0xFFFF0014u },
                { 0x309EA84F2136BF72ul, 0xFFFF0015u },
                { 0xE10399BDDEF63E4Cul, 0xFFFF0016u },
                { 0x7855FFB981CE085Bul, 0xFFFF0017u },
                { 0xCE89466A5C46258Cul, 0xFFFF0018u },
                { 0x87936DAFCC98E7E5ul, 0xFFFF0019u },
                { 0x7475B046AFBE8D0Aul, 0xFFFF001Au },
                { 0x1206D272AED35316ul, 0xFFFF001Bu, @"soundbinary\00095588.spk" },
                { 0x0D2B966041E0FF3Cul, 0xFFFF001Cu, @"soundbinary\0001d76a.spk" },
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
                { 0xEAE26BBA6A5A9AF5ul, 0xFFFF0029u, @"soundbinary\0008c823.spk" },
                { 0x64A2DE1001235CC4ul, 0xFFFF002Au },
                { 0x66E75775F76D1A50ul, 0xFFFF002Bu },
                { 0xA7388FEB12720DAEul, 0xFFFF002Cu },
                { 0xF6A3417C4BBF5DD9ul, 0xFFFF002Du },
                { 0x6855002A3274136Ful, 0xFFFF002Eu },
                { 0x8B40EFFB56B0C069ul, 0xFFFF002Fu },
                { 0x788E0E407D456FF6ul, 0xFFFF0030u },
                { 0x1465B330626DB9A6ul, 0xFFFF0031u, @"worlds\windy_city\generated\wlu\wlu_data_near1028.xml.data.fcb.embed" },
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
                { 0xA9C8049FC4D43BF1ul, 0xFFFF003Fu, @"soundbinary\0012434a.sbao" },
                { 0x41A036B67C0C618Bul, 0xFFFF0040u },
                { 0xEA7269AFAD61ECABul, 0xFFFF0041u },
                { 0xD8942E0374ADF6FAul, 0xFFFF0042u },
                { 0xEC4E5B0B5DAE503Dul, 0xFFFF0043u },
                { 0x15047F863547B6DDul, 0xFFFF0044u },
                { 0x0C7770AD5E429310ul, 0xFFFF0045u },
                { 0x22B07F5E7950FD2Dul, 0xFFFF0046u },
                { 0xD1422DAA367053EEul, 0xFFFF0047u },
                { 0x9675E64494816EBAul, 0xFFFF0048u },
                { 0xE4016E21B562C52Ful, 0xFFFF0049u, @"soundbinary\00132d61.sbao" },
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
                { 0x8DEF6B476DC0536Eul, 0xFFFF0056u },
                //{ 0x43BF1ED948DA7970ul, 0xFFFF00C8u },
                //{ 0x7D3F78A7EE2F35F7ul, 0xFFFF00C9u },
                //{ 0x035200779DBBFE8Ful, 0xFFFF00CAu },
                { 0x9B6346076FB1AAC9ul, 0xFFFF012Cu },
                { 0xB51AD52856BD0920ul, 0xFFFF012Du },
                { 0x17860DC07BD54FFBul, 0xFFFF012Eu, @"worlds\windy_city\generated\roadresources\roadres_0000000034b0cbcc.hgfx" },
                { 0x5C5497EB23458E13ul, 0xFFFF01F4u },
                { 0x28942F8BD278FE88ul, 0xFFFF01F5u, @"worlds\windy_city\generated\batchmeshentity\batchmeshentity_c2_i0_xn0001_yn0001_xp0001_yp0001_00000000b8a91fe5_phys.cbatch" },
                { 0x0B166253861447DCul, 0xFFFF01F6u, @"worlds\windy_city\generated\batchmeshentity\batchmeshentity_c1_i0_xn1791_yn1535_xn1537_yn1281_compound.cbatch" },
            };
#if SANITY_CHECK_OVERRIDES
            _HashOverrideList = hashOverrideList;
#endif
            _HashOverrideLookup = new Dictionary<ulong, uint>();
            foreach (var item in hashOverrideList)
            {
#if SANITY_CHECK_OVERRIDES
                if (item.Path != null)
                {
                    var hash = Disrupt.FileFormats.Hashing.FNV1a64.Compute(item.Path);
                    if (hash != item.Hash)
                    {
                        throw new InvalidOperationException();
                    }
                }
#endif
                _HashOverrideLookup.Add(item.Hash, item.HashOverride);
            }
        }
    }
}
