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

        }
    }
}
