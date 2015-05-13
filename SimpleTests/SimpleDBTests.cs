using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleUtils;
using System.Collections.Generic;

namespace SimpleTests {
    [TestClass]
    public class SimpleDBTests {

        public static readonly string TEST_DB_PATH = ".\\testDB.s3db";

        [ClassInitialize()]
        public static void SimpleDBTestsInitialize( TestContext testContextIn ) {
            using( SimpleDBLayer db = new SimpleDBLayer( TEST_DB_PATH ) ) {

                SimpleDBLayerColumn[] columnsTest = new SimpleDBLayerColumn[] {
                    new SimpleDBLayerColumn("ID",SimpleDBLayerColumnType.INTEGER,true,true,true,true,null),
                    new SimpleDBLayerColumn("String",SimpleDBLayerColumnType.VAR_255,false,false,false,false,null),
                    new SimpleDBLayerColumn("BigNumber",SimpleDBLayerColumnType.DOUBLE,false,false,true,false,null),
                };

                SimpleDBLayerTable[] tablesTest = new SimpleDBLayerTable[] {
                    new SimpleDBLayerTable("TestTableA",columnsTest),
                };

                SimpleDBLayerIndex[] indexesTest = new SimpleDBLayerIndex[]{
                    new SimpleDBLayerIndex("test_index","TestTableA","ID"),
                };

                db.EnsureTablesAndIndexes( tablesTest, indexesTest );

                SimpleDBLayer.DBCondition[] deleteConditions = new SimpleDBLayer.DBCondition[] {
                    new SimpleDBLayer.DBCondition('\0',"String","TestKey",SimpleDBLayer.DBComparator.EQUAL_TO),
                };

                db.DeleteRows( "TestTableA", deleteConditions, false );
            }
        }

        [TestMethod]
        public void TestDBInsert() {
            using( SimpleDBLayer db = new SimpleDBLayer( TEST_DB_PATH ) ) {

                Random rand = new Random();
                int testRand = rand.Next();

                Dictionary<string, object> data = new Dictionary<string, object>();

                data.Add( "BigNumber", testRand.ToString() );
                data.Add( "String", "TestKey" );

                db.InsertRow( "TestTableA", data );

                int dbObject = Convert.ToInt32( (double)db.SelectObject( "BigNumber", "TestTableA", "String", "TestKey" ) );

                Assert.AreEqual( testRand, dbObject );
            }
        }
    }
}
