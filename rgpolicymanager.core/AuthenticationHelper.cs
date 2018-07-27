using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using rgpolicymanager.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace rgpolicymanager.core
{
    /// <summary>
    /// Authentication helper
    /// </summary>
    public class AuthenticationHelper
    {
        /// <summary>
        /// _config
        /// </summary>
        private readonly AppSettings _appSettings;

        public AuthenticationHelper(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Returns service credentials
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceClientCredentials> GetServiceClientCredentials()
        {
            AuthenticationContext authContext = new AuthenticationContext(_appSettings.Authority);

            AuthenticationResult authResult = await authContext.AcquireTokenAsync(_appSettings.Resource, new ClientCredential(_appSettings.Clientid, _appSettings.Clientsecret));

            string accessToken = authResult.AccessToken;

            ServiceClientCredentials serviceClientCreds = new TokenCredentials(authResult.AccessToken);

            return serviceClientCreds;
        }
    }
}
