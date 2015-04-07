using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace SimpleUtils {

    public class SimpleDBLayerException : Exception {
        public SimpleDBLayerException( string message ) : base( message ) { }
    }

    public class SimpleDBLayer : IDisposable {

        public class DBJoinTable {
            public string TableName { get; set; }
            public char TableChar { get; set; }
            public string MainKey { get; set; }
            public string ForeignKey { get; set; }

            public DBJoinTable( string tableNameIn, char tableCharIn, string mainKeyIn, string foreignKeyIn ) {
                this.TableName = tableNameIn;
                this.TableChar = tableCharIn;
                this.MainKey = mainKeyIn;
                this.ForeignKey = foreignKeyIn;
            }
        }

        public class DBCondition {
            public char TableChar { get; set; }
            public string ColumnKey { get; set; }
            public string TestValue { get; set; }
            public DBComparator Comparator { get; set; }

            public DBCondition( char tableCharIn, string columnKeyIn, string testValueIn, DBComparator comparatorIn ) {
                this.TableChar = tableCharIn;
                this.ColumnKey = columnKeyIn;
                this.TestValue = testValueIn;
                this.Comparator = comparatorIn;
            }
        }

        public class DBColumn {
            public char TableChar { get; set; }
            public string Name { get; set; }
            public DBDataType Type { get; set; }

            public DBColumn( char tableCharIn, string nameIn, DBDataType typeIn ) {
                this.TableChar = tableCharIn;
                this.Name = nameIn;
                this.Type = typeIn;
            }
        }

        public class DBRow {
            //private Dictionary<string, object> RowData = new Dictionary<string, object>();

            //public Dictionary<string, object> Data { get { return this.RowData; } }

            private Dictionary<string, object> rowData = new Dictionary<string, object>();

            public void Set( string index, object value ) {
                this.rowData[index] = value;
            }

            public int GetInt( string index ) {
                return (int)(this.rowData[index]);
            }

            public double GetDouble( string index ) {
                return (double)(this.rowData[index]);
            }

            public string GetString( string index ) {
                return (string)(this.rowData[index]);
            }
        }

        public enum DBComparator {
            GREATER_THAN,
            LESS_THAN,
            EQUAL_TO
        }

        public enum DBDataType {
            STRING,
            DOUBLE,
            INT,
        }

#if USE_SQLITE
        private SQLiteConnection database;
#endif

        public SimpleDBLayer( string dbPath ) {
#if USE_SQLITE

            try {
                if( !File.Exists( dbPath ) ) {
                    SQLiteConnection.CreateFile( dbPath );
                }
            } catch( IOException ex ) {
                throw new SimpleDBLayerException( ex.Message );
            }

            this.database = new SQLiteConnection( String.Format( "Data Source={0}; Version=3; Journal Mode=WAL;", dbPath ) );
            this.database.Open();
#endif
        }

        public void Dispose() {
#if USE_SQLITE
            this.database.Close();
#endif
        }

        public void EnsureTablesAndIndexes() {

            // TODO: Check version.
            
#if USE_SQLITE
            try {
                SQLiteCommand createCmd;
                using( createCmd = new SQLiteCommand(
                    "CREATE TABLE IF NOT EXISTS usernames ( " +
                        "key INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "name VARCHAR(255), " +
                        "UNIQUE(name)" +
                    ")",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE TABLE IF NOT EXISTS hostnames ( " +
                        "key INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "name VARCHAR(255), " +
                        "UNIQUE(name)" +
                    ")",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE TABLE IF NOT EXISTS traffic ( " +
                        "key INTEGER PRIMARY KEY AUTOINCREMENT," +
                        "time DOUBLE NOT NULL, " +
                        "src_ip UNSIGNED BIG INT NOT NULL, " +
                        "src_name INTEGER, " +
                        "user_name INTEGER, " +
                        "dest_ip UNSIGNED BIG INT NOT NULL, " +
                        "dest_name INTEGER, " +
                        "sent_bytes INTEGER NOT NULL, " +
                        "rcvd_bytes INTEGER NOT NULL, " +
                        "FOREIGN KEY(src_name) REFERENCES hostnames(key), " +
                        "FOREIGN KEY(user_name) REFERENCES usernames(key)" +
                        "FOREIGN KEY(dest_name) REFERENCES hostnames(key)" +
                    ")",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE TABLE IF NOT EXISTS system (version)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }

                using( createCmd = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS key_idx ON hostnames(key)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS name_idx ON hostnames(name)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS time_idx ON traffic(time)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS src_ip_idx ON traffic(src_ip)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS dest_ip_idx ON traffic(dest_ip)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS username_idx ON usernames(name)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
                using( createCmd = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS username_traffic_idx ON traffic(user_name)",
                    this.database
                ) ) {
                    createCmd.ExecuteNonQuery();
                }
            } catch( SQLiteException ex ) {
                throw new SimpleDBLayerException( ex.Message );
            }
#endif
        }

        private static string TransformProxyToken( DBCondition condition ){
            string tentative = "@" + condition.TableChar + condition.ColumnKey;
            tentative = tentative.Replace( "_", "" );
            switch( condition.Comparator ) {
                case DBComparator.EQUAL_TO:
                    tentative += "EQ";
                    break;
                case DBComparator.GREATER_THAN:
                    tentative += "GT";
                    break;
                case DBComparator.LESS_THAN:
                    tentative += "LT";
                    break;
            }
            return tentative;
        }

        public object SelectObject( string column, string table, string keyColumn, string comparer ) {
            string selectString = String.Format(
                "SELECT {0} FROM {1} WHERE {2}=@comparerParam",
                column, table, keyColumn
            );

            object selectOut = null;
#if USE_SQLITE
            using( SQLiteCommand readCommand = new SQLiteCommand( selectString, this.database ) ) {
                readCommand.Parameters.AddWithValue( "@comparerParam", comparer );
                selectOut = readCommand.ExecuteScalar();
            }
#endif

            return selectOut;
        }

        public DBRow[] SelectRows(
            DBColumn[] columnSelections, Tuple<string, string> tableSelection, DBJoinTable[] tableJoins,
            DBCondition[] conditions
        ) {
            List<DBRow> rowsOut = new List<DBRow>();
            List<string> columnSelectionStrings = new List<string>();
            List<string> tableJoinStrings = new List<string>();
            List<string> conditionStrings = new List<string>();

#if USE_SQLITE
            foreach( DBColumn column in columnSelections ) {
                columnSelectionStrings.Add(
                    column.TableChar + "." + column.Name
                );
            }

            // Convert the join objects into DB-specific join strings.
            foreach( DBJoinTable join in tableJoins ) {
                tableJoinStrings.Add( String.Format(
                    "LEFT OUTER JOIN {0} AS {1} ON {2}.{3}={4}.{5} ",
                    join.TableName,
                    join.TableChar,
                    tableSelection.Item2,
                    join.MainKey,
                    join.TableChar,
                    join.ForeignKey
                ) );
            }

            // Translate the conditions for SQLite.
            foreach( DBCondition condition in conditions ) {
                string comparatorString = "=";
                switch( condition.Comparator ) {
                    case DBComparator.GREATER_THAN:
                        comparatorString = ">";
                        break;

                    case DBComparator.LESS_THAN:
                        comparatorString = "<";
                        break;
                }

                conditionStrings.Add( String.Format(
                    "{0}.{1}{2}{3}",
                    condition.TableChar,
                    condition.ColumnKey,
                    comparatorString,
                    // TODO: Create a proxy token value.
                    //condition.TestValue
                    TransformProxyToken( condition )
                ) );
            }

            string selectQueryString = String.Format(
                "SELECT {0} FROM {1} AS {2} {3} WHERE {4} ORDER BY t.src_ip ASC",
                string.Join( ",", columnSelectionStrings ),
                tableSelection.Item1,
                tableSelection.Item2,
                string.Join( " ", tableJoinStrings ),
                string.Join( " AND ", conditionStrings )
            );

            using( SQLiteCommand trafficReadCommand = new SQLiteCommand(
                selectQueryString, database
            ) ) {
                /*
                trafficReadCommand.Parameters.AddWithValue( "@ipLowParameter", DBConversions.IP_LOCAL_LOW );
                trafficReadCommand.Parameters.AddWithValue( "@ipHighParameter", DBConversions.IP_LOCAL_HIGH );
                trafficReadCommand.Parameters.AddWithValue( "@timeStartParameter", this.startTime );
                trafficReadCommand.Parameters.AddWithValue( "@timeEndParameter", this.endTime );
                 */

                // Add the proxies.
                foreach( DBCondition condition in conditions ) {
                    trafficReadCommand.Parameters.AddWithValue(
                        TransformProxyToken( condition ),
                        condition.TestValue
                    );
                }

                using( SQLiteDataReader reader = trafficReadCommand.ExecuteReader() ) {
                    while( reader.Read() ) {
                        int columnIndexIter = 0;
                        DBRow rowIter = new DBRow();

                        foreach( DBColumn column in columnSelections ) {
                            string columnKey = column.TableChar + "." + column.Name;

                            switch( column.Type ) {
                                case DBDataType.STRING:
                                    rowIter.Set( columnKey, "" );
                                    if( !reader.IsDBNull( columnIndexIter ) ) {
                                        rowIter.Set( columnKey, reader.GetString( columnIndexIter ) );
                                    }
                                    break;

                                case DBDataType.INT:
                                    rowIter.Set( columnKey, reader.GetInt32( columnIndexIter ) );
                                    break;

                                case DBDataType.DOUBLE:
                                    rowIter.Set( columnKey, reader.GetDouble( columnIndexIter ) );
                                    break;
                            }

                            columnIndexIter++;
                        }

                        rowsOut.Add( rowIter );
                    }
                }
            }
#endif

            return rowsOut.ToArray();
        }

        public void InsertRow( string table, Dictionary<string, object> columnData ) {
            this.InsertRow( table, columnData, false );
        }

        public void InsertRow( string table, Dictionary<string, object> columnData, bool orIgnore ) {

            string insertString = String.Format(
                "INSERT{3}INTO {0} ({1}) VALUES ({2})",
                table,
                string.Join( ",", columnData.Keys ),
                string.Join( ",", columnData.Keys.Select( s => String.Format( "@{0}_param", s ) ).ToArray() ),
                orIgnore ? " OR IGNORE " : " "
            );
            
#if USE_SQLITE
            try {
                using( SQLiteCommand insertCmd = new SQLiteCommand( insertString, this.database ) ) {
                    foreach( string columnKey in columnData.Keys ) {
                        insertCmd.Parameters.AddWithValue(
                            String.Format( "@{0}_param", columnKey ), columnData[columnKey]
                        );
                    }
                    insertCmd.ExecuteNonQuery();
                }
            } catch( SQLiteException ex ) {
                throw new SimpleDBLayerException( ex.Message );
            }
#endif
        }
    }
}
