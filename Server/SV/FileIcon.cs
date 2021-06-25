using System;
using System.Runtime.InteropServices;

namespace SV
{
    class FileIcon
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags);
        private const int SHGFI_ICON = 0x100;
        private const int SHGFI_USEFILEATTRIBUTES = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public const int MAX_PATH = 260;
            public const int NAMESIZE = 80;
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAMESIZE)]
            public string szTypeName;
        };

        public enum IconSize
        {
            SHGFI_LARGEICON = 0x0,
            SHGFI_SMALLICON = 0x1
        }
        public static System.Drawing.Icon GetFileIcon(string name, IconSize size)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;



            /* Check the size specified for return. */
            if (IconSize.SHGFI_SMALLICON == size)
            {
                flags += (uint)IconSize.SHGFI_SMALLICON; // include the small icon flag
            }
            else
            {
                flags += (uint)IconSize.SHGFI_LARGEICON;  // include the large icon flag
            }

            SHGetFileInfo(name,
                FILE_ATTRIBUTE_NORMAL,
                ref shfi,
                Marshal.SizeOf(shfi),
                (int)flags);


            // Copy (clone) the returned icon to a new object, thus allowing us 
            // to call DestroyIcon immediately
            System.Drawing.Icon icon = (System.Drawing.Icon)
                                 System.Drawing.Icon.FromHandle(shfi.hIcon).Clone();
            DestroyIcon(shfi.hIcon); // Cleanup
            return icon;
        }
    }
}
