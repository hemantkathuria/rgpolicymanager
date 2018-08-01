using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using rgpolicymanager.core;
using rgpolicymanager.core.Entities;
using System;
using System.Threading.Tasks;

namespace rgpolicymanager
{
    class Program
    {
        private static ServiceProvider _serviceProvider;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Staring Process!");

            // Create service collection
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            ResourceGroupManager resourceGroupManager = _serviceProvider.GetService<ResourceGroupManager>();

            PolicyManager policyManager = _serviceProvider.GetService<PolicyManager>();

            IOptions<Tags> tags = _serviceProvider.GetService<IOptions<Tags>>();

            IOptions<AppSettings> appSettings = _serviceProvider.GetService<IOptions<AppSettings>>();

            string initiativeAssignmentName = $"{appSettings.Value.ResourceGroupName}_{appSettings.Value.Initiativename}";

            var resourceGroup = await resourceGroupManager.EnsureResourceGroupExists(appSettings.Value.ResourceGroupName, appSettings.Value.ResourceGroupLocation, tags.Value);

            Console.WriteLine("Resource Group Created!");

            var initiative = await policyManager.AssignInitiative(appSettings.Value.Initiativename, appSettings.Value.ProjectCode, resourceGroup.Id, initiativeAssignmentName, tags.Value);

            Console.WriteLine("Initiative Assigned!");

            await resourceGroupManager.AssignRoles(appSettings.Value.MainResourceGroup,
                                                    appSettings.Value.PPCReaderRoleId, appSettings.Value.ResourceGroupName, appSettings.Value.ContributorRoleId, appSettings.Value.ADProjectGroupId);

            Console.WriteLine("Roles Assigned!");

            Console.ReadLine();
        }

        //private async Task InitiativeCode()
        //{
            //PolicySetDefinition  psDefinition = await policyManager.GetInitiative("infyppcinitiativeall");

            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(psDefinition);

            //string readJson= System.IO.File.ReadAllText("D:\\Data\\Infosys\\PPC\\policies\\rgpolicymanager\\Policies\\initiative.json");

            //PolicySetDefinition policySetDefinition = Newtonsoft.Json.JsonConvert.DeserializeObject<PolicySetDefinition>(readJson);

            //await policyManager.CreateOrUpdateInitiative("infyppcinitiativeall", policySetDefinition);
        //}

        /// <summary>
        /// Configure Configuration, Logger and Other Services in Dependency Injection
        /// </summary>
        /// <param name="serviceCollection"></param>
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton(configuration);

            serviceCollection.AddOptions();

            serviceCollection.Configure<AppSettings>(appSettings =>
            {
                IConfigurationSection appSettingsSection = configuration.GetSection("AppSettings");

                appSettings.TenantId = appSettingsSection["tenantid"]; 

                appSettings.Authority = appSettingsSection["authority"];

                appSettings.Clientid = appSettingsSection["clientid"];

                appSettings.Clientsecret = appSettingsSection["clientsecret"];

                appSettings.Resource = appSettingsSection["resource"];

                appSettings.Subscriptionid = appSettingsSection["subscriptionid"];

                appSettings.Initiativename = appSettingsSection["initiativename"];

                appSettings.VMNamePattern = appSettingsSection["vmNamePattern"];

                appSettings.ASNamePattern = appSettingsSection["asNamePattern"];

                appSettings.ResourceGroupName = appSettingsSection["resourcegroupname"];

                appSettings.ResourceGroupLocation = appSettingsSection["resourcegrouplocation"];

                appSettings.ProjectCode = appSettingsSection["projectcode"];

                appSettings.PPCReaderRoleId = appSettingsSection["ppcreaderroleid"];

                appSettings.ContributorRoleId = appSettingsSection["contributorroleid"];

                appSettings.MainResourceGroup = appSettingsSection["mainresourcegroup"];

                appSettings.ADProjectGroupId = appSettingsSection["adprojectgroupid"];

            });

            serviceCollection.Configure<Tags>(tags =>
            {
                IConfigurationSection tagsSection = configuration.GetSection("Tags");

                tags.INFY_EA_BusinessUnit = tagsSection["INFY_EA_BusinessUnit"];

                tags.INFY_EA_CostCenter = tagsSection["INFY_EA_CostCenter"];

                tags.INFY_EA_CustomTag01 = tagsSection["INFY_EA_CustomTag01"];

                tags.INFY_EA_CustomTag02 = tagsSection["INFY_EA_CustomTag02"];

                tags.INFY_EA_CustomTag03 = tagsSection["INFY_EA_CustomTag03"];

                tags.INFY_EA_ProjectCode = tagsSection["INFY_EA_ProjectCode"];

                tags.INFY_EA_Purpose = tagsSection["INFY_EA_Purpose"];

                tags.INFY_EA_WorkLoadType = tagsSection["INFY_EA_WorkLoadType"];
                
            });

            serviceCollection.AddTransient<AuthenticationHelper>();

            serviceCollection.AddTransient<PolicyManager>();

            serviceCollection.AddTransient<ResourceGroupManager>();
        }
    }
}
