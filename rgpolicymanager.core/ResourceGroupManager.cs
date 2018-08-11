using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rgpolicymanager.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core.ResourceActions;

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
            ServiceClientCredentials serviceClientCredentials = await _authenticationHelper.GetServiceClientCredentials(ApplicationConstants.RESOURCE_URI.MANAGEMENT);

            Microsoft.Azure.Management.ResourceManager.ResourceManagementClient resourceManagementClient = new Microsoft.Azure.Management.ResourceManager.ResourceManagementClient(serviceClientCredentials);

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


        /// <summary>
        /// Assigns role to main resource group and newly created resource group
        /// </summary>
        /// <param name="mainResourceGroup"></param>
        /// <param name="ppcReaderRoleId"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="contributorRoleId"></param>
        /// <param name="adGroupId"></param>
        /// <returns></returns>
        public async Task AssignRoles(string mainResourceGroup, string ppcReaderRoleId,string resourceGroup, string contributorRoleId, string adGroupId)
        {
            ServiceClientCredentials serviceClientCredentials = await _authenticationHelper.GetServiceClientCredentials(ApplicationConstants.RESOURCE_URI.MANAGEMENT);

            Microsoft.Azure.Management.ResourceManager.ResourceManagementClient resourceManagementClient = new Microsoft.Azure.Management.ResourceManager.ResourceManagementClient(serviceClientCredentials);

            resourceManagementClient.SubscriptionId = _appSettings.Subscriptionid;

            ResourceGroup mainRG = await resourceManagementClient.ResourceGroups.GetAsync(mainResourceGroup);

            await AssignRoles(mainRG.Id,ppcReaderRoleId,adGroupId);

            ResourceGroup rg = await resourceManagementClient.ResourceGroups.GetAsync(resourceGroup);

            await AssignRoles(rg.Id, contributorRoleId, adGroupId);
        }

     

        /// <summary>
        /// Assign the role if it does not exists
        /// </summary>
        /// <param name="resourceGroupId"></param>
        /// <param name="roleId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>

        public async Task AssignRoles(string resourceGroupId, string roleId, string principalId)
        {
            var authenticated = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(_authenticationHelper.GetAzureCrendentials()).WithSubscription(_appSettings.Subscriptionid);

            string roleDefinitionId = $"/subscriptions/{_appSettings.Subscriptionid}/providers/Microsoft.Authorization/roleDefinitions/{roleId}";

            var roleAssignments = await authenticated.AccessManagement.RoleAssignments.ListByScopeAsync(resourceGroupId);

            if (roleAssignments != null)
            {
                var roleAssignmentsList = roleAssignments.ToList();

                var existingAssignment = roleAssignmentsList.Where(x => (x.PrincipalId.Equals(principalId) && x.RoleDefinitionId.Equals(roleDefinitionId))).FirstOrDefault();

                if (existingAssignment is default(IRoleAssignment))
                {
                    await authenticated.AccessManagement.RoleAssignments
                                .Define(SdkContext.RandomGuid())
                                .ForObjectId(principalId)
                                .WithRoleDefinition(roleDefinitionId)
                                .WithScope(resourceGroupId)
                                .CreateAsync();

                }
            }
        }
    }
}
