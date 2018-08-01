using System;
using System.Collections.Generic;
using System.Text;

namespace rgpolicymanager.core.Entities
{
    /// <summary>
    /// APP Settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// TenantId
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Authority
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Client Id
        /// </summary>
        public string  Clientid { get; set; }

        /// <summary>
        /// Client Secret
        /// </summary>
        public string  Clientsecret { get; set; }

        /// <summary>
        /// Resource
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Subscription Id
        /// </summary>
        public string Subscriptionid { get; set; }

        /// <summary>
        /// Initiative Name
        /// </summary>
        public string Initiativename { get; set; }

        /// <summary>
        /// VMNamePattern
        /// </summary>
        public string VMNamePattern { get; set; }

        /// <summary>
        /// ASNamePattern
        /// </summary>
        public string ASNamePattern { get; set; }

        /// <summary>
        /// ResourceGroupName
        /// </summary>
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// ProjectCode
        /// </summary>
        public string ProjectCode { get; set; }

        /// <summary>
        /// ResourceGroupLocation
        /// </summary>
        public string ResourceGroupLocation { get; set; }

        /// <summary>
        /// PPCReaderRoleId
        /// </summary>
        public string PPCReaderRoleId { get; set; }

        /// <summary>
        /// ContributorReaderRoleId
        /// </summary>
        public string ContributorRoleId { get; set; }

        /// <summary>
        /// MainResourceGroup
        /// </summary>
        public string MainResourceGroup { get; set; }

        /// <summary>
        /// ADProjectGroupId
        /// </summary>
        public string ADProjectGroupId { get; set; }


    }
}
