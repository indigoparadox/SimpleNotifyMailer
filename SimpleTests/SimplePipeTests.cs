using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace SimpleUtils {
    [TestClass]
    public class SimplePipeTests {

        private static readonly int TEST_CLIENT_THREAD_COUNT = 3;

        private class ServerTest : IDisposable {

            private SimplePipeServer server;
            private Thread readLoopThread;
            private bool serverThreadRunning = true;

            public bool RecievedFromClient { get; set; }

            private void TestSimplePipeServerThread() {
                while( this.serverThreadRunning ) {
                    string readString = this.server.Read();
                    if( !String.IsNullOrEmpty( readString ) ) {
                        this.RecievedFromClient = true;
                    }
                }
            }

            public ServerTest( string pipeNameIn ) {
                this.server = new SimplePipeServer( pipeNameIn );
                this.server.Listen();

                this.readLoopThread = new Thread( this.TestSimplePipeServerThread );
                this.readLoopThread.Start();
            }

            public void Dispose() {
                this.serverThreadRunning = false;
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
        public void TestSimpleClientToServer() {

            string pipeName = "SimplePipeTestPipe";

            using( ServerTest testServer = new ServerTest(pipeName) ) {

                SimplePipeClient client = new SimplePipeClient( pipeName );
                client.Connect();
                client.Write( "Test" );

                Assert.IsTrue( testServer.RecievedFromClient );
            }
        }
    }
}
