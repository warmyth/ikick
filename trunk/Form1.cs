using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Media;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Magic;
using Ini;
using HotKeysHandler;

namespace iKick
{
    public partial class Form1 : Form
    {
        public static uint isLoadingOffset = 0xAD5636; //GameState
        readonly Process[] processes = Process.GetProcessesByName("Wow");
        BlackMagic application;
        FunctionManager functionManager;
        KeyboardHook hook = new KeyboardHook();
        public Form1()
        {
            InitializeComponent();
            BlackMagic application;
            FunctionManager functionManager;
            application = new BlackMagic(processes[0].Id);
            functionManager = new FunctionManager(application);
            hook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            hook.RegisterHotKey(HotKeysHandler.ModifierKeys.Alt, Keys.X);
            //functionManager.LuaDoString("print(\"Me: " + ObjectManager.Me.BaseAddress + "\");");
            //functionManager.LuaDoString("freeslots = GetContainerNumFreeSlots(0) + GetContainerNumFreeSlots(1) + GetContainerNumFreeSlots(2) + GetContainerNumFreeSlots(3) + GetContainerNumFreeSlots(4)");
            //MessageBox.Show(functionManager.GetLocalizedText("freeslots"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            application = new BlackMagic(processes[0].Id);
            functionManager = new FunctionManager(application);
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            IniFile ini = new IniFile(appPath + @"\config.ini");
            //functionManager.LuaDoString("local frame = CreateFrame(\"FRAME\", \"WhispFrame\"); frame:RegisterEvent(\"CHAT_MSG_WHISPER\"); local function whispHandler(arg1, arg2, WmsgContent, WmsgAuthor) fMsgTest = \"[W From] [\" .. WmsgAuthor .. \"]: \" .. WmsgContent; end frame:SetScript(\"OnEvent\", whispHandler);");
            //kickName.Text = ini.IniReadValue("CONFIG", "kickName");
            functionManager.LuaDoString("print(\"|cffff6060<|r|cFF00FFFFiKick|cffff6060>|r Ready !\")");
        }

        private void kickName_TextChanged(object sender, EventArgs e)
        {
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            IniFile ini = new IniFile(appPath + @"\config.ini");
            //ini.IniWriteValue("CONFIG", "kickName", kickName.Text);
        }

        private void kick_Tick(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                if (rTarget.Checked == true)
                {
                    functionManager.LuaDoString(textBox1.Text);
                }
                if (rFocus.Checked == true)
                {
                    functionManager.LuaDoString(textBox2.Text);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
          //  UInt32 chatStart = 0xAD79B8;
          //  UInt32 NextMessage = 0x17C0;
          //  textBox1.Clear();
          //  for (uint i = 0; i < 260; i++)
         //   {
         //       var baseMsg = chatStart + (i * NextMessage);
         //       string s = SMemory.ReadASCIIString(application.ProcessHandle, ((uint)application.MainModule.BaseAddress + baseMsg) + 0x3C, 0xFF);
         //       textBox1.Text += s + '\n';
          //  }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = "Kick Frequency: " + Convert.ToString(trackBar1.Value) + " ms";
            kick.Interval = trackBar1.Value;
        }
        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            string key = e.Key.ToString();
            string mod = e.Modifier.ToString();

            if (key == "X")
            {
                if (checkBox1.Checked)
                {
                    checkBox1.Checked = false;
                    functionManager.LuaDoString("print(\"|cffff6060[|r|cFF00FFFFiKick|cffff6060]|r Autokick Disabled !\")");
                    //functionManager.LuaDoString("RaidNotice_AddMessage(RaidWarningFrame, \"|cffff6060[|r|cFF00FFFFLazyRotation|cffff6060]|r Rotation Disabled !\", ChatTypeInfo[\"RAID_WARNING\"])");
                }
                else
                {
                    checkBox1.Checked = true;
                    functionManager.LuaDoString("print(\"|cffff6060[|r|cFF00FFFFiKick|cffff6060]|r Autokick Enabled !\")");
                    //functionManager.LuaDoString("RaidNotice_AddMessage(RaidWarningFrame, \"|cffff6060[|r|cFF00FFFFLazyRotation|cffff6060]|r Rotation Enabled !\", ChatTypeInfo[\"RAID_WARNING\"])");
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            functionManager.LuaDoString(textBox3.Text);
        }
    }
}
