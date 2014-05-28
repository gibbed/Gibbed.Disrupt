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

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    // Don't forget to update FieldTypeHelper if you add something here.
    public enum FieldType
    {
        Invalid = 0,
        BinHex,
        Boolean,
        UInt8,
        Int8,
        UInt16,
        Int16,
        UInt32,
        Int32,
        UInt64,
        Int64,
        Float32,
        Float64,
        Vector2,
        Vector3,
        Vector4,
        String,
        Enum8,
        Enum16,
        Enum32,
        Hash32,
        Hash64,
        Id32,
        Id64,
        Rml,
        ComputeHash32,
        ComputeHash64,
        Array32,
    }
}
