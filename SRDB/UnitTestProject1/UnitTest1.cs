using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SRDB;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private SimpleRegistyDB testDb;

        public UnitTest1()
        {
            testDb = new SimpleRegistyDB(@"testDb");
        }

        [TestMethod]
        public void TestAddGroup()
        {
        }

        [TestMethod]
        public void TestAddVpnItem()
        {

        }



    }
}
