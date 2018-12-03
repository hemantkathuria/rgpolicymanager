using CsvHelper;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using rgpolicymanager.core;
using rgpolicymanager.core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace rgpolicymanager
{
    /// <summary>
    /// Service Provider
    /// </summary>
    class Program
    {
        private static ServiceProvider _serviceProvider;
        
        /// <summary>
        /// Main Method
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Staring Process!");

                // Create service collection
                var serviceCollection = new ServiceCollection();

                ConfigureServices(serviceCollection);

                _serviceProvider = serviceCollection.BuildServiceProvider();

                //await InitiativeCode();

                ResourceGroupManager resourceGroupManager = _serviceProvider.GetService<ResourceGroupManager>();

                PolicyManager policyManager = _serviceProvider.GetService<PolicyManager>();

                GraphManager graphManager = _serviceProvider.GetService<GraphManager>();

                IOptions<Tags> tags = _serviceProvider.GetService<IOptions<Tags>>();

                IOptions<AppSettings> appSettings = _serviceProvider.GetService<IOptions<AppSettings>>();

                string initiativeAssignmentName = $"{appSettings.Value.ResourceGroupName}_{appSettings.Value.Initiativename}";

                var resourceGroup = await resourceGroupManager.EnsureResourceGroupExists(appSettings.Value.ResourceGroupName, appSettings.Value.ResourceGroupLocation, tags.Value);

                Console.WriteLine("Resource Group Created!");

                var initiative = await policyManager.AssignInitiative(appSettings.Value.Initiativename, appSettings.Value.ProjectCode, resourceGroup.Id, initiativeAssignmentName, tags.Value);

                Console.WriteLine("Initiative Assigned!");

                var groupid = await graphManager.EnsureUserGroupMembershipExists(appSettings.Value.ADPUserEmailAddress, appSettings.Value.ADPUserEmailAddress, appSettings.Value.InviteUserRedirectUri, appSettings.Value.ADProjectGroupName);

                Console.WriteLine("Ensured User, Group and Membership Exists!");

                await resourceGroupManager.AssignRoles(appSettings.Value.MainResourceGroup,
                                                        appSettings.Value.PPCReaderRoleId, appSettings.Value.ResourceGroupName,
                                                        appSettings.Value.ContributorRoleId, groupid);

                Console.WriteLine("Roles Assigned to Group on Resource Group!");

                Console.WriteLine("Press any key to exit!");
                
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        #region
        //private static async Task InitiativeCode()
        //{
        //    PolicyManager policyManager = _serviceProvider.GetService<PolicyManager>();

        //    PolicySetDefinition psDefinition = await policyManager.GetInitiative("infyppcinitiativeall");

        //    string json = Newtonsoft.Json.JsonConvert.SerializeObject(psDefinition);

        //    string readJson = System.IO.File.ReadAllText("D:\\Data\\Infosys\\PPC\\policies\\rgpolicymanager\\Policies\\initiative.json");

        //    PolicySetDefinition policySetDefinition = Newtonsoft.Json.JsonConvert.DeserializeObject<PolicySetDefinition>(readJson);

        //    await policyManager.CreateOrUpdateInitiative("infyppcinitiativeall", policySetDefinition);
        //}
        #endregion

        /// <summary>
        /// Configure Configuration, Logger and Other Services in Dependency Injection
        /// </summary>
        /// <param name="serviceCollection"></param>
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            string csvFilename = "InputData.csv";

            bool csvExists = System.IO.File.Exists(csvFilename);

            List<CSVSetting> csvRecords = null;

            CSVSetting csvSetting = null;

            if (csvExists)
            {
                TextReader reader = new StreamReader(csvFilename);

                var csv = new CsvReader(reader);

                csv.Configuration.RegisterClassMap<CsvSettingMap>();

                csvRecords = csv.GetRecords<CSVSetting>().ToList<CSVSetting>();

                csvSetting = csvRecords.FirstOrDefault<CSVSetting>(c => c.Status.Equals("NEW", StringComparison.InvariantCultureIgnoreCase));
            }
            
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

                appSettings.Subscriptionid = appSettingsSection["subscriptionid"];

                appSettings.Initiativename = appSettingsSection["initiativename"];

                appSettings.PPCReaderRoleId = appSettingsSection["ppcreaderroleid"];

                appSettings.ContributorRoleId = appSettingsSection["contributorroleid"];

                appSettings.MainResourceGroup = appSettingsSection["mainresourcegroup"];

                appSettings.InviteUserRedirectUri = appSettingsSection["inviteuserredirecturi"];

                /////////////

                if (csvExists && csvSetting != null)
                {
                    appSettings.VMNamePattern = csvSetting.VMNamePattern;

                    appSettings.ASNamePattern = csvSetting.ASNamePattern;

                    appSettings.ResourceGroupName = csvSetting.ResourceGroupName;

                    appSettings.ResourceGroupLocation = csvSetting.ResourceGroupLocation;

                    appSettings.ProjectCode = csvSetting.ProjectCode;

                    appSettings.ADProjectGroupName = csvSetting.ADProjectGroupName;

                    appSettings.ADPUserEmailAddress = csvSetting.ADPUserEmailAddress;
                }
                else
                {
                    appSettings.VMNamePattern = appSettingsSection["vmNamePattern"];

                    appSettings.ASNamePattern = appSettingsSection["asNamePattern"];

                    appSettings.ResourceGroupName = appSettingsSection["resourcegroupname"];

                    appSettings.ResourceGroupLocation = appSettingsSection["resourcegrouplocation"];

                    appSettings.ProjectCode = appSettingsSection["projectcode"];

                    appSettings.ADProjectGroupName = appSettingsSection["adprojectgroupname"];

                    appSettings.ADPUserEmailAddress = appSettingsSection["aduseremailaddress"];
                }
                
            });

            serviceCollection.Configure<Tags>(tags =>
            {
                if (csvExists && csvSetting != null)
                {
                    tags.INFY_EA_BusinessUnit = csvSetting.INFY_EA_BusinessUnit;

                    tags.INFY_EA_CostCenter = csvSetting.INFY_EA_CostCenter;

                    tags.INFY_EA_CustomTag01 = csvSetting.INFY_EA_CustomTag01;

                    tags.INFY_EA_CustomTag02 = csvSetting.INFY_EA_CustomTag02;

                    tags.INFY_EA_CustomTag03 = csvSetting.INFY_EA_CustomTag03;

                    tags.INFY_EA_ProjectCode = csvSetting.INFY_EA_ProjectCode;

                    tags.INFY_EA_Purpose = csvSetting.INFY_EA_Purpose;

                    tags.INFY_EA_WorkLoadType = csvSetting.INFY_EA_WorkLoadType;
                }
                else
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
                }
            });

            serviceCollection.AddTransient<AuthenticationHelper>();

            serviceCollection.AddTransient<PolicyManager>();

            serviceCollection.AddTransient<ResourceGroupManager>();

            serviceCollection.AddTransient<GraphManager>();
        }
    }
}
