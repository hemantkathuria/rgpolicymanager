using System;
using System.Collections.Generic;
using System.Text;

namespace rgpolicymanager.core.Entities
{
    public class InitiativeParameters
    {
        public InfyBusinessUnitValue infyBusinessUnitValue { get; set; }
        public InfyCostCenterValue infyCostCenterValue { get; set; }
        public InfyProjectCodeValue infyProjectCodeValue { get; set; }
        public InfyEAPurposeValue infyEAPurposeValue { get; set; }
        public InfyEAWorkloadTypeValue infyEAWorkloadTypeValue { get; set; }
        public InfyEACustomTag01Value infyEACustomTag01Value { get; set; }
        public InfyEACustomTag02Value infyEACustomTag02Value { get; set; }
        public InfyEACustomTag03Value infyEACustomTag03Value { get; set; }
        public VmNamePatternValueValue vmNamePatternValueValue { get; set; }
    }

    public class InfyBusinessUnitValue
    {
        public string value { get; set; }
    }

    public class InfyCostCenterValue
    {
        public string value { get; set; }
    }

    public class InfyProjectCodeValue
    {
        public string value { get; set; }
    }

    public class InfyEAPurposeValue
    {
        public string value { get; set; }
    }

    public class InfyEAWorkloadTypeValue
    {
        public string value { get; set; }
    }

    public class InfyEACustomTag01Value
    {
        public string value { get; set; }
    }

    public class InfyEACustomTag02Value
    {
        public string value { get; set; }
    }

    public class InfyEACustomTag03Value
    {
        public string value { get; set; }
    }

    public class VmNamePatternValueValue
    {
        public string value { get; set; }
    }

   
}
