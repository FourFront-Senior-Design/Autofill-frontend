using System;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataStructuresTests
{
    [TestClass]
    public class HeadstoneTest
    {
        [TestMethod]
        public void TestIsNumber()
        {
            Headstone head = new Headstone();
            Assert.IsTrue(head.isNumber("99") == true);
            Assert.IsTrue(head.isNumber("aa") == false);
            Assert.IsTrue(head.isNumber("0-") == false);
        }

        [TestMethod]
        public void TestMakeValid()
        {
            Headstone head = new Headstone();
            Assert.IsTrue(head.makeValid("\r\n") == "");
            Assert.IsTrue(head.makeValid("a\r") == "a");
            Assert.IsTrue(head.makeValid("\nab\r\nbb") == "abbb");
        }

        [TestMethod]
        public void TestEmblems()
        {
            Headstone head = new Headstone();

            head.Emblem1 = "\r0\n";
            Assert.IsTrue(head.Emblem1 == "0");

            head.Emblem2 = "\ra\n";
            Assert.IsTrue(head.Emblem2 == "");

            head.Emblem2 = "\r10\n";
            Assert.IsTrue(head.Emblem2 == "10");
        }
    }
}