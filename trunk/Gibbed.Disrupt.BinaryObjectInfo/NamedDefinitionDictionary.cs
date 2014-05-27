﻿/* Copyright (c) 2014 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    public sealed class NamedDefinitionDictionary<TType>
        where TType : INamedDefinition
    {
        private readonly List<TType> _Items;

        public NamedDefinitionDictionary(IEnumerable<TType> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this._Items = items.ToList();
        }

        public bool ContainsKey(string key)
        {
            return this._Items.Any(i => i.Name == key);
        }

        public TType this[string key]
        {
            get { return this._Items.FirstOrDefault(i => i.Name == key); }
        }

        public IEnumerable<TType> Items
        {
            get { return this._Items; }
        }
    }
}
