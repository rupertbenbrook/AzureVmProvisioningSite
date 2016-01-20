# AzureVmProvisioningSite
A website that fronts the Azure Resource Manager API to provision and manage VMs and snapshots.

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Frupertbenbrook%2FAzureVmProvisioningSite%2Frelease%2Fazuredeploy.json)

## Deployment and Configuration

### Azure Active Directory Configuration
This website relies on Azure Active Directory (AAD) both for authenticating users to the site and for
authenticating with the Azure Resource Management (ARM) APIs to manage virtual machines. This requires an
application to be registered in AAD prior to the deployment of the website to Azure so that the appropriate
application configuration can be included in the website deployment parameters. There are three steps to this
registration:

* Creation of an AAD application
* Delegation to ARM APIs
* Creation of an application key

[Integrating Applications with Azure Active Directory](https://azure.microsoft.com/en-us/documentation/articles/active-directory-integrating-applications/)

### Azure Resource Manager Configuration
* Create a Resource Group to contain the virtual machines under management
* Apply permissions to the Resource Group to enable the website to manage virtual machines

### Website Deployment

### Application Configuration
This application has a number of application configuration settings that need to be set correctly.
These settings can be set either in the web.config file or application settings in Azue Web App,
but given the sensitive nature of a number of these settings the recommendation is to use Azure Web App
settings.

**ClientId** - the Client ID obtained from your Azure Active Directory application regstration that identifies
this application.

**AuthorityUriBase** - the URL base for obtaining tokens from Azure Active Directory. Typically this takes the
form https://login.windows.net/*tenantId*, where *tenantId* is the ID of your Azire Active Directory tenant.
This URL can be obtained from the Endpoints button of your Azure Active Directory Application configuration.

**PostLogoutRedirectUri** - the URL to go to when the user logs out of the website. This can be any URL you like
but should make sense with the experience of logging out from the site.

**DomainHint** - the domain hint to pass to the Azure Active Directory login experience. This is typically
*domainName*.onmicrosoft.com, where *domainName* is the original name of your Azure Active Directory tenant.
Using this domain hint can avoid the UI switch that happens when the Azure Active Directory login discovers
the tenant from the user login domain name.

**ClientSecret** - this is the key generated for your application in Azure Active Directory, and is used to
authenticate the application with Azure Resource Manager.

**TokenResource** - the URL base for Azure Resource Manager to obtain a token for authentication. This is
typically https://management.core.windows.net.

**SubscriptionId** - the subscription ID for the subscription containing the virtual machine resources
to manage.

**DeploymentResourceGroup** - the name of the resource group in the subscription containing the virtual machine
resources to manage. This resource group needs to exist and have the appropriate permissions applied for this
application to manage virtual machine resources.

> Copyright 2016 Rupert Benbrook
>
>Licensed under the Apache License, Version 2.0 (the "License");
>you may not use this file except in compliance with the License.
>You may obtain a copy of the License at
>
>   http://www.apache.org/licenses/LICENSE-2.0
>
>Unless required by applicable law or agreed to in writing, software
>distributed under the License is distributed on an "AS IS" BASIS,
>WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
>See the License for the specific language governing permissions and
>limitations under the License.