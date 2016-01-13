using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureVmProvisioningSite.Models
{
    public class ServicePrincipalAuthentication : IServicePrincipalAuthentication
    {
        private static string _token;
        private static DateTimeOffset? _renew;

        public async Task<string> AquireTokenAsync()
        {
            if ((_renew != null) && (_token != null))
            {
                if (_renew >= DateTimeOffset.UtcNow)
                {
                    return _token;
                }
            }
            var authenticationContext = new AuthenticationContext(Configuration.AuthorityUriBase);
            var credential = new ClientCredential(Configuration.ClientId, Configuration.ClientSecret);
            var res = await authenticationContext.AcquireTokenAsync(Configuration.TokenResource, credential);
            if (res == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            _renew = res.ExpiresOn.Subtract(new TimeSpan((res.ExpiresOn - DateTimeOffset.UtcNow).Ticks / 2));
            _token = res.AccessToken;
            return _token;
        }
    }
}