#if USE_REGISTRY
using Microsoft.Win32;
#endif // USE_REGISTRY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleUtils
{
    public class SimpleConfig {
        private Dictionary<string, string> internalConfig;

        public SimpleConfig() {
            this.internalConfig = new Dictionary<string, string>();
        }

        public void Set( string keyIn, string valueIn ) {
            this.internalConfig[keyIn] = valueIn;
        }

        public string Get( string keyIn, string defaultIn ) {
            if( this.internalConfig.ContainsKey( keyIn ) ) {
                return this.internalConfig[keyIn];
            } else {
                return defaultIn;
            }
        }

#if USE_REGISTRY
        public void SaveConfigRegistry( string appKeyIn ) {
        }

        public static SimpleConfig LoadConfigRegistry( string appKeyIn ) {
            SimpleConfig optionsOut = new SimpleConfig();

            using( RegistryKey loadKey = Registry.LocalMachine.OpenSubKey( "Software\\" + appKeyIn ) ) {
                foreach( string valueName in loadKey.GetValueNames() ) {
                    optionsOut.Set(
                        valueName,
                        (string)Registry.GetValue(
                            "HKEY_LOCAL_MACHINE\\Software\\" + appKeyIn,
                            valueName,
                            ""
                        )
                    );
                    if( null == optionsOut.Get( valueName, null ) ) {
                        optionsOut.Set(valueName, "" );
                    }
                }
            }

            return optionsOut;
        }
#endif // USE_REGISTRY
    }
}
