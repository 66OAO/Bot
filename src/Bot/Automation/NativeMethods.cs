using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation
{
    public static class NativeMethods
    {
        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        public static extern int MapVirtualKey(int uCode, int uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendInput(int nInput, ref NativeMethods.INPUT pInput, int cbSize);

        [DllImport("user32.dll")]
        public static extern short VkKeyScan(char ch);

        public const int InputKeyboard = 1;
        public const int InputMouse = 0;
        public const int KeyeventfExtendedkey = 1;
        public const int KeyeventfKeyup = 2;
        public const int KeyeventfScancode = 8;
        public const int MouseeventfVirtualdesk = 16384;
        public const int SMCxvirtualscreen = 78;
        public const int SMCyvirtualscreen = 79;
        public const int SMXvirtualscreen = 76;
        public const int SMYvirtualscreen = 77;
        public const int VKeyCharMask = 255;
        public const int VKeyShiftMask = 256;
        public const int WheelDelta = 120;
        public const int XButton1 = 1;
        public const int XButton2 = 2;
        public struct INPUT
        {
            public int type;

            public NativeMethods.INPUTUNION union;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)]
            public NativeMethods.MOUSEINPUT mouseInput;

            [FieldOffset(0)]
            public NativeMethods.KEYBDINPUT keyboardInput;
        }

        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int Time;
            public IntPtr dwExtraInfo;
        }

        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int Time;
            public IntPtr dwExtraInfo;
        }

        [Flags]
        public enum SendMouseInputFlags
        {
            Move = 1,
            LeftDown = 2,
            LeftUp = 4,
            RightDown = 8,
            RightUp = 16,
            MiddleDown = 32,
            MiddleUp = 64,
            XDown = 128,
            XUp = 256,
            Wheel = 2048,
            Absolute = 32768
        }
    }
}
