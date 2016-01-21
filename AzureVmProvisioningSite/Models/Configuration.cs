using System.Configuration;

namespace AzureVmProvisioningSite.Models
{
    public static class Configuration
    {
        public static string AuthorityUriBase => ConfigurationManager.AppSettings["AuthorityUriBase"];
        public static string ClientId => ConfigurationManager.AppSettings["ClientId"];
        public static string ClientSecret => ConfigurationManager.AppSettings["ClientSecret"];
        public static string TokenResource => ConfigurationManager.AppSettings["TokenResource"];
        public static string SubscriptionId => ConfigurationManager.AppSettings["SubscriptionId"];
        public static string PostLogoutRedirectUri => ConfigurationManager.AppSettings["PostLogoutRedirectUri"];
        public static string RedirectUri => ConfigurationManager.AppSettings["RedirectUri"];
        public static string DeploymentResourceGroup => ConfigurationManager.AppSettings["DeploymentResourceGroup"];
        public static string DomainHint => ConfigurationManager.AppSettings["DomainHint"];
    }
}