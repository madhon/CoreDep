namespace CoreDep;

using Pulumi;
using Pulumi.AzureNative.Web.Inputs;
using Random = Pulumi.Random;
using AzureNativeResources = Pulumi.AzureNative.Resources;
using AzureNativeWeb = Pulumi.AzureNative.Web;

public class CoreDepStack : Stack
{
    [Output] public Output<string> CoreDepEndpoint { get; set; }

    private const string AppServicePlanBaseName = "coredep-asp";
    private const string AppServiceBaseName = "coredep-webapp";
    private const string ResourceGroupBaseName = "coredep-rg";

    public CoreDepStack()
    {
            var config = new Config();
            var region = config.Get("region") ?? "uksouth";

            var appServicePlanSize = config.Get("appServicePlanSize") ?? "F1";
            var appServicePlanTier = config.Get("appServicePlanTier") ?? "Free";

            var env = config.Get("env") ?? "dev";

            // Create the resource group
            var resourceGroup  = new AzureNativeResources.ResourceGroup($"{ResourceGroupBaseName}-{env}", new AzureNativeResources.ResourceGroupArgs
            {
                ResourceGroupName =  $"{ResourceGroupBaseName}-{env}",
                Location = region,
            });

            // Create the Linux App Service Plan
            var appServicePlan = new AzureNativeWeb.AppServicePlan($"{AppServicePlanBaseName}-{env}", new AzureNativeWeb.AppServicePlanArgs
            {
                Name = $"{AppServicePlanBaseName}-{env}",
                Location = resourceGroup .Location,
                ResourceGroupName = resourceGroup.Name,
                Kind = "Linux",
                Sku = new AzureNativeWeb.Inputs.SkuDescriptionArgs
                {
                    Size = appServicePlanSize,
                    Tier = appServicePlanTier,
                },
            });

            const string imageInDockerHub = "madhon/madhonsite:1120";

            var coreDepDiagConfig = new AzureNativeWeb.WebAppDiagnosticLogsConfiguration($"{AppServiceBaseName}-{env}",
                new AzureNativeWeb.WebAppDiagnosticLogsConfigurationArgs
                {
                    Name = $"{AppServiceBaseName}-{env}",
                    ResourceGroupName = resourceGroup.Name,
                });

            var coreDepApp = new AzureNativeWeb.WebApp($"{AppServiceBaseName}-{env}", new AzureNativeWeb.WebAppArgs
            {
                Name = $"{AppServiceBaseName}-{env}",
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                ServerFarmId = appServicePlan.Id,
                Identity = new AzureNativeWeb.Inputs.ManagedServiceIdentityArgs
                {
                    Type = AzureNativeWeb.ManagedServiceIdentityType.SystemAssigned,
                    //UserAssignedIdentities = new [] { "userAssignedIdentityId" },
                },
                SiteConfig = new AzureNativeWeb.Inputs.SiteConfigArgs
                {
                    AppSettings = new []
                    {
                        new AzureNativeWeb.Inputs.NameValuePairArgs
                        {
                            Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                            Value = "false",
                        },
                    },
                    AlwaysOn = false,
                    Use32BitWorkerProcess = false,
                    MinTlsVersion = "1.2",
                    Http20Enabled = true,
                    RemoteDebuggingEnabled = false,
                    FtpsState = "Disabled",
                    LinuxFxVersion = $"DOCKER|{imageInDockerHub}",
                },
                HttpsOnly = true,
            });


        this.CoreDepEndpoint = Output.Format($"https://{coreDepApp.DefaultHostName}");
    }
}
