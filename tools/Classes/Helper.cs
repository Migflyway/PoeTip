using System;
using System.Runtime.InteropServices;

namespace tools
{
    class Helper
    {
        #region CLIPBOARD API

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public static int WM_CLIPBOARDUPDATE = 0x031D;

        /// <summary>
        /// ANSI text. A null character signals the end of the data.
        /// </summary>
        private static int CF_TEXT = 1;
        /// <summary>
        /// OEM character text. A null character signals the end of the data.
        /// </summary>
        private static int CF_OEMTEXT = 7;
        /// <summary>
        /// Unicode text format. A null character signals the end of the data.
        /// </summary>
        private static int CF_UNICODETEXT = 13;

        #endregion


        public static void SetClipboard(IntPtr handle, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (OpenClipboard(handle))
                {// Open the clipboard, and empty it. 
                    EmptyClipboard();

                    IntPtr hGlobal = Marshal.StringToHGlobalUni(text);
                    // Place the handle on the clipboard. 
                    SetClipboardData(CF_UNICODETEXT, hGlobal);
                    CloseClipboard();

                    if(hGlobal!=IntPtr.Zero)
                        Marshal.FreeHGlobal(hGlobal);
                }
            }

        }


        public static string GetBetweenText(string text, string leftstr, string rightstr)
        {
            int startIndex = 0;
            return GetBetweenText(text, leftstr, rightstr, ref startIndex);
        }
        public static string GetBetweenText(string text, string leftstr, string rightstr, ref int startIndex)
        {
            string temp = string.Empty;
            int i = text.IndexOf(leftstr, startIndex);
            if (i != -1)
            {
                i += leftstr.Length;
                int n = text.IndexOf(rightstr, i);
                if (n != -1)
                {
                    startIndex = n + rightstr.Length;
                    temp = text.Substring(i, n - i);
                }
            }
            return temp;
        }


    }


    public class ItemInfo
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int MapTier { get; set; }
        public bool Unidentified { get; set; }
        public ItemFlag Flag { get; set; }

        public bool isScarable { get; set; }

        // 是否为遗产
        public bool IsRelic_Unique { get; set; }
    }

    [Flags]
    public enum ItemFlag
    {
        /// <summary>
        /// 暗金
        /// </summary>
        Unique = 1,
        /// <summary>
        /// 地图
        /// </summary>
        Map = 2,
        /// <summary>
        /// 首饰
        /// </summary>
        Accessories = 4,
        /// <summary>
        /// 胸甲
        /// </summary>
        Armours = 8,
        /// <summary>
        /// 药剂
        /// </summary>
        Flasks = 16,
        /// <summary>
        /// 珠宝
        /// </summary>
        Jewels = 32,
        /// <summary>
        /// 武器
        /// </summary>
        Weapons = 64,
        /// <summary>
        /// 裂痕石
        /// </summary>
        Breachstone = 128,
        /// <summary>
        /// 碎片
        /// </summary>
        Fragments = 256,
        /// <summary>
        /// 精华
        /// </summary>
        Essence = 512,
        /// <summary>
        /// 命运卡
        /// </summary>
        Divination = 1024,
        /// <summary>
        /// 通货
        /// </summary>
        Currency = 2048,
        /// <summary>
        /// 预言
        /// </summary>
        Prophecies = 4096,
        /// <summary>
        /// 普通
        /// </summary>
        Normal = 8192,

        Resonator = 16384,

        Fossil = 32768,

        Scarab = 65536,
    }


}
