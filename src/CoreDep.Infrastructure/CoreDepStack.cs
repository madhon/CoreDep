namespace CoreDep;

using Pulumi;
using Pulumi.AzureNative.Web.Inputs;
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
                Sku = new SkuDescriptionArgs
                {
                    Size = appServicePlanSize,
                    Tier = appServicePlanTier,
                    Name = appServicePlanSize,
                },
            }, new CustomResourceOptions()
            {
                Protect = false,
            });

            const string imageInDockerHub = "madhon/madhonsite:1450";
            const string fullImageUri = "index.docker.io/madhon/madhonsite:1450";

            var coreDepApp = new AzureNativeWeb.WebApp($"{AppServiceBaseName}-{env}", new AzureNativeWeb.WebAppArgs
            {
                Name = $"{AppServiceBaseName}-{env}",
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                ServerFarmId = appServicePlan.Id,
                Identity = new ManagedServiceIdentityArgs
                {
                    Type = AzureNativeWeb.ManagedServiceIdentityType.SystemAssigned,
                    //UserAssignedIdentities = new [] { "userAssignedIdentityId" },
                },
                SiteConfig = new SiteConfigArgs
                {
                    AppSettings = new []
                    {
                        new NameValuePairArgs
                        {
                            Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                            Value = "false",
                        },
                    },
                    AlwaysOn = false,
                    Use32BitWorkerProcess = true, // has to be 32bit for Free tier
                    MinTlsVersion = AzureNativeWeb.SupportedTlsVersions.SupportedTlsVersions_1_2,
                    Http20Enabled = true,
                    RemoteDebuggingEnabled = false,
                    FtpsState = AzureNativeWeb.FtpsState.Disabled,
                    LinuxFxVersion = $"sitecontainers",
                },
                Kind = "app,linux",
                HyperV = false,
                HttpsOnly = true,
            }, new CustomResourceOptions
            {
                Protect = false,
            });

            var coreDepDiagConfig = new AzureNativeWeb.WebAppDiagnosticLogsConfiguration($"{AppServiceBaseName}-{env}",
                new AzureNativeWeb.WebAppDiagnosticLogsConfigurationArgs
                {
                    Name = coreDepApp.Name,
                    ResourceGroupName = resourceGroup.Name,
                    ApplicationLogs = new ApplicationLogsConfigArgs
                    {
                       FileSystem = new FileSystemApplicationLogsConfigArgs
                       {
                           Level = AzureNativeWeb.LogLevel.Error,
                       }
                    },
                    HttpLogs = new HttpLogsConfigArgs
                    {
                        FileSystem = new FileSystemHttpLogsConfigArgs
                        {
                            Enabled = false,
                            RetentionInDays = 10,
                            RetentionInMb = 35,
                        },
                    },
                }, new CustomResourceOptions
                {
                    DependsOn= coreDepApp,
                    Protect = false,
                });

            var container = new AzureNativeWeb.WebAppSiteContainer($"{AppServiceBaseName}-{env}",
                new AzureNativeWeb.WebAppSiteContainerArgs
                {
                    Name = coreDepApp.Name,
                    ContainerName = "main",
                    ResourceGroupName = resourceGroup.Name,
                    Image = imageInDockerHub,
                    IsMain = true,
                    TargetPort = "8080",
                    Kind = "app,linux,container",
                }, new CustomResourceOptions
                {
                    DependsOn= coreDepApp,
                    Protect = false,
                }
            );

        this.CoreDepEndpoint = Output.Format($"https://{coreDepApp.DefaultHostName}");
    }
}
