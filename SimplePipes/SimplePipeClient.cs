using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace SimpleUtils {
    public class SimplePipeClient : SimplePipePeer {

        protected NamedPipeClientStream pipeClient = null;
        protected StreamWriter pipeClientStream = null;
        protected string clientID;
        protected bool connected = false;

        public SimplePipeClient( string pipeNameIn ) : base( pipeNameIn ) { 

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            this.clientID = new string(
                Enumerable.Repeat( chars, 8 )
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray()
            );

            

        }

        public void Connect() {
            this.pipeClient = new NamedPipeClientStream( SimplePipePeer.FormatPipeName( this.pipeName, true ) );
            this.pipeClient.Connect();
            this.pipeClientStream = new StreamWriter( this.pipeClient );
            this.pipeClientStream.WriteLine( "$SimplePipeCommand.NewPipe." + this.clientID );
            this.pipeClient.Dispose();
            
            
        }

        protected override void OpenPeerPipes( string clientCodeIn ) {
            Debug.Write( clientCodeIn );

            this.pipeServer = SimplePipePeer.OpenReadServerPipe( SimplePipePeer.FormatPipeName( this.pipeName, false ) + this.clientID );
            this.pipeServerStream = new StreamReader( this.pipeServer );

            this.pipeClient = new NamedPipeClientStream( SimplePipePeer.FormatPipeName( this.pipeName, false ) + "Server" + this.clientID );
            this.pipeClientStream = new StreamWriter( this.pipeClient );
            this.pipeClient.Connect();

            this.pipeServer.WaitForConnection();
        }

        public override void Write( string messageIn ) {
            this.pipeClientStream.WriteLine( messageIn );
        }

#if false
        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if successfully connected, false otherwise.</returns>
        public bool Connect() {
            SafeFileHandle pipeHandle = CreateFile(
                FormatPipeName(this.pipeName),
                GENERIC_READ | GENERIC_WRITE,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_FLAG_OVERLAPPED,
                IntPtr.Zero
            );

            //could not get a handle to the named pipe
            if( pipeHandle.IsInvalid ) {
                return false;
            }

            this.pipeStream = new FileStream( pipeHandle, FileAccess.ReadWrite, (int)BUFFER_SIZE, true );

            return true;
        }
#endif
    }
}
