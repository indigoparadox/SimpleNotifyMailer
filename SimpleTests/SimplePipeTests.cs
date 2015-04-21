using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;

namespace SimpleUtils {
    [TestClass]
    public class SimplePipeTests {

        private static readonly int TEST_CLIENT_THREAD_COUNT = 3;

        private class ServerTest : IDisposable {

            private SimplePipeServer server;

            public bool RecievedFromClient { get; set; }

            public ServerTest( string pipeNameIn ) {
                this.server = new SimplePipeServer( pipeNameIn );
                this.server.OnReadCallback = this.OnRead;
                this.server.Listen();
            }

            public void Write( string messageIn ) {
                this.server.Write( messageIn, true );
            }

            private void OnRead( string messageIn ) {
                if( messageIn.Equals( "Test" ) ) {
                    this.RecievedFromClient = true;
                }

                Debug.WriteLine( messageIn );
            }

            public void Dispose() {
                this.server.Stop();
            }
        }

        public SimplePipeTests() {
        }

        private void TestSimplePipeClientThread( object state ) {
            bool sendTestString = (bool)state;


        }

        [TestMethod]
        public void TestSimplePipes() {

#if false
            // TODO: Create client threads and read-write between them.
            Thread[] clientThreads = new Thread[TEST_CLIENT_THREAD_COUNT];
            for( int i = 0 ; TEST_CLIENT_THREAD_COUNT > i ; i++ ) {
                clientThreads[i] = new Thread( this.TestSimplePipeClientThread );
                if( (TEST_CLIENT_THREAD_COUNT - 1) == i ) {
                    // Only send from the last thread.
                    clientThreads[i].Start( true );
                } else {
                    clientThreads[i].Start( false );
                }
            }
            
            // TODO: Cycle until all threads are complete.
#endif

        }

        [TestMethod]
        public void TestSimplePipeClientToServer() {

            string pipeName = "SimplePipeTestPipe";

            using( ServerTest testServer = new ServerTest(pipeName) ) {

                SimplePipeClient client = new SimplePipeClient( pipeName );
                //client.Connect();
                client.Write( "Test", true );

                // TODO: Actually wait until server has nothing else to read.
                //Thread.Sleep( 2000 );

                Assert.IsTrue( testServer.RecievedFromClient );
            }
        }

        [TestMethod]
        public void TestSimplePipeServerToClient() {

            string pipeName = "SimplePipeTestPipe";
            bool clientRead = false;

            using( ServerTest testServer = new ServerTest( pipeName ) ) {

                SimplePipeClient client = new SimplePipeClient( pipeName );
                client.OnReadCallback += delegate( string messageIn ) { clientRead = true; };
                client.Listen();
                testServer.Write( "Test" );

                // TODO: Actually wait until server has nothing else to read.
                //Thread.Sleep( 2000 );

                Assert.IsTrue( clientRead );
            }
        }
    }
}
