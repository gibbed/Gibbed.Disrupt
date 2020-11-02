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

namespace Gibbed.Disrupt.FileFormats.Big
{
    // Names in this enumeration should match what we see the game use for the
    // game folder. data_win64, data_orbis, etc.
    //
    // The numeric IDs of this enumeration do *not* match the actual IDs you
    // see in game files. This is a custom set to include all platforms across
    // archive versions.
    public enum Platform : byte
    {
        Any = 0,
        Win32, // data_win32 *UNUSED*
        Win64, // data_win64
        Xenon, // no platform directory : Xbox 360
        PS3, // data_ps3
        WiiU, // no platform directory : Wii U
        Durango, // data_durango : Xbox 1
        Orbis, // data_orbis : PS4
        Scarlett, // data_scarlett : Xbox Series X
        Prospero, // data_prospero : PS5
        Yeti, // data_yeti : Stadia
    }
}
