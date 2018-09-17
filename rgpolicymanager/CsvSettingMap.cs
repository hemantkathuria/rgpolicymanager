using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration;

namespace rgpolicymanager
{
    /// <summary>
    /// CsvSettingMap
    /// </summary>
    public class CsvSettingMap: CsvHelper.Configuration.ClassMap<CSVSetting>
    {
        public CsvSettingMap()
        {
            Map(m => m.Status).Name("Status");
            Map(m => m.VMNamePattern).Name("vmNamePattern");
            Map(m => m.ASNamePattern).Name("asNamePattern");
            Map(m => m.ResourceGroupName).Name("resourcegroupname");
            Map(m => m.ProjectCode).Name("projectcode");
            Map(m => m.ResourceGroupLocation).Name("resourcegrouplocation");
            Map(m => m.ADProjectGroupName).Name("adprojectgroupname");
            Map(m => m.ADPUserEmailAddress).Name("aduseremailaddress");
            Map(m => m.INFY_EA_BusinessUnit).Name("INFY_EA_BusinessUnit");
            Map(m => m.INFY_EA_CostCenter).Name("INFY_EA_CostCenter");
            Map(m => m.INFY_EA_ProjectCode).Name("INFY_EA_ProjectCode");
            Map(m => m.INFY_EA_Purpose).Name("INFY_EA_Purpose");
            Map(m => m.INFY_EA_WorkLoadType).Name("INFY_EA_WorkLoadType");
            Map(m => m.INFY_EA_CustomTag01).Name("INFY_EA_CustomTag01");
            Map(m => m.INFY_EA_CustomTag02).Name("INFY_EA_CustomTag02");
            Map(m => m.INFY_EA_CustomTag03).Name("INFY_EA_CustomTag03");
        }
    }
}
