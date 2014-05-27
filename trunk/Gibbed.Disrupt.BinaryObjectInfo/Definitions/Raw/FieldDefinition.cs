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
using System.Globalization;
using System.Xml.Serialization;

namespace Gibbed.Disrupt.BinaryObjectInfo.Definitions.Raw
{
    public class FieldDefinition
    {
        private string _Name;
        private uint? _Hash;

        public FieldDefinition()
        {
            this.Type = FieldType.BinHex;
            this.ArrayType = FieldType.Invalid;
        }

        [XmlAttribute("name")]
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Hash.HasValue == true &&
                    FileFormats.Hashing.CRC32.Compute(value) != this._Hash.Value)
                {
                    throw new InvalidOperationException();
                }

                this._Name = value;
            }
        }

        [XmlIgnore]
        public uint Hash
        {
            get
            {
                if (this._Hash.HasValue == true)
                {
                    return this._Hash.Value;
                }

                if (this._Name != null)
                {
                    var hash = FileFormats.Hashing.CRC32.Compute(this._Name);
                    this._Hash = hash;
                    return hash;
                }

                throw new InvalidOperationException();
            }

            set
            {
                if (this._Name != null &&
                    FileFormats.Hashing.CRC32.Compute(this._Name) != value)
                {
                    throw new InvalidOperationException();
                }

                this._Hash = value;
            }
        }

        [XmlAttribute("hash")]
        public string HashString
        {
            get { return this.Hash.ToString("X8"); }
            set { this.Hash = uint.Parse(value, NumberStyles.AllowHexSpecifier); }
        }

        [XmlAttribute("type")]
        public FieldType Type { get; set; }

        [XmlAttribute("array_type")]
        public FieldType ArrayType { get; set; }

        [XmlElement("enum")]
        public EnumDefinition Enum { get; set; }
    }
}
