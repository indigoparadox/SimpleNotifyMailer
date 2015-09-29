using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDWM {
    public class Win32Dwm {

        protected enum DwmNCRenderingPolicy {
            DWMNCRP_USEWINDOWSTYLE, // Enable/disable non-client rendering based on window style
            DWMNCRP_DISABLED, // Disabled non-client rendering; window style is ignored
            DWMNCRP_ENABLED, // Enabled non-client rendering; window style is ignored
            DWMNCRP_LAST
        }

        [Flags]
        protected enum DwmWindowAttribute : uint {
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
        protected struct Rect {
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
        protected struct ThumbnailProperties {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        public enum TaskbarStates {
            NoProgress = 0,
            Indeterminate = 0x1,
            Normal = 0x2,
            Error = 0x4,
            Paused = 0x8
        }

        [ComImportAttribute()]
        [GuidAttribute( "ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf" )]
        [InterfaceTypeAttribute( ComInterfaceType.InterfaceIsIUnknown )]
        protected interface ITaskbarList3 {
            // ITaskbarList
            [PreserveSig]
            void HrInit();
            [PreserveSig]
            void AddTab( IntPtr hwnd );
            [PreserveSig]
            void DeleteTab( IntPtr hwnd );
            [PreserveSig]
            void ActivateTab( IntPtr hwnd );
            [PreserveSig]
            void SetActiveAlt( IntPtr hwnd );

            // ITaskbarList2
            [PreserveSig]
            void MarkFullscreenWindow( IntPtr hwnd, [MarshalAs( UnmanagedType.Bool )] bool fFullscreen );

            // ITaskbarList3
            [PreserveSig]
            void SetProgressValue( IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal );
            [PreserveSig]
            void SetProgressState( IntPtr hwnd, TaskbarStates state );
        }

        [GuidAttribute( "56FDF344-FD6D-11d0-958A-006097C9A090" )]
        [ClassInterfaceAttribute( ClassInterfaceType.None )]
        [ComImportAttribute()]
        protected class TaskbarInstance {
        }

        public const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;
        public const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;

        protected static ITaskbarList3 taskbarInstance = (ITaskbarList3)new TaskbarInstance();
        protected static bool taskbarSupported = Environment.OSVersion.Version >= new Version( 6, 1 );

        [DllImport( "dwmapi.dll", PreserveSig = true )]
        protected static extern int DwmSetWindowAttribute( IntPtr hwnd, DwmWindowAttribute attr, IntPtr attrValue, int attrSize );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        protected static extern void DwmRegisterThumbnail( IntPtr destinationWindowHandle, IntPtr sourceWindowHandle, out IntPtr thumbnailHandle );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        protected static extern void DwmUnregisterThumbnail( IntPtr thumbnailHandle );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        protected static extern void DwmUpdateThumbnailProperties( IntPtr thumbnailHandle, ref ThumbnailProperties properties );

        [DllImport( "dwmapi.dll" )]
        protected static extern int DwmSetIconicLivePreviewBitmap( IntPtr hwnd, IntPtr hbitmap, IntPtr ptClient, uint flags );

        [DllImport( "dwmapi.dll" )]
        protected static extern int DwmSetIconicThumbnail( IntPtr hwnd, IntPtr hbitmap, uint flags );

        [DllImport( "dwmapi.dll" )]
        protected static extern int DwmInvalidateIconicBitmaps( IntPtr hwnd );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        [return: MarshalAs( UnmanagedType.Bool )]
        protected static extern bool DwmIsCompositionEnabled();

        public static void EnableCustomThumbnail( IntPtr hWnd ) {
            IntPtr iTrue = Marshal.AllocHGlobal( sizeof( int ) );
            Marshal.WriteInt32( iTrue, 1 );

            Win32Dwm.DwmSetWindowAttribute(
                hWnd,
                Win32Dwm.DwmWindowAttribute.DWMWA_FORCE_ICONIC_REPRESENTATION,
                iTrue,
                sizeof( int )
            );
            Win32Dwm.DwmSetWindowAttribute(
                hWnd,
                Win32Dwm.DwmWindowAttribute.DWMWA_HAS_ICONIC_BITMAP,
                iTrue,
                sizeof( int )
            );
        }

        public static void DisableCustomThumbnail( IntPtr hWnd ) {
            DwmInvalidateIconicBitmaps( hWnd );
        }

        public static void DisablePeek( IntPtr hWnd ) {
            if( DwmIsCompositionEnabled() ) {
                IntPtr iStatus = Marshal.AllocHGlobal( sizeof( int ) );
                Marshal.WriteInt32( iStatus, 1 );
                DwmSetWindowAttribute(
                    hWnd,
                    DwmWindowAttribute.DWMWA_EXCLUDED_FROM_PEEK,
                    iStatus,
                    sizeof( int )
                );
                DwmSetWindowAttribute(
                    hWnd,
                    DwmWindowAttribute.DWMWA_DISALLOW_PEEK,
                    iStatus,
                    sizeof( int )
                );
            }
        }

        public static void InvalidateCustomThumbnail( IntPtr hWnd ) {
            DwmInvalidateIconicBitmaps( hWnd );
        }

        public static void SetCustomThumbnail( IntPtr hWnd, Bitmap thumbnail ) {
            DwmSetIconicThumbnail( hWnd, thumbnail.GetHbitmap(), 0 );
        }

        public static void SetCustomLivePreview( IntPtr hWnd, Bitmap livePreview ) {
            DwmSetIconicLivePreviewBitmap( hWnd, livePreview.GetHbitmap(), IntPtr.Zero, 0 );
        }

        public static void SetTaskbarState( IntPtr windowHandle, TaskbarStates taskbarState ) {
            if( taskbarSupported ) taskbarInstance.SetProgressState( windowHandle, taskbarState );
        }

        public static void SetTaskbarState( IntPtr windowHandle, double progressValue, double progressMax ) {
            if( taskbarSupported ) taskbarInstance.SetProgressValue( windowHandle, (ulong)progressValue, (ulong)progressMax );
        }
    }
}
