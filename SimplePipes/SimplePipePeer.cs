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

    public enum SimplePipeAccessLevel {
        Default,
        AuthenticatedUsers
    }

    public abstract class SimplePipePeer {

        protected const uint BUFFER_SIZE = 4096;

        public delegate void OnReadCallbackDelegate( string messageIn );

        public OnReadCallbackDelegate OnReadCallback { get; set; }

        protected string pipeName;
        protected bool active = true;
        protected bool connected = false;
        protected bool client = true; // Are we a client? Should be overridden by servers.
        protected SimplePipeAccessLevel accessLevel = SimplePipeAccessLevel.Default;
        
        /// <summary>
        /// The duration (in seconds) to wait before timing out a connection.
        /// 
        /// -1 means wait forever.
        /// </summary>
        public int ConnectionTimeout { get; set; }

        public SimplePipePeer( string pipeNameIn ) {
            this.pipeName = pipeNameIn;

            this.OnReadCallback = null;

            this.ConnectionTimeout = -1;
        }

        public static string FormatPipeName( string pipeNameIn, bool returnPipe ) {
            return String.Format( "\\\\.\\pipe\\{0}{1}", pipeNameIn, returnPipe ? "Return" : "" );
        }

        protected NamedPipeServerStream CreateServerPipe( string pipePathIn ) {
            NamedPipeServerStream newPipe = new NamedPipeServerStream(
                pipePathIn,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                0,
                0,
                null,
                HandleInheritability.None,
                PipeAccessRights.ChangePermissions
            );

            if( SimplePipeAccessLevel.Default != this.accessLevel ) {

                PipeSecurity listenerAccess = newPipe.GetAccessControl();

                switch( this.accessLevel ) {
                    case SimplePipeAccessLevel.AuthenticatedUsers:
                        listenerAccess.AddAccessRule( new PipeAccessRule(
                            new SecurityIdentifier( WellKnownSidType.AuthenticatedUserSid, null ),
                            PipeAccessRights.ReadWrite,
                            AccessControlType.Allow
                        ) );
                        break;

                    default:
                        // This shouldn't happen.
                        break;
                }

                // Always allow the owner full control.
                listenerAccess.AddAccessRule( new PipeAccessRule(
                    WindowsIdentity.GetCurrent().Owner,
                    PipeAccessRights.FullControl,
                    AccessControlType.Allow
                ) );

                newPipe.SetAccessControl( listenerAccess );
            }

            return newPipe;
        }

        public void Write( string messageIn, bool waitSync ) {
            this.Write( Encoding.UTF8.GetBytes( messageIn ), waitSync );
        }

        public void Write( byte[] bufferIn, bool waitSync ) {
            try {
                NamedPipeClientStream pipeClientLocal = new NamedPipeClientStream(
                    ".",
                    SimplePipePeer.FormatPipeName( this.pipeName, !this.client ),
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous
                );

                // The connect function will indefinitely wait for the pipe to become available.\
                // TODO: Is this thread-safe? I don't think it is.
                int localTimeoutCountdown = 0;
                do {
                    try {
                        pipeClientLocal.Connect( 1000 );
                        this.connected = true;
                    } catch( IOException ex ) {
                        localTimeoutCountdown++;
                        if( 0 <= this.ConnectionTimeout && this.ConnectionTimeout <= localTimeoutCountdown ) {
                            throw new TimeoutException( ex.Message );
                        }
                    }
                } while( !this.connected );

                if( waitSync ) {
                    pipeClientLocal.Write( bufferIn, 0, bufferIn.Length );
                    pipeClientLocal.Flush();
                    pipeClientLocal.Close();
                    pipeClientLocal.Dispose();
                } else {
                    // Just kick off the process and finish it below.
                    pipeClientLocal.BeginWrite( bufferIn, 0, bufferIn.Length, new AsyncCallback( this.OnWrite ), pipeClientLocal );
                }
            } catch( TimeoutException ex ) {
                // TODO: Does this still execute finally{} below?
                throw ex;
            } finally {
                this.connected = false;
            }
        }

        public void Listen() {
            try {
                string pipeName = SimplePipePeer.FormatPipeName( this.pipeName, this.client );
                NamedPipeServerStream pipeServerLocal = CreateServerPipe( pipeName );
                pipeServerLocal.BeginWaitForConnection( new AsyncCallback( this.OnConnection ), pipeServerLocal );
            } catch( Exception ex ) {
                Debug.WriteLine( ex.Message );
            }
        }

        public void Stop() {
            this.active = false;
        }

        /// <summary>
        ///
        /// </summary>
        public void WaitForPeers() {
            while( this.connected ) {
                Thread.Sleep( 100 );
            }
        }

        protected void OnConnection( IAsyncResult iarIn ) {
            this.connected = true;
            NamedPipeServerStream pipeServerLocal = (NamedPipeServerStream)iarIn.AsyncState;
            pipeServerLocal.EndWaitForConnection( iarIn );

            // Read the message and execute the callback on it.
            byte[] buffer = new byte[BUFFER_SIZE];
            pipeServerLocal.Read( buffer, 0, buffer.Length );
            this.OnRead( Encoding.UTF8.GetString( buffer ) );

            pipeServerLocal.Close();
            this.connected = false;

            if( !this.active ) {
                // Don't continue listening.
                return;
            }

            // Create a new recursive wait server.
            pipeServerLocal = CreateServerPipe(
                SimplePipePeer.FormatPipeName( this.pipeName, this.client )
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
