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
            Lua_DoStringAddress = 0x75AD0,
            Lua_GetLocalizedTextAddress = 0x4AB730,
        }
        public enum afk
        {
            LastHardwareAction = 0x9D3798,
            TimeStamp = 0x9C0B14,
        }
    }
}
