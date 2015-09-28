using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDWM {
    public class Win32Dwm {

        [Flags]
        public enum DwmWindowAttribute : uint {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct Rect {
            internal Rect( int left, int top, int right, int bottom ) {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct ThumbnailProperties {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        public const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;
        public const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;

        [DllImport( "dwmapi.dll", PreserveSig = true )]
        public static extern int DwmSetWindowAttribute( IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        public static extern void DwmRegisterThumbnail( IntPtr destinationWindowHandle, IntPtr sourceWindowHandle, out IntPtr thumbnailHandle );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        public static extern void DwmUnregisterThumbnail( IntPtr thumbnailHandle );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        public static extern void DwmUpdateThumbnailProperties( IntPtr thumbnailHandle, ref ThumbnailProperties properties );

        [DllImport( "dwmapi.dll" )]
        public static extern int DwmSetIconicLivePreviewBitmap( IntPtr hwnd, IntPtr hbitmap, IntPtr ptClient, uint flags );

        [DllImport( "dwmapi.dll" )]
        public static extern int DwmSetIconicThumbnail( IntPtr hwnd, IntPtr hbitmap, uint flags );

        [DllImport( "dwmapi.dll" )]
        public static extern int DwmInvalidateIconicBitmaps( IntPtr hwnd );
    }
}
