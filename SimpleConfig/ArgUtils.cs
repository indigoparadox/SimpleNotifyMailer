﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleUtils {

    public class SimpleArg {
        public string ShortAction { get; set; }
        public string LongAction { get; set; }
        public string Help { get; set; }
        public Type ArgType { get; set; }
        public object ArgResult { get; set; }

        public SimpleArg(string shortActionIn, string longActionIn, string helpIn, Type typeIn ) {
            this.ShortAction = shortActionIn;
            this.LongAction = longActionIn;
            this.Help = helpIn;
            this.ArgType = typeIn;

            // Default booleans to false.
            if( typeof( Boolean ) == typeIn ) {
                this.ArgResult = false;
            } else {
                this.ArgResult = null;
            }
        }

        public SimpleArg( string shortActionIn, string longActionIn, Type typeIn ) : this( shortActionIn, longActionIn, null, typeIn ) {

        }
    }

    public class ArgUtils {
        public static void ParseArgs( string[] args, SimpleArg[] argOptions ) {

            int insideArg = -1;

            foreach( string argIter in args ) {
                if( 0 > insideArg ) {
                    int argIndex = ArgIndex( argIter, argOptions );
                    if( 0 > argIndex ) {
                        // TODO: Throw exception.
                    } else {
                        insideArg = ParseArgFollower( argOptions, argIndex ) ? argIndex : -1;
                    }
                } else {
                    ParseArgData( argOptions, insideArg, argIter );
                    insideArg = -1;
                }
            }
        }

        protected static void ParseArgData( SimpleArg[] argOptionsIn, int argIndexIn, string dataIn ) {
            if( typeof( String ) == argOptionsIn[argIndexIn].ArgType ) {
                argOptionsIn[argIndexIn].ArgResult = dataIn;
            } else if( typeof( DateTime ) == argOptionsIn[argIndexIn].ArgType ) {
                argOptionsIn[argIndexIn].ArgResult = DateTime.Parse( dataIn );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the argument data is the next arg to be passed, or false otherwise.</returns>
        protected static bool ParseArgFollower( SimpleArg[] argOptionsIn, int argIndexIn ) {
            if( typeof( Boolean ) == argOptionsIn[argIndexIn].ArgType ) {

                // Set the boolean value to true since we encountered this argument.
                argOptionsIn[argIndexIn].ArgResult = true;

                return false;
            } else {
                return true;
            }
        }

        public static int ArgIndex( string argIn, SimpleArg[] argOptionsIn ) {
            for( int i = 0 ; argOptionsIn.Length > i ; i++ ) {
                if(
                    argOptionsIn[i].ShortAction.ToLower().Equals( argIn.ToLower() ) ||
                    argOptionsIn[i].LongAction.ToLower().Equals( argIn.ToLower() )
                ) {
                    return i;
                }
            }
            return -1;
        }

        public static SimpleArg ArgLong( string argLongIn, SimpleArg[] argOptionsIn ) {
            foreach( SimpleArg argIter in argOptionsIn ) {
                if( argIter.LongAction.ToLower().Equals( argLongIn.ToLower() ) ) {
                    return argIter;
                }
            }
            return null;
        }
    }
}
