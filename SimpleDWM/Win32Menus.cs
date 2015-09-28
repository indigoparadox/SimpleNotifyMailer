using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDWM {
    public class Win32Menus {

        public const int MF_STRING = 0x0;
        public const int MF_SEPARATOR = 0x800;

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr GetSystemMenu( IntPtr hWnd, bool bRevert );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool AppendMenu( IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool InsertMenu( IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem );
    }
}
