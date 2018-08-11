using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using rgpolicymanager.core.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace rgpolicymanager.core
{
    public class GraphManager
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
        public GraphManager(IOptions<AppSettings> appSettings, AuthenticationHelper authenticationHelper)
        {
            _authenticationHelper = authenticationHelper;

            _appSettings = appSettings.Value;
        }

        public async Task<string> EnsureUserGroupMembershipExists(string userDisplayName, string userEmailAddress,string inviteUserRedirectUri, string groupName)
        {
            string groupId = await EnsureAzureADGroupExists(groupName);

            string userId = await EnsureAzureADGuestUserExists(userDisplayName, userEmailAddress, inviteUserRedirectUri);

            await EnsureUserExistsInGroup(groupId, userId);

            return groupId;
        }

        /// <summary>
        /// Ensures the group Exists
        /// </summary>
        /// <param name="groupName"></param>
        private async Task<string> EnsureAzureADGroupExists(string groupName)
        {
            string groupId;

            var client = RestClient
            .Configure()
            .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
            .WithLogLevel(HttpLoggingDelegatingHandler.Level.None)
            .WithCredentials(_authenticationHelper.GetAzureCrendentials())
            .Build();

            GraphRbacManager graphRbacManager = new GraphRbacManager(client, _appSettings.TenantId);

            IActiveDirectoryGroup group = await graphRbacManager.Groups.GetByNameAsync(groupName);

            if (group == null)
            {
                GroupCreateParameters parameters = new GroupCreateParameters() { DisplayName = groupName, MailNickname = groupName };

                ADGroupInner groupInner = await graphRbacManager.Groups.Inner.CreateAsync(parameters);

                groupId = groupInner.ObjectId;
            }else
            {
                groupId = group.Id;
            }

            return groupId;
        }

        /// <summary>
        /// https://blogs.msdn.microsoft.com/premier_developer/2017/09/29/getting-started-with-the-azure-ad-b2b-invite-api/
        /// </summary>
        /// <param name="userPrincipalName"></param>
        /// <returns></returns>
        private async Task<string> EnsureAzureADGuestUserExists(string userDisplayName, string userEmailAddress, string redirectUri)
        {
            var client = RestClient
            .Configure()
            .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
            .WithLogLevel(HttpLoggingDelegatingHandler.Level.None)
            .WithCredentials(_authenticationHelper.GetAzureCrendentials())
            .Build();

            GraphRbacManager graphRbacManager = new GraphRbacManager(client, _appSettings.TenantId);

            IActiveDirectoryUser user = await graphRbacManager.Users.GetByNameAsync(userDisplayName);

            if (user == null)
            {
                return await InviteUser(userDisplayName, userEmailAddress, redirectUri);
            }
            else
            {
                return user.Id;
            }
        }


        /// <summary>
        /// EnsureUserExistsInGroup
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task EnsureUserExistsInGroup(string groupId, string userId)
        {
            ServiceClientCredentials credentails = await _authenticationHelper.GetServiceClientCredentials(ApplicationConstants.RESOURCE_URI.AD_GRAPH);

            GraphRbacManagementClient rbacClient = new GraphRbacManagementClient(credentails);

            rbacClient.TenantID = _appSettings.TenantId;

            CheckGroupMembershipParameters parameters = new CheckGroupMembershipParameters() { MemberId = userId, GroupId = groupId };

            var isMemberExists = await rbacClient.Groups.IsMemberOfAsync(parameters);

            if (isMemberExists.Value.HasValue && isMemberExists.Value.Value)
            {

            }
            else
            {
                string url = $"{ApplicationConstants.BASE_URL.AD_GRAPH}{_appSettings.TenantId}/directoryObjects/{userId}";

                GroupAddMemberParameters grouParameters = new GroupAddMemberParameters() { Url = url };

                await rbacClient.Groups.AddMemberAsync(groupId, grouParameters);
            }
        }


        /// <summary>
        /// Invites B2B User
        /// </summary>
        /// <param name="userDisplayName"></param>
        /// <param name="emailAddress"></param>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        private async Task<string> InviteUser(string userDisplayName, string emailAddress, string redirectUrl)
        {
            AuthenticationResult authenticationResult = await _authenticationHelper.GetGraphAuthenticationResult();

            InvitationModel invite = new InvitationModel();

            invite.invitedUserDisplayName = userDisplayName;

            invite.invitedUserEmailAddress = emailAddress;

            invite.inviteRedirectUrl = redirectUrl;

            invite.sendInvitationMessage = true;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{ApplicationConstants.BASE_URL.MICROSOFT_GRAPH}");

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                
                HttpResponseMessage response = await client.PostAsync("v1.0/invitations", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(invite)));

                string inviteResultString = await response.Content.ReadAsStringAsync();

                InvitationModel inviteResult = Newtonsoft.Json.JsonConvert.DeserializeObject<InvitationModel>(inviteResultString);

                return inviteResult.invitedUser.id;
            }
        }

    }
}
