using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace SimpleUtils {
    public class SimplePipeConnectionException : Exception {
        public SimplePipeConnectionException( string messageIn ) : base( messageIn ) { }
    }

    public abstract class SimplePipePeer {

        protected const uint BUFFER_SIZE = 4096;

        protected Thread listeningThread = null;
        protected NamedPipeServerStream pipeServer = null;
        protected StreamReader pipeServerStream = null;
        protected string pipeName;
        protected bool active = true;
        protected bool connected = false;

        public SimplePipePeer( string pipeNameIn ) {
            this.pipeName = pipeNameIn;
        }

        public static string FormatPipeName( string pipeNameIn, bool server ) {
            return String.Format( "\\\\.\\pipe\\{0}{1}", pipeNameIn, server ? "Server" : "" );
        }

        protected static NamedPipeServerStream OpenReadServerPipe(string pipePathIn) {
            NamedPipeServerStream newPipe = new NamedPipeServerStream(
                pipePathIn,
                PipeDirection.InOut,
                100,
                PipeTransmissionMode.Message
            );
            
#if USE_PIPE_ACL
            // Allow normal users to talk to us.
            PipeSecurity listenerAccess = newPipe.GetAccessControl();
            listenerAccess.AddAccessRule( new PipeAccessRule(
                new SecurityIdentifier( WellKnownSidType.AuthenticatedUserSid, null ),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow
            ) );
            listenerAccess.AddAccessRule( new PipeAccessRule(
                WindowsIdentity.GetCurrent().Owner,
                PipeAccessRights.FullControl,
                AccessControlType.Allow
            ) );
            newPipe.SetAccessControl( listenerAccess );
#endif // USE_PIPE_ACL

            return newPipe;
        }

        public string Read() {
            // XXX
            do {
                try {
                    
                } catch( InvalidOperationException ) {
                    this.connected = false;
                    Thread.Sleep( 500 );
                } catch( Exception ex ) {
                    // TODO
                    Debug.WriteLine( ex.Message );
                }
            } while( true ); // Read until we can return above.
        }

        public abstract void Write( string messageIn );
        protected abstract void OpenPeerPipes( string clientCodeIn );

#if false
        public string Read() {
            return this.Read( null );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientCodeIn">The index of the pipe to read. null returns the first pipe with waiting input.</param>
        /// <returns>The line read from the requested pipe.</returns>
        public abstract string Read( string clientCodeIn );

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
#endif

        public void Stop() {
            this.active = false;
            
            // TODO
        }
    }
}
