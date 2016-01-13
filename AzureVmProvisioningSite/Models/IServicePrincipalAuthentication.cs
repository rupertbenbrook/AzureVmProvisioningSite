using System.Threading.Tasks;

namespace AzureVmProvisioningSite.Models
{
    public interface IServicePrincipalAuthentication
    {
        Task<string> AquireTokenAsync();
    }
}