using System;
using System.Collections.Generic;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataStructuresTests
{
    [TestClass]
    public class PersonTest
    {
        [TestMethod]
        public void  TestClearHeadstone()
        {
            Person primary = new Person();
            primary.FirstName = "P_First";
            primary.MiddleName = "P_Middle";
            primary.LastName = "P_Last";
            primary.Suffix = "P_Suffix";
            primary.Location = "P_Location";
            primary.BranchUnitCustom = "P_BranchUnitCustom";
            primary.BirthDate = "O5/02/2020";
            primary.DeathDate = "O5/02/2020";
            primary.Inscription = "P_Inscription";
            primary.AwardCustom = "P_AwardCustom";
            primary.AwardList = new List<string> { "P_A1", "P_A2", "P_A3", "P_A4", "P_A5", "P_A6", "P_A7" };
            primary.WarList = new List<string> { "P_W1", "P_W2", "P_W3", "P_W4" };
            primary.RankList = new List<string> { "P_R1", "P_R2", "P_R3" };
            primary.BranchList = new List<string> { "P_B1", "P_B2", "P_B3" };

            primary.clearPerson();

            Person clear = new Person();
            
            Assert.IsTrue(primary.Equals(clear));

        }

        [TestMethod]
        public void TestDatePerson()
        {
            Person primary = new Person();

            primary.DeathDate = "00/00/1898";
            Assert.IsTrue(primary.DeathDate == "00/00/1898");


            primary.BirthDate = "00/10/1898";
            Assert.IsTrue(primary.BirthDate == "00/10/1898");


            primary.BirthDate = "01/10/1898";
            Assert.IsTrue(primary.BirthDate == "01/10/1898");
        }
    }
}

