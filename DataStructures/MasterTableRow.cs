using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public class MasterTableRow
    {
        private const string BRANCH_UNIT_CUSTOMV_D_DB_NAME = "Branch-Unit_CustomV";
        private const string BRANCH_UNIT_CUSTOMS_D_DB_NAME = "Branch-Unit_CustomS_D";

        #region RowProperties
        public string AccessUniqueID { get; set; }
        public string SequenceID { get; set; }
        public string PrimaryKey { get; set; }
        public string CemeteryName { get; set; }
        public string BurialSectionNumber { get; set; }
        public string Wall { get; set; }
        public string RowNumber { get; set; }
        public string GravesiteNumber { get; set; }
        public string MarkerType { get; set; }
        public Nullable<int> Emblem1 { get; set; }
        public Nullable<int> Emblem2 { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Location { get; set; }
        public string Rank { get; set; }
        public string Rank2 { get; set; }
        public string Rank3 { get; set; }
        public string Award { get; set; }
        public string Award2 { get; set; }
        public string Award3 { get; set; }
        public string Award4 { get; set; }
        public string Award5 { get; set; }
        public string Award6 { get; set; }
        public string Award7 { get; set; }
        public string Awards_Custom { get; set; }
        public string War { get; set; }
        public string War2 { get; set; }
        public string War3 { get; set; }
        public string War4 { get; set; }
        public string Branch { get; set; }
        public string Branch2 { get; set; }
        public string Branch3 { get; set; }
        // Branch_Unit_CustomV is actually Branch-Unit_CustomV in database
        public string Branch_Unit_CustomV { get; set; }
        public string BirthDate { get; set; }
        public string DeathDate { get; set; }
        public string Inscription { get; set; }
        public string FirstNameS_D { get; set; }
        public string MiddleNameS_D { get; set; }
        public string LastNameS_D { get; set; }
        public string SuffixS_D { get; set; }
        public string LocationS_D { get; set; }
        public string RankS_D { get; set; }
        public string Rank2S_D { get; set; }
        public string Rank3S_D { get; set; }
        public string AwardS_D { get; set; }
        public string Award2S_D { get; set; }
        public string Award3S_D { get; set; }
        public string Award4S_D { get; set; }
        public string Award5S_D { get; set; }
        public string Award6S_D { get; set; }
        public string Award7S_D { get; set; }
        public string Awards_CustomS_D { get; set; }
        public string WarS_D { get; set; }
        public string War2S_D { get; set; }
        public string War3S_D { get; set; }
        public string War4S_D { get; set; }
        public string BranchS_D { get; set; }
        public string Branch2S_D { get; set; }
        public string Branch3S_D { get; set; }
        // Branch_Unit_CustomS_D is actually Branch-Unit_CustomS_D in database
        public string Branch_Unit_CustomS_D { get; set; }
        public string BirthDateS_D { get; set; }
        public string DeathDateS_D { get; set; }
        public string InscriptionS_D { get; set; }
        public string FirstNameS_D_2 { get; set; }
        public string MiddleNameS_D_2 { get; set; }
        public string LastNameS_D_2 { get; set; }
        public string SuffixS_D_2 { get; set; }
        public string LocationS_D_2 { get; set; }
        public string RankS_D_2 { get; set; }
        public string AwardS_D_2 { get; set; }
        public string WarS_D_2 { get; set; }
        public string BranchS_D_2 { get; set; }
        public string InscriptionS_D_2 { get; set; }
        public string BirthDateS_D_2 { get; set; }
        public string DeathDateS_D_2 { get; set; }
        public string FirstNameS_D_3 { get; set; }
        public string MiddleNameS_D_3 { get; set; }
        public string LastNameS_D_3 { get; set; }
        public string SuffixS_D_3 { get; set; }
        public string LocationS_D_3 { get; set; }
        public string RankS_D_3 { get; set; }
        public string AwardS_D_3 { get; set; }
        public string WarS_D_3 { get; set; }
        public string BranchS_D_3 { get; set; }
        public string InscriptionS_D_3 { get; set; }
        public string BirthDateS_D_3 { get; set; }
        public string DeathDateS_D_3 { get; set; }
        public string FirstNameS_D_4 { get; set; }
        public string MiddleNameS_D_4 { get; set; }
        public string LastNameS_D_4 { get; set; }
        public string SuffixS_D_4 { get; set; }
        public string LocationS_D_4 { get; set; }
        public string RankS_D_4 { get; set; }
        public string AwardS_D_4 { get; set; }
        public string WarS_D_4 { get; set; }
        public string BranchS_D_4 { get; set; }
        public string InscriptionS_D_4 { get; set; }
        public string BirthDateS_D_4 { get; set; }
        public string DeathDateS_D_4 { get; set; }
        public string FirstNameS_D_5 { get; set; }
        public string MiddleNameS_D_5 { get; set; }
        public string LastNameS_D_5 { get; set; }
        public string SuffixS_D_5 { get; set; }
        public string LocationS_D_5 { get; set; }
        public string BirthDateS_D_5 { get; set; }
        public string DeathDateS_D_5 { get; set; }
        public string FirstNameS_D_6 { get; set; }
        public string MiddleNameS_D_6 { get; set; }
        public string LastNameS_D_6 { get; set; }
        public string SuffixS_D_6 { get; set; }
        public string LocationS_D_6 { get; set; }
        public string BirthDateS_D_6 { get; set; }
        public string DeathDateS_D_6 { get; set; }
        public string BurialType { get; set; }
        public string BurialSize { get; set; }
        public string FeatureLocationDescription { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double LidarX { get; set; }
        public double LidarY { get; set; }
        public double LidarZ { get; set; }
        public double FrontImageX { get; set; }
        public double FrontImageY { get; set; }
        public string FrontDatetime { get; set; }
        public double BackImageX { get; set; }
        public double BackImageY { get; set; }
        public string BackDatetime { get; set; }
        public string QAInitials { get; set; }
        public string FrontFilename { get; set; }
        public string BackFilename { get; set; }
        public string SeqNum { get; set; }
        public string ImageHyperlink_Front { get; set; }
        public string ImageHyperlink_Back { get; set; }
        #endregion 

        public bool TrySetPropertyByName(string propertyName, object value)
        {
            if (propertyName == BRANCH_UNIT_CUSTOMS_D_DB_NAME)
            {
                if (!string.IsNullOrWhiteSpace(Branch_Unit_CustomV))
                {
                    Branch_Unit_CustomV = value as string;
                    return true;
                }

            }
            else if (propertyName == BRANCH_UNIT_CUSTOMV_D_DB_NAME)
            {
                if (!string.IsNullOrWhiteSpace(Branch_Unit_CustomS_D))
                {
                    Branch_Unit_CustomS_D = value as string;
                    return true;
                }
            }
            else
            {
                var prop = this.GetType().GetProperty(propertyName);
                var propType = prop.PropertyType;
                if (propType == typeof(string))
                {
                    string stringVal = prop.GetValue(this) as string;
                    if (!string.IsNullOrWhiteSpace(stringVal))
                    {
                        prop.SetValue(this, value);
                        return true;
                    }
                }
                else if (propType == typeof(int))
                {
                    Nullable<int> intVal = (Nullable<int>) prop.GetValue(this);
                    if (!intVal.HasValue)
                    {
                        prop.SetValue(this, value);
                        return true;
                    }
                }
                else if (propType == typeof(double))
                {
                    // We do not foresee ever having to set any of the double properties
                    return false;
                }
            }
            return false;
        }
    }
}
