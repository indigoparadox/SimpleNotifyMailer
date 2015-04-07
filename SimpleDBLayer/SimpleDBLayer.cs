using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif // DEBUG
using System.IO;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;

using SQLiteConnection = Community.CsharpSqlite.SQLiteClient.SqliteConnection;
using SQLiteException = Community.CsharpSqlite.SQLiteClient.SqliteException;
using SQLiteCommand = Community.CsharpSqlite.SQLiteClient.SqliteCommand;
using SQLiteDataReader = Community.CsharpSqlite.SQLiteClient.SqliteDataReader;

namespace SimpleUtils {
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
#endif // USE_SQLITE

        public SimpleDBLayer( string dbPath ) {

#if USE_SQLITE
            string connectionString = String.Format( "Data Source={0}; Version=3; Journal Mode=WAL;", dbPath );

#if !USE_SQLITE_MANAGED
            try {
                if( !File.Exists( dbPath ) ) {
                    SQLiteConnection.CreateFile( dbPath );
                }
            } catch( IOException ex ) {
                throw new SimpleDBLayerException( ex.Message );
            }
#endif // !USE_SQLITE_MANAGED

            this.database = new SQLiteConnection( connectionString );
            this.database.Open();
#endif // USE_SQLITE_MANAGED, USE_SQLITE
        }

        public void Dispose() {
#if USE_SQLITE
            this.database.Close();
#endif
        }

        public void EnsureTablesAndIndexes( SimpleDBLayerTable[] tablesIn, SimpleDBLayerIndex[] indexesIn ) {

            // TODO: Check version.

#if USE_SQLITE
            try {
                foreach( SimpleDBLayerTable table in tablesIn ) {
                    StringBuilder createQueryString = new StringBuilder();

                    createQueryString.Append( "CREATE TABLE IF NOT EXISTS " + table.TableName + " (" );

                    List<string> uniqueColumns = new List<string>();
                    Dictionary<string, SimpleDBLayerTableColumn> foreignKeys = new Dictionary<string, SimpleDBLayerTableColumn>();

                    foreach( SimpleDBLayerColumn column in table.TableColumns ) {
                        createQueryString.Append( String.Format(
                            "{0} {1} {2} {3}, ",
                            column.ColumnName,
                            SimpleDBLayerColumn.GetTypeString( column.ColumnType ),
                            column.ColumnPrimaryKey ? "PRIMARY KEY" : "",
                            column.ColumnNotNull ? "NOT NULL" : "",
                            column.ColumnAutoIncrement ? "AUTOINCREMENT" : ""
                        ) );

                        // Save additional column information for the end of the statement.

                        if( column.ColumnUnique ) {
                            uniqueColumns.Add( column.ColumnName );
                        }

                        if( null != column.ColumnForeignKey ) {
                            foreignKeys.Add( column.ColumnName, column.ColumnForeignKey );
                        }
                    }

                    // Apply unique columns and foreign keys using informaton saved above.
                    
                    foreach( string column in uniqueColumns ) {
                        createQueryString.Append( String.Format( "UNIQUE({0}), ", column ) );
                    }

                    foreach( string foreignKey in foreignKeys.Keys ) {
                        createQueryString.Append( String.Format(
                            "FOREIGN KEY({0}) REFERENCES {1}({2}), ",
                            foreignKey,
                            foreignKeys[foreignKey].TableName,
                            foreignKeys[foreignKey].ColumnName
                        ) );
                    }

                    // Strip off last comma.
                    createQueryString.Remove( createQueryString.Length - 2, 2 );

                    createQueryString.Append( ")" );

#if DEBUG
                    Debug.WriteLine( createQueryString.ToString() );
#endif // DEBUG

                    using( SQLiteCommand createCmd = new SQLiteCommand( createQueryString.ToString(), this.database ) ) {
                        createCmd.ExecuteNonQuery();
                    }
                }

                foreach( SimpleDBLayerIndex index in indexesIn ) {
                    using( SQLiteCommand createCmd = new SQLiteCommand(
                        String.Format(
                            "CREATE INDEX IF NOT EXISTS {0} ON {1}({2})",
                            index.IndexName,
                            index.TableName,
                            index.ColumnName
                        ),
                        this.database
                    ) ) {
                        createCmd.ExecuteNonQuery();
                    }
                }
            } catch( SQLiteException ex ) {
                throw new SimpleDBLayerException( ex.Message );
            }
#endif // USE_SQLITE
        }

        private static string TransformProxyToken( DBCondition condition ) {
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
#if USE_SQLITE_MANAGED
            //using( Sqlite3 
#elif USE_SQLITE
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
#if USE_SQLITE_MANAGED
                    trafficReadCommand.Parameters.Add(
                        TransformProxyToken( condition ),
                        condition.TestValue
                    );
#else
                    trafficReadCommand.Parameters.AddWithValue(
                        TransformProxyToken( condition ),
                        condition.TestValue
                    );
#endif // USE_SQLITE_MANAGED
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
#if USE_SQLITE_MANAGED
                        insertCmd.Parameters.Add(
                            String.Format( "@{0}_param", columnKey ), columnData[columnKey]
                        );
#else
                        insertCmd.Parameters.AddWithValue(
                            String.Format( "@{0}_param", columnKey ), columnData[columnKey]
                        );
#endif // USE_SQLITE_MANAGED
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
