using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RefconGatewayBase.AzureFunctions;
using RefconGatewayBase.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]

namespace RefconGatewayBase.AzureFunctions;

public class Startup : FunctionsStartup
{
    /// <inheritdoc />
    public override void Configure(IFunctionsHostBuilder builder)
    {
        try
        {
            // retrieve the AF path using environment varialble based on where the AF is being executed local/host
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            var actualRoot = localRoot ?? azureRoot;
            var config = new ConfigurationBuilder()
                         .SetBasePath(actualRoot)
                         .AddJsonFile("local.settings.json", true, true)
                         .AddEnvironmentVariables()
                         .Build();
            ConfigBase.SetConfig(config);
        }
        catch (Exception ex)
        {
            Console.Write("Error on startUp configure" + ex.Message);
            throw;
        }
    }
}