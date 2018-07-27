using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using rgpolicymanager.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace rgpolicymanager.core
{
    public class ResourceGroupManager
    {
        /// <summary>
        /// _appSettings
        /// </summary>
        private readonly AppSettings _appSettings;

        /// <summary>
        /// _authenticationHelper
        /// </summary>
        private readonly AuthenticationHelper _authenticationHelper = null;

        /// <summary>
        /// ResourceGroupManager
        /// </summary>
        /// <param name="config"></param>
        public ResourceGroupManager(IOptions<AppSettings> appSettings,AuthenticationHelper authenticationHelper)
        {
            _authenticationHelper = authenticationHelper;

            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Ensures that a resource group with the specified name exists. If it does not, will attempt to create one.
        /// </summary>
        /// <param name="resourceManagementClient">The resource manager client.</param>
        /// <param name="resourceGroupName">The name of the resource group.</param>
        /// <param name="resourceGroupLocation">The resource group location. Required when creating a new resource group.</param>
        public async Task<ResourceGroup> EnsureResourceGroupExists(string resourceGroupName, string resourceGroupLocation , Tags tags)
        {
            ServiceClientCredentials serviceClientCredentials = await _authenticationHelper.GetServiceClientCredentials();

            ResourceManagementClient resourceManagementClient = new ResourceManagementClient(serviceClientCredentials);

            resourceManagementClient.SubscriptionId = _appSettings.Subscriptionid;

            if (!resourceManagementClient.ResourceGroups.CheckExistence(resourceGroupName))
            {
                var resourceGroup = new ResourceGroup();

                var keyValueTags = tags.GetTags();

                resourceGroup.Tags = new Dictionary<string, string>();

                if (tags != null && keyValueTags.Count > 0)
                {
                    foreach(KeyValuePair<string, string> tag in keyValueTags)
                    {
                        resourceGroup.Tags.Add(tag);
                    }
                }

                resourceGroup.Location = resourceGroupLocation;

                return await resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(resourceGroupName, resourceGroup);

            }else
            {
                return await resourceManagementClient.ResourceGroups.GetAsync(resourceGroupName);
            }
        }
    }
}
