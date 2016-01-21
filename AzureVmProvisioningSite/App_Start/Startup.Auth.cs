using System.Threading.Tasks;
using Config = AzureVmProvisioningSite.Models.Configuration;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace AzureVmProvisioningSite
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = Config.ClientId,
                    Authority = Config.AuthorityUriBase,
                    RedirectUri = Config.RedirectUri,
                    PostLogoutRedirectUri = Config.PostLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            context.ProtocolMessage.DomainHint = Config.DomainHint;
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}
