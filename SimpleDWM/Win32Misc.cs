using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDWM {
    public class Win32Misc {
        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_RESTORE = 0xF120;

        public static uint HiWord( UInt32 inp ) {
            return (inp >> 16);
        }

        public static uint LoWord( UInt32 inp ) {
            return (inp & 0xffff);
        }
    }
}
