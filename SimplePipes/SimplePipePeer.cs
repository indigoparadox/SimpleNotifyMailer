using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleUtils {
    public class SimplePipeConnectionException : Exception {
        public SimplePipeConnectionException( string messageIn ) : base( messageIn ) { }
    }

    public class SimplePipePeer {

        [DllImport( "kernel32.dll", SetLastError = true )]
        protected static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes
        );

        [DllImport( "kernel32.dll", SetLastError = true )]
        protected static extern int ConnectNamedPipe( SafeFileHandle hNamedPipe, IntPtr lpOverlapped );

        [DllImport( "kernel32.dll", SetLastError = true )]
        protected static extern SafeFileHandle CreateFile(
           String pipeName,
           uint dwDesiredAccess,
           uint dwShareMode,
           IntPtr lpSecurityAttributes,
           uint dwCreationDisposition,
           uint dwFlagsAndAttributes,
           IntPtr hTemplate );

        protected const uint DUPLEX = (0x00000003);
        protected const uint FILE_FLAG_OVERLAPPED = (0x40000000);
        protected const uint GENERIC_READ = (0x80000000);
        protected const uint GENERIC_WRITE = (0x40000000);
        protected const uint OPEN_EXISTING = 3;

        protected const uint BUFFER_SIZE = 4096;

        protected string pipeName;
        protected bool active = true;
        protected FileStream pipeStream = null;
        protected bool connected = false;

        public SimplePipePeer( string pipeNameIn ) {
            this.pipeName = pipeNameIn;
        }

        protected static string FormatPipeName( string pipeNameIn ) {
            return String.Format( "\\\\.\\pipe\\{0}", pipeNameIn );
        }

        public string Read() {
            byte[] buffer = new byte[BUFFER_SIZE];

            int bytesRead = this.pipeStream.Read( buffer, 0, (int)BUFFER_SIZE );

            // Could not read from file stream;
            if( bytesRead == 0 ) {
                this.connected = false;
                return null;
            }

            return Encoding.UTF8.GetString( buffer );
        }

        public void Write( string message ) {
            if( null != this.pipeStream ) {
                byte[] sendBuffer = Encoding.UTF8.GetBytes( message ); try {
                    this.pipeStream.Write( sendBuffer, 0, sendBuffer.Length );
                } catch( IOException ) {
                    this.connected = false;
                }
                this.pipeStream.Flush();
            }
        }

        public void Stop() {
            this.active = false;
            
            // TODO
        }
    }
}
