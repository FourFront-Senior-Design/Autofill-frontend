using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesInterface
{
    public interface IDatabaseService
    {
        Headstone GetHeadstone(int index);

        void SetHeadstone(int index, Headstone headstone);

        bool InitDBConnection(string sectionFilePath);

        int TotalItems { get; }

        List<CemeteryNameData> CemeteryNames { get; }

        List<LocationData> LocationNames { get; }

        List<BranchData> BranchNames { get; }

        List<WarData> WarNames { get; }

        List<AwardData> AwardNames { get; }

        string SectionFilePath { get; }
    }
}
