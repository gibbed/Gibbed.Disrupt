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
using System.Linq;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions;
using Gibbed.Disrupt.BinaryObjectInfo.Definitions.Loaders;

namespace Gibbed.Disrupt.BinaryObjectInfo
{
    public class InfoManager
    {
        public HashedDefinitionDictionary<ClassDefinition> ClassDefinitions { get; private set; }
        public NamedDefinitionDictionary<ObjectFileDefinition> ObjectFileDefinitions { get; private set; }

        private InfoManager()
        {
        }

        public static InfoManager Load(string basePath)
        {
            var manager = new InfoManager();
            manager.ClassDefinitions = ClassDefinitionLoader.Load(basePath);
            manager.ObjectFileDefinitions = ObjectFileDefinitionLoader.Load(basePath, manager.ClassDefinitions);
            return manager;
        }

        public ClassDefinition GetClassDefinition(string name)
        {
            return this.ClassDefinitions[name];
        }

        public ClassDefinition GetClassDefinition(uint hash)
        {
            return this.ClassDefinitions[hash];
        }

        public ObjectFileDefinition GetObjectFileDefinition(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            name = name.ToLowerInvariant();
            return this.ObjectFileDefinitions.Items.FirstOrDefault(i => i.Aliases.Contains(name) == true);
        }
    }
}
