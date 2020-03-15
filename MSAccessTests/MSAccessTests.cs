using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataStructures;
using Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MSAccessTests
{
    [TestClass]
    public class MSAccessTests
    {
        private static string exePath = Directory.GetCurrentDirectory() + "\\..\\..\\..\\";
        private string sectionPath = Path.Combine(exePath, "TestDatabases\\Section0000P_UprightMakerTypes");
        private Headstone testHeadstone = new Headstone();

        public MSAccessTests()
        {
            testHeadstone.CemeteryName = "FT. RICHARDSON NATIONAL CEMETERY";
            testHeadstone.BurialSectionNumber = "BurialSectionNumber";
            testHeadstone.WallID = "2";
            testHeadstone.RowNum = "RowNum";
            testHeadstone.GavestoneNumber = "GravestoneNumber";
            testHeadstone.MarkerType = "Markertype";
            testHeadstone.Emblem1 = "2";
            testHeadstone.Emblem2 = "1";

            Person primary = new Person();
            primary.FirstName = "P_First";
            primary.MiddleName = "P_Middle";
            primary.LastName = "P_Last";
            primary.Suffix = "P_Suffix";
            primary.Location = "P_Location";
            primary.BranchUnitCustom = "P_BranchUnitCustom";
            primary.BirthDate = "P_BirthDate";
            primary.DeathDate = "P_DeathDate";
            primary.Inscription = "P_Inscription";
            primary.AwardCustom = "P_AwardCustom";
            primary.AwardList = new List<string> { "P_A1", "P_A2", "P_A3", "P_A4", "P_A5", "P_A6", "P_A7" };
            primary.WarList = new List<string> { "P_W1", "P_W2", "P_W3", "P_W4" };
            primary.RankList = new List<string> { "P_R1", "P_R2", "P_R3" };
            primary.BranchList = new List<string> { "P_B1", "P_B2", "P_B3" };
            testHeadstone.PrimaryDecedent = primary;

            testHeadstone.OthersDecedentList = new List<Person>();

            Person secondary = new Person();
            secondary.FirstName = "S_First";
            secondary.MiddleName = "S_Middle";
            secondary.LastName = "S_Last";
            secondary.Suffix = "S_Suffix";
            secondary.Location = "S_Location";
            secondary.BranchUnitCustom = "S_BranchUnitCustom";
            secondary.BirthDate = "S_BirthDate";
            secondary.DeathDate = "S_DeathDate";
            secondary.Inscription = "S_Inscription";
            secondary.AwardCustom = "S_AwardCustom";
            secondary.AwardList = new List<string> { "S_A1", "S_A2", "S_A3", "S_A4", "S_A5", "S_A6", "S_A7" };
            secondary.WarList = new List<string> { "S_W1", "S_W2", "S_W3", "S_W4" };
            secondary.RankList = new List<string> { "S_R1", "S_R2", "S_R3" };
            secondary.BranchList = new List<string> { "S_B1", "S_B2", "S_B3" };
            testHeadstone.OthersDecedentList.Add(secondary);

            Person other1 = new Person();
            other1.FirstName = "O1_First";
            other1.MiddleName = "O1_Middle";
            other1.LastName = "O1_Last";
            other1.Suffix = "O1_Suffix";
            other1.Location = "O1_Location";
            other1.BirthDate = "O1_BirthDate";
            other1.DeathDate = "O1_DeathDate";
            other1.Inscription = "O1_Inscription";
            other1.AwardList = new List<string> { "O1_A1" };
            other1.WarList = new List<string> { "O1_W1" };
            other1.RankList = new List<string> { "O1_R1" };
            other1.BranchList = new List<string> { "O1_B1" };
            testHeadstone.OthersDecedentList.Add(other1);

            Person other2 = new Person();
            other2.FirstName = "O2_First";
            other2.MiddleName = "O2_Middle";
            other2.LastName = "O2_Last";
            other2.Suffix = "O2_Suffix";
            other2.Location = "O2_Location";
            other2.BirthDate = "O2_BirthDate";
            other2.DeathDate = "O2_DeathDate";
            other2.Inscription = "O2_Inscription";
            other2.AwardList = new List<string> { "O2_A2" };
            other2.WarList = new List<string> { "O2_W2" };
            other2.RankList = new List<string> { "O2_R2" };
            other2.BranchList = new List<string> { "O2_B2" };
            testHeadstone.OthersDecedentList.Add(other2);

            Person other3 = new Person();
            other3.FirstName = "O3_First";
            other3.MiddleName = "O3_Middle";
            other3.LastName = "O3_Last";
            other3.Suffix = "O3_Suffix";
            other3.Location = "O3_Location";
            other3.BirthDate = "O3_BirthDate";
            other3.DeathDate = "O3_DeathDate";
            other3.Inscription = "O3_Inscription";
            other3.AwardList = new List<string> { "O3_A3" };
            other3.WarList = new List<string> { "O3_W3" };
            other3.RankList = new List<string> { "O3_R3" };
            other3.BranchList = new List<string> { "O3_B3" };
            testHeadstone.OthersDecedentList.Add(other3);

            Person other4 = new Person();
            other4.FirstName = "O4_First";
            other4.MiddleName = "O4_Middle";
            other4.LastName = "O4_Last";
            other4.Suffix = "O4_Suffix";
            other4.Location = "O4_Location";
            other4.BirthDate = "O4_BirthDate";
            other4.DeathDate = "O4_DeathDate";
            testHeadstone.OthersDecedentList.Add(other4);

            Person other5 = new Person();
            other5.FirstName = "O5_First";
            other5.MiddleName = "O5_Middle";
            other5.LastName = "O5_Last";
            other5.Suffix = "O5_Suffix";
            other5.Location = "O5_Location";
            other5.BirthDate = "O5_BirthDate";
            other5.DeathDate = "O5_DeathDate";
            testHeadstone.OthersDecedentList.Add(other5);
        }

        [TestMethod]
        public void CanAccessDB()
        {
            MicrosoftAccess databaseService = new MicrosoftAccess();
            bool success = databaseService.InitDBConnection(sectionPath);
            Assert.IsTrue(success == true);
        }

        [TestMethod]
        public void CanCountDatabases()
        {
            Regex reg = new Regex(@".*_be.accdb");

            var Dbfiles = Directory.GetFiles(sectionPath)
                .Where(path => reg.IsMatch(path))
                .ToList();

            int countDatabase = Dbfiles.Count;
            Assert.IsTrue(countDatabase == 1);
        }

        [TestMethod]
        public void CanQuerryTables()
        {
            MicrosoftAccess databaseService = new MicrosoftAccess();
            databaseService.InitDBConnection(sectionPath);

            List<CemeteryNameData> Cemeterynames = databaseService.CemeteryNames;
            Assert.IsTrue(Cemeterynames.Count == 85);

            List<AwardData> Awardnames = databaseService.AwardNames;
            Assert.IsTrue(Awardnames.Count == 64);

            List<BranchData> Branchnames = databaseService.BranchNames;
            Assert.IsTrue(Branchnames.Count == 80);
            
            List<LocationData> Locationnames = databaseService.LocationNames;
            Assert.IsTrue(Locationnames.Count == 237);
        }

        [TestMethod]
        public void TotalRecords()
        {
            MicrosoftAccess databaseService = new MicrosoftAccess();
            databaseService.InitDBConnection(sectionPath);

            int count = databaseService.TotalItems;
            Assert.IsTrue(count == 35);
        }

        [TestMethod]
        public void CanQueryHeadStone()
        {
            MicrosoftAccess databaseService = new MicrosoftAccess();
            databaseService.InitDBConnection(sectionPath);

            Headstone headstone = databaseService.GetHeadstone(1);
            databaseService.SetHeadstone(1, testHeadstone);

            Headstone newHeadstone = databaseService.GetHeadstone(1);

            //Reset to original
            databaseService.SetHeadstone(1, headstone);

            Assert.IsTrue(newHeadstone.PrimaryKey == "910-BurialSectionNumber-RowNum-GravestoneNumber");
            Assert.IsTrue(matchHeaderInfo(newHeadstone, testHeadstone));
            Assert.IsTrue(newHeadstone.PrimaryDecedent.Equals(testHeadstone.PrimaryDecedent));
            Assert.IsTrue(newHeadstone.OthersDecedentList[0].Equals(testHeadstone.OthersDecedentList[0]));
            Assert.IsTrue(newHeadstone.OthersDecedentList[1].Equals(testHeadstone.OthersDecedentList[1]));
            Assert.IsTrue(newHeadstone.OthersDecedentList[2].Equals(testHeadstone.OthersDecedentList[2]));
            Assert.IsTrue(newHeadstone.OthersDecedentList[3].Equals(testHeadstone.OthersDecedentList[3]));
            Assert.IsTrue(newHeadstone.OthersDecedentList[4].Equals(testHeadstone.OthersDecedentList[4]));
            Assert.IsTrue(newHeadstone.OthersDecedentList[5].Equals(testHeadstone.OthersDecedentList[5]));
        }

        public bool matchHeaderInfo(Headstone newHeadstone, Headstone testHeadstone)
        {
            Assert.IsTrue(newHeadstone.CemeteryName == testHeadstone.CemeteryName);
            Assert.IsTrue(newHeadstone.BurialSectionNumber == testHeadstone.BurialSectionNumber);
            Assert.IsTrue(newHeadstone.WallID == testHeadstone.WallID);
            Assert.IsTrue(newHeadstone.GavestoneNumber == testHeadstone.GavestoneNumber);
            Assert.IsTrue(newHeadstone.RowNum == testHeadstone.RowNum);
            Assert.IsTrue(newHeadstone.MarkerType == testHeadstone.MarkerType);
            Assert.IsTrue(newHeadstone.Emblem1 == testHeadstone.Emblem1);
            Assert.IsTrue(newHeadstone.Emblem2 == testHeadstone.Emblem2);
            return true;
        }
    }
}
