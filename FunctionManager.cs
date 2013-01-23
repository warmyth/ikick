using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic;

namespace iKick
{
    public class FunctionManager
    {
        private BlackMagic process;
        private HookManager aHook;

        public FunctionManager(BlackMagic process)
        {
            this.process = process;
            this.aHook = new HookManager(process);
        }

        public void LuaDoString(string command)
        {
            int nSize = command.Length + 0x100;
            uint codeCave = process.AllocateMemory(nSize);
            uint moduleBase = (uint)process.MainModule.BaseAddress;

            process.WriteASCIIString(codeCave, command);

            process.Asm.Clear();

            String[] asm = new String[] 
            {
                "mov eax, " + codeCave,
                "push 0",
                "push eax",
              
                "push eax",
                "mov eax, " + (moduleBase + Offsets.Endscene.Lua_DoStringAddress),
                
                "call eax",
                "add esp, 0xC",
                "retn",    
            };

            aHook.InjectAndExecute(asm);
            process.FreeMemory(codeCave);
        }
        public string GetLocalizedText(string command)
        {
            int nSize = command.Length + 0x100;
            uint codeCave = process.AllocateMemory(nSize);
            uint moduleBase = (uint)process.MainModule.BaseAddress;
            var ClntObjMgrGetActivePlayerObj = moduleBase + Offsets.Endscene.ClntObjMgrGetActivePlayerObjAddress;
            var FrameScript__GetLocalizedText = moduleBase + Offsets.Endscene.Lua_GetLocalizedTextAddress;

            process.WriteASCIIString(codeCave, command);

            String[] asm = new String[] 
                {
                "call " + ClntObjMgrGetActivePlayerObj,
                "mov ecx, eax",
                "push -1",
                "mov edx, " + codeCave + "",
                "push edx",
                "call " + FrameScript__GetLocalizedText,
                "retn",
                };

            string sResult = Encoding.ASCII.GetString(aHook.InjectAndExecute(asm));
            process.FreeMemory(codeCave);

            return sResult;
        }
        public void InteractGameObject(uint baseAddress)
        {
            uint InteractVMT = 45;
            if (baseAddress > 0)
            {
                uint VMT44 = process.ReadUInt(process.ReadUInt(baseAddress) + ((uint)InteractVMT * 4));
                var objectManagerBase = 0x463C;

                string[] asm = new string[]
                {
            "fs mov eax, [0x2C]",
            "mov eax, [eax]",
            "add eax, 0x10",
            "mov dword [eax], " + objectManagerBase,
            "mov ecx, " + baseAddress,
            "call " + VMT44,
            "retn",
                 };


                aHook.InjectAndExecute(asm);
            }
        }
    }

}
