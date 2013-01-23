using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iKick
{
    class Offsets
    {
        public struct Direct3D
        {
            public static uint Direct3D9__Device = 0xB18ADC;
            public static uint Direct3D9__Device__OffsetA = 0x2808;
            public static uint Direct3D9__Device__OffsetB = 0xA8;
        }
        public enum Endscene : uint
        {
            ClntObjMgrGetActivePlayerObjAddress = 0x33E0,
            Lua_DoStringAddress = 0x75AC0,
            Lua_GetLocalizedTextAddress = 0x4AB6A0,
        }
    }
}
