#if USE_REGISTRY
using Microsoft.Win32;
#endif // USE_REGISTRY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if USE_WINFORMS
using System.Windows.Forms;
using System.Diagnostics;
#endif // USE_WINFORMS

namespace SimpleUtils {
#if USE_REGISTRY
    public enum SimpleConfigRegistryNode {
        NODE_32,
        NODE_64,
        NODE_DEFAULT
    }

    public enum SimpleConfigRegistryHive {
        HIVE_LOCAL_MACHINE,
        HIVE_LOCAL_USER
    }
#endif // USE_REGISTRY

    public class SimpleConfigException : Exception {
        public SimpleConfigException( string messageIn ) : base( messageIn ) { }
    }

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

        public string[] GetList( string keyIn, char separatorIn ) {
            string listString = this.Get( keyIn, "" );
            string[] listOut;
            if( string.IsNullOrWhiteSpace( listString ) ) {
                listOut = new string[] { };
            } else {
                listOut = listString.Split( separatorIn ).Select( i => i.Trim() ).ToArray();
            }
            return listOut;
        }

        public int GetInteger( string keyIn, int defaultIn ) {
            int intOut = 0;

            if( this.internalConfig.ContainsKey( keyIn ) && int.TryParse( this.internalConfig[keyIn], out intOut ) ) {
                return intOut;
            } else {
                return defaultIn;
            }
        }

#if USE_WINFORMS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textBoxIn"></param>
        /// <param name="listBoxIn"></param>
        /// <param name="listNameIn">The human-readable name of the list. Should fit in the sentence structure "The [listNameIn] list".</param>
        public static void AddTextToList( TextBox textBoxIn, ListBox listBoxIn, string listNameIn ) {
            
            // Just ignore empty input strings.
            if( string.IsNullOrWhiteSpace( textBoxIn.Text ) ) {
                return;
            }
            
            if( listBoxIn.Items.Contains( textBoxIn.Text ) ) {
                MessageBox.Show(
                    String.Format( "The {0} list already contains the item \"{1}\".", listNameIn, textBoxIn.Text ),
                    Application.ProductName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            } else {
                listBoxIn.Items.Add( textBoxIn.Text );
                textBoxIn.Text = "";
            }
        }

        public static void RemoveTextFromList( ListBox listBoxIn ) {
            if(
                0 <= listBoxIn.SelectedIndex &&
                listBoxIn.SelectedIndex < listBoxIn.Items.Count
            ) {
                listBoxIn.Items.Remove( (string)listBoxIn.Items[listBoxIn.SelectedIndex] );
            }
        }
#endif // USE_WINFORMS

#if USE_REGISTRY
        public void SaveConfigRegistry( string appKeyIn ) {
            this.SaveConfigRegistry( appKeyIn, SimpleConfigRegistryNode.NODE_DEFAULT );
        }

        public void SaveConfigRegistry( string appKeyIn, SimpleConfigRegistryNode regNodeIn ) {
            this.SaveConfigRegistry( appKeyIn, regNodeIn, SimpleConfigRegistryHive.HIVE_LOCAL_MACHINE );
        }
            
        public void SaveConfigRegistry( string appKeyIn, SimpleConfigRegistryNode regNodeIn, SimpleConfigRegistryHive hiveIn ) {

            RegistryView regView = RegistryView.Default;
            switch(regNodeIn){
                case SimpleConfigRegistryNode.NODE_32:
                    regView = RegistryView.Registry32;
                    break;
                case SimpleConfigRegistryNode.NODE_64:
                    regView = RegistryView.Registry64;
                    break;
            }

            RegistryHive regHive = RegistryHive.LocalMachine;
            if( SimpleConfigRegistryHive.HIVE_LOCAL_USER == hiveIn ) {
                regHive = RegistryHive.CurrentUser;
            }

            try {
                using( RegistryKey baseKey = RegistryKey.OpenBaseKey( regHive, regView ) ) {

                    // Make sure the key exists before writing to it.
                    using( RegistryKey loadKey = baseKey.OpenSubKey( "Software\\" + appKeyIn ) ) {
                        if( null == loadKey ) {
                            baseKey.CreateSubKey( "Software\\" + appKeyIn );
                        }
                    }

                    // Write out the config as subkeys.
                    using( RegistryKey loadKey = baseKey.OpenSubKey( "Software\\" + appKeyIn, true ) ) {
                        foreach( string keyIter in this.internalConfig.Keys ) {
                            loadKey.SetValue(
                                keyIter,
                                this.internalConfig[keyIter],
                                RegistryValueKind.String
                            );
                        }
                    }
                }
            } catch( UnauthorizedAccessException ex ) {
                // Wrap and throw upwards to isolate usings.
                throw new SimpleConfigException( ex.Message );
            }
        }

        public static SimpleConfig LoadConfigRegistry( string appKeyIn ) {
            return SimpleConfig.LoadConfigRegistry( appKeyIn, SimpleConfigRegistryNode.NODE_DEFAULT );
        }
        public static SimpleConfig LoadConfigRegistry( string appKeyIn, SimpleConfigRegistryNode regNodeIn ) {
            return SimpleConfig.LoadConfigRegistry( appKeyIn, regNodeIn, SimpleConfigRegistryHive.HIVE_LOCAL_MACHINE );
        }

        public static SimpleConfig LoadConfigRegistry( string appKeyIn, SimpleConfigRegistryNode regNodeIn, SimpleConfigRegistryHive hiveIn ) {
            SimpleConfig optionsOut = new SimpleConfig();

            RegistryView regView = RegistryView.Default;
            switch( regNodeIn ) {
                case SimpleConfigRegistryNode.NODE_32:
                    regView = RegistryView.Registry32;
                    break;
                case SimpleConfigRegistryNode.NODE_64:
                    regView = RegistryView.Registry64;
                    break;
            }

            RegistryHive regHive = RegistryHive.LocalMachine;
            if( SimpleConfigRegistryHive.HIVE_LOCAL_USER == hiveIn ) {
                regHive = RegistryHive.CurrentUser;
            }

            try {
                using( RegistryKey baseKey = RegistryKey.OpenBaseKey( regHive, regView ) ) {
                    using( RegistryKey loadKey = baseKey.OpenSubKey( "Software\\" + appKeyIn ) ) {
                        if( null == loadKey ) {
                            // The key doesn't exist, so we can't do anything, anyway.
                            return new SimpleConfig();
                        }

                        foreach( string valueName in loadKey.GetValueNames() ) {
                            optionsOut.Set(
                                valueName,
                                (string)loadKey.GetValue(
                                    valueName,
                                    ""
                                )
                            );
                            if( null == optionsOut.Get( valueName, null ) ) {
                                optionsOut.Set( valueName, "" );
                            }
                        }
                    }
                }
            } catch( UnauthorizedAccessException ex ) {
                // Wrap and throw upwards to isolate usings.
                throw new SimpleConfigException( ex.Message );
            }

            return optionsOut;
        }
#endif // USE_REGISTRY
    }
}
