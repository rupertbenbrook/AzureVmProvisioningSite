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
* Apply permissions to the Resource Group to enable the website to 

### Website Deployment


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