using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
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

        public delegate void OnReadCallbackDelegate( string messageIn );

        public OnReadCallbackDelegate OnReadCallback { get; set; }

        protected string pipeName;
        protected bool active = true;
        protected bool connected = false;
        protected bool client = true; // Are we a client? Should be overridden by servers.

        public SimplePipePeer( string pipeNameIn ) {
            this.pipeName = pipeNameIn;

            this.OnReadCallback = null;
        }

        public static string FormatPipeName( string pipeNameIn, bool returnPipe ) {
            return String.Format( "\\\\.\\pipe\\{0}{1}", pipeNameIn, returnPipe ? "Return" : "" );
        }

#if false
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
#endif

        public void Write( string messageIn, bool waitSync ) {
            try {
                NamedPipeClientStream pipeClientLocal = new NamedPipeClientStream(
                    ".",
                    SimplePipePeer.FormatPipeName( this.pipeName, !this.client ),
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous
                );

                // The connect function will indefinitely wait for the pipe to become available.
                pipeClientLocal.Connect();

                byte[] buffer = Encoding.UTF8.GetBytes( messageIn );
                if( waitSync ) {
                    pipeClientLocal.Write( buffer, 0, buffer.Length );
                    pipeClientLocal.Flush();
                    pipeClientLocal.Close();
                    pipeClientLocal.Dispose();
                } else {
                    // Just kick off the process and finish it below.
                    pipeClientLocal.BeginWrite( buffer, 0, buffer.Length, new AsyncCallback( this.OnWrite ), pipeClientLocal );
                }
            } catch( TimeoutException ex ) {
                // TODO
                Debug.WriteLine( ex.Message );
            }
        }

        public void Listen() {
            try {
                string pipeName = SimplePipePeer.FormatPipeName( this.pipeName, this.client );
                NamedPipeServerStream pipeServerLocal = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous
                );
                pipeServerLocal.BeginWaitForConnection( new AsyncCallback( this.OnConnection ), pipeServerLocal );
            } catch( Exception ex ) {
                Debug.WriteLine( ex.Message );
            }
        }

        public void Stop() {
            this.active = false;
        }

        protected void OnConnection( IAsyncResult iarIn ) {
            NamedPipeServerStream pipeServerLocal = (NamedPipeServerStream)iarIn.AsyncState;
            pipeServerLocal.EndWaitForConnection( iarIn );

            // Read the message and execute the callback on it.
            byte[] buffer = new byte[BUFFER_SIZE];
            pipeServerLocal.Read( buffer, 0, buffer.Length );
            this.OnRead( Encoding.UTF8.GetString( buffer ) );

            pipeServerLocal.Close();

            if( !this.active ) {
                // Don't continue listening.
                return;
            }

            // Create a new recursive wait server.
            pipeServerLocal = new NamedPipeServerStream(
                SimplePipePeer.FormatPipeName( this.pipeName, this.client ),
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous
            );
            pipeServerLocal.BeginWaitForConnection( new AsyncCallback( this.OnConnection ), pipeServerLocal );
        }

        protected void OnRead( string messageIn ) {
            StringBuilder messageOut = new StringBuilder();

            // Strip stray nulls off.
            // TODO: TypeUtils?
            for( int i = 0 ; messageIn.Length > i ; i++ ) {
                if( '\0' == messageIn[i] ) {
                    break;
                } else {
                    messageOut.Append( messageIn[i] );
                }
            }

            if( null != this.OnReadCallback ) {
                this.OnReadCallback( messageOut.ToString() );
            }
        }

        protected void OnWrite( IAsyncResult iarIn ) {
            try {
                // Get the pipe.
                NamedPipeClientStream pipeClientLocal = (NamedPipeClientStream)iarIn.AsyncState;

                // End the write.
                pipeClientLocal.EndWrite( iarIn );
                pipeClientLocal.Flush();
                pipeClientLocal.Close();
                pipeClientLocal.Dispose();
            } catch( Exception ex ) {
                Debug.WriteLine( ex.Message );
            }
        }
    }
}
