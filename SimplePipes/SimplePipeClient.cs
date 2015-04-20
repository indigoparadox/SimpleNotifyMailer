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

        public SimplePipeClient( string pipeNameIn ) : base( pipeNameIn ) { 

#if false
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            this.clientID = new string(
                Enumerable.Repeat( chars, 8 )
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray()
            );
#endif

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
