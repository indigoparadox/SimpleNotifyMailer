using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleUtils {

    public class SimpleDBLayerException : Exception {
        public SimpleDBLayerException( string message ) : base( message ) { }
    }

    public class SimpleDBLayerIndex {
        public string IndexName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }

        public SimpleDBLayerIndex( string indexNameIn, string tableNameIn, string columnNameIn ) {
            this.IndexName = indexNameIn;
            this.TableName = tableNameIn;
            this.ColumnName = columnNameIn;
        }
    }

    public enum SimpleDBLayerColumnType {
        INTEGER,
        VAR_255,
        DOUBLE,
        UNSIGNED_INTEGER,
    }

    public class SimpleDBLayerTableColumn {
        public string TableName { get; set; }
        public string ColumnName { get; set; }

        public SimpleDBLayerTableColumn( string tableIn, string columnIn ) {
            this.TableName = tableIn;
            this.ColumnName = columnIn;
        }
    }

    public class SimpleDBLayerColumn {
        public string ColumnName { get; set; }
        public SimpleDBLayerColumnType ColumnType { get; set; }
        public bool ColumnPrimaryKey { get; set; }
        public bool ColumnAutoIncrement { get; set; }
        public bool ColumnNotNull { get; set; }
        public bool ColumnUnique { get; set; }
        public SimpleDBLayerTableColumn ColumnForeignKey { get; set; }

        public SimpleDBLayerColumn( 
            string nameIn, SimpleDBLayerColumnType typeIn, bool primaryKeyIn, bool autoIncIn,
            bool notNullIn, bool uniqueIn, SimpleDBLayerTableColumn foreignKeyIn
        ) {
            this.ColumnName = nameIn;
            this.ColumnType = typeIn;
            this.ColumnPrimaryKey = primaryKeyIn;
            this.ColumnAutoIncrement = autoIncIn;
            this.ColumnNotNull = notNullIn;
            this.ColumnUnique = uniqueIn;
            this.ColumnForeignKey = foreignKeyIn;
        }

        public static string GetTypeString( SimpleDBLayerColumnType typeIn ) {
#if USE_SQLITE
            switch( typeIn ) {
                case SimpleDBLayerColumnType.DOUBLE:
                    return "DOUBLE";
                case SimpleDBLayerColumnType.INTEGER:
                    return "INTEGER";
                case SimpleDBLayerColumnType.UNSIGNED_INTEGER:
                    return "UNSIGNED BIG INT";
                case SimpleDBLayerColumnType.VAR_255:
                    return "VARCHAR(255)";
            }
#endif // USE_SQLITE

            return "";
        }
    }

    public class SimpleDBLayerTable {
        public string TableName { get; set; }
        public SimpleDBLayerColumn[] TableColumns { get; set; }

        public SimpleDBLayerTable( string nameIn, SimpleDBLayerColumn[] columnsIn ) {
            this.TableName = nameIn;
            this.TableColumns = columnsIn;
        }
    }

}
