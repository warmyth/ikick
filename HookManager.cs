using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Magic;
using System.Diagnostics;

namespace iKick
{
    public class HookManager
    {
        private BlackMagic process;

        private bool mainThreadHooked;
        private bool ExecutingCode;

        private uint codeCave;
        private uint injectionAddress;
        private uint returnAddress;

        public HookManager(BlackMagic process)
        {
            this.process = process;

            this.mainThreadHooked = false;
            this.ExecutingCode = false;

            this.codeCave = 0;
            this.injectionAddress = 0;
            this.returnAddress = 0;
        }

        private void HookApplication()
        {
            if (!process.IsProcessOpen)
                throw new Exception("Process is not open");

            uint baseAddress = (uint)process.MainModule.BaseAddress;
            uint pDevice = process.ReadUInt(baseAddress + Offsets.Direct3D.Direct3D9__Device);
            uint pEnd = process.ReadUInt(pDevice + Offsets.Direct3D.Direct3D9__Device__OffsetA);
            uint pScene = process.ReadUInt(pEnd);
            uint pEndScene = process.ReadUInt(pScene + Offsets.Direct3D.Direct3D9__Device__OffsetB);

            if (process.ReadUInt(pEndScene) == 0xE9 && (codeCave == 0 || injectionAddress == 0))
            {
                DisposeOfHook();
            }
            if (process.ReadUInt(pEndScene) != 0xE9)
            {
                try
                {
                    mainThreadHooked = false;

                    codeCave = process.AllocateMemory(2048);
                    injectionAddress = process.AllocateMemory(0x4);

                    process.WriteInt(injectionAddress, 0);

                    returnAddress = process.AllocateMemory(0x4);
                    process.WriteInt(returnAddress, 0);

                    process.Asm.Clear();

                    process.Asm.AddLine("mov edi, edi");
                    process.Asm.AddLine("push ebp");
                    process.Asm.AddLine("mov ebp, esp");

                    process.Asm.AddLine("pushfd");
                    process.Asm.AddLine("pushad");

                    process.Asm.AddLine("mov eax, [" + injectionAddress + "]");
                    process.Asm.AddLine("test eax, eax");
                    process.Asm.AddLine("je @out");

                    process.Asm.AddLine("mov eax, [" + injectionAddress + "]");
                    process.Asm.AddLine("call eax");

                    process.Asm.AddLine("mov [" + returnAddress + "], eax");

                    process.Asm.AddLine("mov edx, " + injectionAddress);
                    process.Asm.AddLine("mov ecx, 0");
                    process.Asm.AddLine("mov [edx], ecx");

                    process.Asm.AddLine("@out:");

                    uint sizeAsm = (uint)(process.Asm.Assemble().Length);

                    process.Asm.Inject(codeCave);

                    int sizeJumpBack = 5;

                    process.Asm.Clear();
                    process.Asm.AddLine("jmp " + (pEndScene + sizeJumpBack));
                    process.Asm.Inject(codeCave + sizeAsm);// + (uint)sizeJumpBack);

                    process.Asm.Clear(); // $jmpto
                    process.Asm.AddLine("jmp " + (codeCave));
                    process.Asm.Inject(pEndScene);

                }
                catch
                {
                    mainThreadHooked = false; return;
                }
                mainThreadHooked = true;
            }

        }
        private void DisposeOfHook()
        {
            if (!process.IsProcessOpen)
                throw new Exception("Process is not open");

            uint baseAddress = (uint)process.MainModule.BaseAddress;
            uint pDevice = process.ReadUInt(baseAddress + Offsets.Direct3D.Direct3D9__Device);
            uint pEnd = process.ReadUInt(pDevice + Offsets.Direct3D.Direct3D9__Device__OffsetA);
            uint pScene = process.ReadUInt(pEnd);
            uint pEndScene = process.ReadUInt(pScene + Offsets.Direct3D.Direct3D9__Device__OffsetB);

            try
            {
                if (process.ReadByte(pEndScene) == 0xE9) // check if wow is already hooked and dispose Hook
                {
                    // Restore origine endscene:
                    process.Asm.Clear();
                    process.Asm.AddLine("mov edi, edi");
                    process.Asm.AddLine("push ebp");
                    process.Asm.AddLine("mov ebp, esp");
                    process.Asm.Inject(pEndScene);
                }

                // free memory:
                process.FreeMemory(codeCave);
                process.FreeMemory(injectionAddress);
                process.FreeMemory(returnAddress);
            }
            catch
            {
            }
        }

        public byte[] InjectAndExecute(string[] asm)
        {
            while (ExecutingCode)
            {
                System.Threading.Thread.Sleep(5);
            }

            ExecutingCode = true;

            HookApplication();

            byte[] tempsByte = new byte[0];

            // reset return value pointer
            process.WriteInt(returnAddress, 0);

            if (process.IsProcessOpen && mainThreadHooked)
            {
                // Write the asm stuff
                process.Asm.Clear();
                foreach (string tempLineAsm in asm)
                {
                    process.Asm.AddLine(tempLineAsm);
                }

                // Allocation Memory
                int codeSize = process.Asm.Assemble().Length;
                uint injectionAsm_Codecave = process.AllocateMemory(codeSize);


                try
                {
                    // Inject
                    process.Asm.Inject(injectionAsm_Codecave);
                    process.WriteInt(injectionAddress, (int)injectionAsm_Codecave);

                    // Wait to launch code
                    while (process.ReadInt(injectionAddress) > 0)
                    {
                        System.Threading.Thread.Sleep(5);
                    }

                    byte Buf = new Byte();
                    List<byte> retnByte = new List<byte>();
                    uint dwAddress = process.ReadUInt(returnAddress);
                    Buf = process.ReadByte(dwAddress);
                    while (Buf != 0)
                    {
                        retnByte.Add(Buf);
                        dwAddress = dwAddress + 1;
                        Buf = process.ReadByte(dwAddress);
                    }
                    tempsByte = retnByte.ToArray();
                }
                catch { }

                // Free memory allocated 
                process.FreeMemory(injectionAsm_Codecave);
            }

            DisposeOfHook();

            ExecutingCode = false;

            return tempsByte;
        }
    }
}
