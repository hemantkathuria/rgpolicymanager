using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Options;
using rgpolicymanager.core.Entities;
using Newtonsoft.Json;

namespace rgpolicymanager.core
{
    /// <summary>
    /// Policy Manager
    /// </summary>
    public class PolicyManager
    {
        /// <summary>
        /// AppSettings
        /// </summary>
        private readonly AppSettings _appSettings;

        /// <summary>
        /// AuthenticationHelper
        /// </summary>
        private readonly AuthenticationHelper _authenticationHelper;

        /// <summary>
        /// PROJECT_REPLACE_TOKEN
        /// </summary>
        private const string PROJECT_REPLACE_TOKEN = "$PRJ$";

        /// <summary>
        /// Policy Matter
        /// </summary>
        /// <param name="config"></param>
        /// <param name="authenticationHelper"></param>
        public PolicyManager(IOptions<AppSettings> appSettings, AuthenticationHelper authenticationHelper)
        {
            _appSettings = appSettings.Value;

            _authenticationHelper = authenticationHelper;
        }

        /// <summary>
        /// Assigns the initiative
        /// </summary>
        /// <param name="initiativeName"></param>
        /// <param name="scope"></param>
        /// <param name="assignmentName"></param>
        /// <returns></returns>
        public async Task<PolicyAssignment> AssignInitiative(string initiativeName,string projectCode, string scope, string assignmentName, Tags tags)
        {
            var serviceCredentials = await _authenticationHelper.GetServiceClientCredentials();

            PolicyClient client = new PolicyClient(serviceCredentials);

            string subscriptionid = _appSettings.Subscriptionid;

            client.SubscriptionId = subscriptionid;

            PolicyAssignment existingAssignment = null;

            try
            {
                existingAssignment = await client.PolicyAssignments.GetAsync(scope, assignmentName);
            }
            catch (ErrorResponseException) { }
            
            if (existingAssignment != null)
            {
                await client.PolicyAssignments.DeleteAsync(scope, assignmentName);
            }
            
            var initiativeParameters = GetInitiativeParameters(tags, projectCode);

            var json = JsonConvert.SerializeObject(initiativeParameters);

            var parameters = JObject.Parse(json);
            
            PolicySetDefinition initiative = await client.PolicySetDefinitions.GetAsync(initiativeName);

            PolicyAssignment assignment = new PolicyAssignment();

            assignment.PolicyDefinitionId = initiative.Id;

            assignment.Parameters = parameters;

            return await client.PolicyAssignments.CreateAsync(scope, assignmentName, assignment);
        }

        /// <summary>
        /// Returns the initiative Details
        /// </summary>
        /// <param name="initiativeName"></param>
        /// <returns></returns>
        public async Task<PolicySetDefinition> GetInitiative(string initiativeName)
        {
            var serviceCredentials = await _authenticationHelper.GetServiceClientCredentials();

            PolicyClient client = new PolicyClient(serviceCredentials);

            string subscriptionid = _appSettings.Subscriptionid;

            client.SubscriptionId = subscriptionid;

            PolicySetDefinition initiative = await client.PolicySetDefinitions.GetAsync(initiativeName);

            return initiative;
        }

        /// <summary>
        /// DeleteInitiative
        /// </summary>
        /// <param name="initiativeName"></param>
        /// <returns></returns>
        public async Task DeleteInitiative(string initiativeName)
        {
            var serviceCredentials = await _authenticationHelper.GetServiceClientCredentials();

            PolicyClient client = new PolicyClient(serviceCredentials);

            string subscriptionid = _appSettings.Subscriptionid;

            client.SubscriptionId = subscriptionid;

            await client.PolicySetDefinitions.DeleteAsync(initiativeName);
        }

        /// <summary>
        /// CreateOrUpdateInitiative
        /// </summary>
        /// <param name="initiativeName"></param>
        /// <returns></returns>
        public async Task CreateOrUpdateInitiative(string initiativeName, PolicySetDefinition policySetDefinition)
        {
            var serviceCredentials = await _authenticationHelper.GetServiceClientCredentials();

            PolicyClient client = new PolicyClient(serviceCredentials);

            string subscriptionid = _appSettings.Subscriptionid;

            client.SubscriptionId = subscriptionid;

            await client.PolicySetDefinitions.CreateOrUpdateAsync(initiativeName, policySetDefinition);
        }


        /// <summary>
        /// GetInitiativeParameters
        /// </summary>
        /// <param name="tags"></param>
        private InitiativeParameters GetInitiativeParameters(Tags tags, string projectCode)
        {
            InitiativeParameters parameters = new InitiativeParameters();

            parameters.infyBusinessUnitValue = new InfyBusinessUnitValue() { value = tags.INFY_EA_BusinessUnit };

            parameters.infyCostCenterValue = new InfyCostCenterValue() { value = tags.INFY_EA_CostCenter };

            parameters.infyEACustomTag01Value = new InfyEACustomTag01Value() { value = tags.INFY_EA_CustomTag01 };

            parameters.infyEACustomTag02Value = new InfyEACustomTag02Value() { value = tags.INFY_EA_CustomTag02 };

            parameters.infyEACustomTag03Value = new InfyEACustomTag03Value() { value = tags.INFY_EA_CustomTag03 };

            parameters.infyEAPurposeValue = new InfyEAPurposeValue() { value = tags.INFY_EA_Purpose };

            parameters.infyEAWorkloadTypeValue = new InfyEAWorkloadTypeValue() { value = tags.INFY_EA_WorkLoadType };

            parameters.infyProjectCodeValue = new InfyProjectCodeValue() { value = tags.INFY_EA_ProjectCode };

            parameters.vmNamePatternValueValue = new VmNamePatternValueValue() { value = _appSettings.VMNamePattern };

            parameters.asNamePatternValueValue = new AsNamePatternValueValue() { value = _appSettings.ASNamePattern };

            parameters.vmNamePatternValueValue.value = parameters.vmNamePatternValueValue.value.Replace(PROJECT_REPLACE_TOKEN, projectCode);

            parameters.asNamePatternValueValue.value = parameters.asNamePatternValueValue.value.Replace(PROJECT_REPLACE_TOKEN, projectCode);

            return parameters;

        }
    }
}
