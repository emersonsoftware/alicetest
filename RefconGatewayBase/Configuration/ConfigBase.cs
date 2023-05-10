using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using RefconGatewayBase.Logging;

namespace RefconGatewayBase.Configuration;

public abstract class ConfigBase
{
    private static IConfiguration config;

    public static void SetConfig(IConfiguration configuration)
    {
        config = configuration;
    }

    protected static T GetSettingFromConfig<T>(string settingName, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(settingName)) { throw new ArgumentException("settingName must have a value!"); }

        T value;
        if (config == null)
        {
            // Fallback to default config handler, this will not include local.settings.json
            config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        }

        try
        {
            var conf = config[settingName];
            if (!string.IsNullOrEmpty(conf)) { value = (T)Convert.ChangeType(config[settingName], typeof(T), CultureInfo.InvariantCulture); }
            else { value = defaultValue; }
        }
        catch (Exception ex)
        {
            if (Log.LoggersConfigured()) { Log.Warning($"Error reading setting:{settingName ?? string.Empty}, default value used:{defaultValue} - exception: {ex.Message}", ex); }

            value = defaultValue;
        }
#if DEBUG
        if (string.IsNullOrEmpty(value.ToString()) && settingName != "AuthServer:Audience") { Debugger.Break(); }
#endif
        return value;
    }
}