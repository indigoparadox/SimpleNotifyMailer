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

            public int RecievedFromClient { get; set; }

            public ServerTest( string pipeNameIn ) {
                this.RecievedFromClient = 0;
                this.server = new SimplePipeServer( pipeNameIn );
                this.server.OnReadCallback = this.OnRead;
                this.server.Listen();
            }

            public void Write( string messageIn ) {
                this.server.Write( messageIn, true );
            }

            private void OnRead( string messageIn ) {
                if( messageIn.StartsWith( "Test" ) ) {
                    this.RecievedFromClient += 1;
                }

                Debug.WriteLine( messageIn );
            }

            public void WaitForClients() {
                this.server.WaitForPeers();
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

            string pipeName = "SimplePipeTestPipeCTS";

            using( ServerTest testServer = new ServerTest(pipeName) ) {

                SimplePipeClient client = new SimplePipeClient( pipeName );
                //client.Connect();
                client.Write( "Test", true );

                // TODO: Actually wait until server has nothing else to read.
                //Thread.Sleep( 2000 );

                testServer.WaitForClients();

                Assert.IsTrue( 0 < testServer.RecievedFromClient );
            }
        }

        [TestMethod]
        public void TestSimplePipeClientToServerMultiple() {

            string pipeName = "SimplePipeTestPipeCTSM";

            using( ServerTest testServer = new ServerTest( pipeName ) ) {

                SimplePipeClient client = new SimplePipeClient( pipeName );
                //client.Connect();
                client.Write( "Test", true );
                testServer.WaitForClients();
                Assert.IsTrue( 0 < testServer.RecievedFromClient );

                client.Write( "Test1", true );
                testServer.WaitForClients();
                Assert.IsTrue( 1 < testServer.RecievedFromClient );

                client.Write( "Test2", true );
                testServer.WaitForClients();
                Assert.IsTrue( 1 < testServer.RecievedFromClient );

                client.Write( "Test3", true );
                testServer.WaitForClients();
                Assert.IsTrue( 1 < testServer.RecievedFromClient );

                // TODO: Actually wait until server has nothing else to read.
                //Thread.Sleep( 2000 );


            }
        }

        [TestMethod]
        public void TestSimplePipeClientServerChat() {

            string pipeName = "SimplePipeTestPipeCSC";
            int clientReadCount = 0;

            using( ServerTest testServer = new ServerTest( pipeName ) ) {

                SimplePipeClient client = new SimplePipeClient( pipeName );
                client.OnReadCallback = delegate( string messageIn ){
                    if( messageIn.StartsWith( "Test" ) ) {
                        clientReadCount += 1;
                    }
                };
                client.Listen();
                
                client.Write( "Test", true );
                testServer.WaitForClients();
                Assert.IsTrue( 0 < testServer.RecievedFromClient );

                testServer.Write( "Test" );
                client.WaitForPeers();
                Assert.IsTrue( 0 < clientReadCount );

                client.Write( "Test1", true );
                testServer.WaitForClients();
                Assert.IsTrue( 1 < testServer.RecievedFromClient );

                testServer.Write( "TestA" );
                client.WaitForPeers();
                Assert.IsTrue( 1 < clientReadCount );

                client.Write( "Test2", true );
                testServer.WaitForClients();
                Assert.IsTrue( 2 < testServer.RecievedFromClient );

                testServer.Write( "TestB" );
                client.WaitForPeers();
                Assert.IsTrue( 2 < clientReadCount );

                client.Write( "Test3", true );
                testServer.WaitForClients();
                Assert.IsTrue( 3 < testServer.RecievedFromClient );

                testServer.Write( "TestC" );
                client.WaitForPeers();
                Assert.IsTrue( 3 < clientReadCount );
            }
        }

        [TestMethod]
        public void TestSimplePipeServerToClient() {

            string pipeName = "SimplePipeTestPipeSTC";
            bool clientRead = false;

            using( ServerTest testServer = new ServerTest( pipeName ) ) {

                SimplePipeClient client = new SimplePipeClient( pipeName );
                client.OnReadCallback += delegate( string messageIn ) { clientRead = true; };
                client.Listen();
                testServer.Write( "Test" );

                // TODO: Actually wait until server has nothing else to read.
                //Thread.Sleep( 2000 );

                testServer.WaitForClients();
                client.WaitForPeers();

                Assert.IsTrue( clientRead );
            }
        }
    }
}
