using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleUtils {
    public class SimplePipeServer : SimplePipePeer {

        private Thread listeningThread = null;

        public SimplePipeServer( string pipeNameIn ) : base( pipeNameIn ) {
        }

        public void Listen() {
            if( null == this.listeningThread ) {
                this.listeningThread = new Thread( this.ListenThread );
            }
        }

        protected void ListenThread() {
            SafeFileHandle clientPipeHandle = null;
            while( this.active ) {
                if( null != this.pipeStream && this.connected ) {
                    Thread.Sleep( 1000 );
                    continue;
                }

                clientPipeHandle = CreateNamedPipe(
                    FormatPipeName( this.pipeName ),
                    DUPLEX | FILE_FLAG_OVERLAPPED,
                    0,
                    255,
                    BUFFER_SIZE,
                    BUFFER_SIZE,
                    0,
                    IntPtr.Zero
                );

                if( clientPipeHandle.IsInvalid ) {
                    throw new SimplePipeConnectionException( "Failed to create named pipe." );
                }

                int success = ConnectNamedPipe( clientPipeHandle, IntPtr.Zero );

                if( 1 != success ) {
                    throw new SimplePipeConnectionException( "Failed to connect client pipe." );
                }

                this.pipeStream = new FileStream( clientPipeHandle, FileAccess.ReadWrite, (int)BUFFER_SIZE, true );
                this.connected = true;
            }
        }
    }
}
