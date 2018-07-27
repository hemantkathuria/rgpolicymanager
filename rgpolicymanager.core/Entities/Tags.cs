using System;
using System.Collections.Generic;
using System.Text;


namespace rgpolicymanager.core.Entities
{
    /// <summary>
    /// Tags
    /// </summary>
    public class Tags
    {
        /// <summary>
        /// INFY_EA_BusinessUnit
        /// </summary>
        public string  INFY_EA_BusinessUnit { get; set;}

        /// <summary>
        /// INFY_EA_CostCenter
        /// </summary>
        public string INFY_EA_CostCenter { get; set; }

        /// <summary>
        /// INFY_EA_ProjectCode
        /// </summary>
        public string INFY_EA_ProjectCode { get; set; }

        /// <summary>
        /// INFY_EA_Purpose
        /// </summary>
        public string INFY_EA_Purpose { get; set; }

        /// <summary>
        /// INFY_EA_WorkLoadType
        /// </summary>
        public string INFY_EA_WorkLoadType { get; set; }

        /// <summary>
        /// INFY_EA_CustomTag01
        /// </summary>
        public string INFY_EA_CustomTag01 { get; set; }

        /// <summary>
        /// INFY_EA_CustomTag02
        /// </summary>
        public string INFY_EA_CustomTag02 { get; set; }

        /// <summary>
        /// INFY_EA_CustomTag03
        /// </summary>
        public string INFY_EA_CustomTag03 { get; set; }

        /// <summary>
        /// Get Tags
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string,string>> GetTags()
        {
            List<KeyValuePair<string, string>> tags = new List<KeyValuePair<string, string>>();

            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_BusinessUnit), INFY_EA_BusinessUnit));
            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_CostCenter), INFY_EA_CostCenter));
            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_ProjectCode), INFY_EA_ProjectCode));
            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_Purpose), INFY_EA_Purpose));
            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_WorkLoadType), INFY_EA_WorkLoadType));
            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_CustomTag01), INFY_EA_CustomTag01));
            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_CustomTag02), INFY_EA_CustomTag02));
            tags.Add(new KeyValuePair<string, string>(nameof(INFY_EA_CustomTag03), INFY_EA_CustomTag03));

            return tags;
        }
    }
}
