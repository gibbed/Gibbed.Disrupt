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

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    public static class FieldTypeHelper
    {
        public static string GetString(this FieldType type)
        {
            switch (type)
            {
                case FieldType.BinHex:
                {
                    return "BinHex";
                }

                case FieldType.Boolean:
                {
                    return "Boolean";
                }

                case FieldType.UInt8:
                {
                    return "UInt8";
                }

                case FieldType.Int8:
                {
                    return "Int8";
                }

                case FieldType.UInt16:
                {
                    return "UInt16";
                }

                case FieldType.Int16:
                {
                    return "Int16";
                }

                case FieldType.UInt32:
                {
                    return "UInt32";
                }

                case FieldType.Int32:
                {
                    return "Int32";
                }

                case FieldType.UInt64:
                {
                    return "UInt64";
                }
                case FieldType.Int64:
                {
                    return "Int64";
                }

                case FieldType.Float32:
                {
                    return "Float32";
                }

                case FieldType.Float64:
                {
                    return "Float64";
                }
                case FieldType.Vector2:
                {
                    return "Vector2";
                }

                case FieldType.Vector3:
                {
                    return "Vector3";
                }

                case FieldType.Vector4:
                {
                    return "Vector4";
                }

                case FieldType.String:
                {
                    return "String";
                }

                case FieldType.Enum8:
                {
                    return "Enum8";
                }

                case FieldType.Enum16:
                {
                    return "Enum16";
                }

                case FieldType.Enum32:
                {
                    return "Enum32";
                }

                case FieldType.Hash32:
                {
                    return "Hash32";
                }

                case FieldType.Hash64:
                {
                    return "Hash64";
                }

                case FieldType.Id32:
                {
                    return "Id32";
                }

                case FieldType.Id64:
                {
                    return "Id64";
                }

                case FieldType.Rml:
                {
                    return "Rml";
                }

                case FieldType.ComputeHash32:
                {
                    return "ComputeHash32";
                }

                case FieldType.ComputeHash64:
                {
                    return "ComputeHash64";
                }

                case FieldType.Array32:
                {
                    return "Array32";
                }
            }

            throw new NotSupportedException();
        }
    }
}
