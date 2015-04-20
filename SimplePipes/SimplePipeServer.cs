using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleUtils {
    /// <summary>
    /// This server will wait for clients to connect and then read a code from
    /// the connected pipe. This code will then be opened as a pipe to write
    /// back to the client.
    /// </summary>
    public class SimplePipeServer : SimplePipePeer {

        private Dictionary<string, NamedPipeClientStream> clientPipeHandles = new Dictionary<string, NamedPipeClientStream>();
        private Dictionary<string, StreamWriter> clientPipeStreamHandles = new Dictionary<string, StreamWriter>();
        private Dictionary<string, NamedPipeServerStream> serverPipeHandles = new Dictionary<string, NamedPipeServerStream>();
        private Dictionary<string, StreamReader> serverPipeStreamHandles = new Dictionary<string, StreamReader>();

        public SimplePipeServer( string pipeNameIn ) : base( pipeNameIn ) {
            this.pipeServer = SimplePipePeer.OpenReadServerPipe( SimplePipePeer.FormatPipeName( pipeNameIn, true ) );
            this.pipeServerStream = new StreamReader( this.pipeServer );
        }

        public void Listen() {
            if( null == this.listeningThread ) {
                this.listeningThread = new Thread( this.ListenThread );
            }
        }

        protected void ListenThread() {
            while( this.active ) {
                this.pipeServer.WaitForConnection();
                try {
                    while( true ) {// Loop until we get a disconnect exception or break.
                        string readLine = this.pipeServerStream.ReadLine();
                        if( readLine.StartsWith( "$SimplePipeCommand" ) ) {
                            string[] readCommandArray = readLine.Split( '.' );
                            if( readCommandArray[1].Equals( "NewPipe" ) ) {
                                this.OpenPeerPipes( readCommandArray[2] );
                                // TODO: Waiting here is a bad idea.
                                this.serverPipeHandles[readCommandArray[2]].WaitForConnection();
                                this.clientPipeHandles[readCommandArray[2]].Connect();
                            }
                        }
                    }
                } catch( IOException ) {
                    // TODO
                }
            }
        }

        public override void Write( string messageIn ) {
            throw new NotImplementedException();
        }

        protected override void OpenPeerPipes( string clientCodeIn ) {
            //Debug.Write( clientCodeIn );

            this.serverPipeHandles.Add( clientCodeIn, SimplePipePeer.OpenReadServerPipe( SimplePipePeer.FormatPipeName( this.pipeName, false ) + "Server" + clientCodeIn ) );
            this.serverPipeStreamHandles.Add( clientCodeIn, new StreamReader( this.serverPipeHandles[clientCodeIn] ) );

            this.clientPipeHandles.Add( clientCodeIn, new NamedPipeClientStream( SimplePipePeer.FormatPipeName( this.pipeName, false ) + "Client" + clientCodeIn ) );
            this.clientPipeStreamHandles.Add( clientCodeIn, new StreamWriter( this.clientPipeHandles[clientCodeIn] ) );
        }
    }
}
