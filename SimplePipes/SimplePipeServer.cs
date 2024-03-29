﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace SimpleUtils {
    /// <summary>
    /// This server will wait for clients to connect and then read a code from
    /// the connected pipe. This code will then be opened as a pipe to write
    /// back to the client.
    /// </summary>
    public class SimplePipeServer : SimplePipePeer {
        public SimplePipeServer( string pipeNameIn ) : base( pipeNameIn ) {
            this.client = false;
        }

        public SimplePipeServer( string pipeNameIn, SimplePipeAccessLevel accessLevelIn ) : base( pipeNameIn ) {
            this.client = false;
            this.accessLevel = accessLevelIn;
        }
    }
}
