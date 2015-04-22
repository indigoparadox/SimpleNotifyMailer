using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleUtils {
    public class TypeUtils {

        public static string FormatMB( long bytes ) {
            return String.Format( "{0:0.00}", bytes / 1024 / 1042 );
        }

        public static string[] SplitStringQuoted( string stringIn ) {
            List<string> stringArrayOut = new List<string>();
            bool insideQuotes = false;
            StringBuilder currentString = new StringBuilder();

            foreach( char c in stringIn ) {
                switch( c ) {
                    case '\n':
                    case '\r':
                    case '\t':
                    case ' ':
                        if( insideQuotes ) {
                            // Append the space to the string.
                            currentString.Append( c );
                        } else {
                            // Create a new string.
                            stringArrayOut.Add( currentString.ToString() );
                            currentString = new StringBuilder();
                        }
                        break;

                    case '"':
                        if( !insideQuotes ) {
                            // Start a new string.
                            insideQuotes = true;
                        } else {
                            // End the current string.
                            insideQuotes = false;
                        }
                        break;

                    default:
                        // Append to the current string if it's printable.
                        if( !Char.IsControl( c ) && 0x2028 != c && 0x2029 != c && 0 < c ) {
                            currentString.Append( c );
                        }
                        break;
                }
            }

            // Add the last string built.
            if( 0 < currentString.Length ) {
                stringArrayOut.Add( currentString.ToString() );
            }

            return stringArrayOut.ToArray();
        }
    }
}
