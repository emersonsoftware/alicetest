using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RefconGatewayBase.Configuration;

namespace Refcon.EmailGatewayClient;

public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
                   .ConfigureFunctionsWorkerDefaults()
                   .Build();
        var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
        var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
        var actualRoot = localRoot ?? azureRoot;
        var config = new ConfigurationBuilder()
                     .SetBasePath(actualRoot)
                     .AddJsonFile("local.settings.json", true, true)
                     .AddEnvironmentVariables()
                     .Build();
        ConfigBase.SetConfig(config);
        host.Run();
    }
}