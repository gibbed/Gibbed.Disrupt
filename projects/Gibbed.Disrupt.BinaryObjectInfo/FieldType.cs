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

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    public enum FieldType
    {
        Invalid = 0,
        BinHex,

        Boolean, // bool
        UInt8, // unsigned char
        Int8, // char
        UInt16, // unsigned short
        Int16, // signed short
        UInt32, // unsigned int
        Int32, // signed int
        UInt64, // unsigned long long
        Int64, // signed long long
        Float, // float
        Vector2, // ndVec2
        Vector3, // ndVec3
        Vector4, // ndVec4
        Quaternion, // ndQuat
        String, // ndString
        Enum, // enum
        StringId, // CStringID
        NoCaseStringId, // CNoCaseStringID
        PathId, // CPathID
        StringId64, // CStringID WDL
        NoCaseStringId64, // CNoCaseStringID WDL
        PathId64, // CPathID WDL

        Rml,
        Array32,
    }
}
