using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleUtils {
    [TestClass]
    public class SimpleConfigTests {
        [TestMethod]
        public void TestSimpleConfigUserSaveLoadRegistry() {

            string appKey = "SimpleConfigTests";
            string testKey1 = "Test";
            string testVal1 = "Bears";
            string testKey2 = "TestArray";
            string testVal2 = "Bears,Birds,Barns";
            string testVal2Sub2 = "Barns";

            SimpleConfig config = new SimpleConfig();
            config.Set( testKey1, testVal1 );
            config.Set( testKey2, testVal2 );
            config.SaveConfigRegistry( appKey, SimpleConfigRegistryNode.NODE_DEFAULT, SimpleConfigRegistryHive.HIVE_LOCAL_USER );
            Assert.AreEqual( config.Get( testKey1, "" ), testVal1 );
            Assert.AreEqual( config.GetList( testKey2, ',' )[2], testVal2Sub2 );

            SimpleConfig testConfig = SimpleConfig.LoadConfigRegistry( appKey, SimpleConfigRegistryNode.NODE_DEFAULT, SimpleConfigRegistryHive.HIVE_LOCAL_USER );
            Assert.AreEqual( testConfig.Get( testKey1, "" ), testVal1 );
            Assert.AreEqual( testConfig.GetList( testKey2, ',' )[2], testVal2Sub2 );
        }
    }
}
