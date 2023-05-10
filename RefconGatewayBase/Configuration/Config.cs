namespace RefconGatewayBase.Configuration;

public class Config : ConfigBase
{
    #region Telemetry

    public static string TelemetryHubConnectionString => GetSettingFromConfig("TelemetryHubConnectionString", "");

    #endregion Telemetry

    #region GMS

    public static string GmsBaseUrl => GetSettingFromConfig("GmsBaseUrl", "");

    #endregion GMS

    #region AuthServer

    public class AuthServer
    {
        public static string BaseUrl => GetSettingFromConfig("AuthServer:BaseUrl", "");

        public static string Audience => GetSettingFromConfig("AuthServer:Audience", "");

        public static string APIKEY => GetSettingFromConfig("AuthServer:APIKEY", "cDUoUAsou02aEV3kVyOgwA==");
    }

    #endregion AuthServer

    #region UnifiedModel

    public class UnifiedModel
    {
        public static string BaseAddress => GetSettingFromConfig("UnifiedModel:BaseAddress", "");

        public static string FunctionKey => GetSettingFromConfig("UnifiedModel:FunctionKey", "");

        public static int UpdateMinutes => GetSettingFromConfig("UnifiedModel:UpdateMinutes", 15);
    }

    #endregion UnifiedModel

    #region API

    public static string ApiBaseAddress => GetSettingFromConfig("apibase_address", @"");

    public static string ApiKey => GetSettingFromConfig("APIkey", @"8AQW2GHbn1dyapXvuIIZ4ieqV548xyHE3NXPmq8wzmvHCkrFaUS6ZE69pv");

    #endregion API

    #region CosmosDb

    public static string CosmosEndpointUrl => GetSettingFromConfig("CosmosEndpointUrl", "");

    public static string CosmosPrimaryKey => GetSettingFromConfig("CosmosPrimaryKey", "");

    public static string CosmosTelemetryDatabase => GetSettingFromConfig("CosmosTelemetryDatabase", "Telemetry");

    #endregion CosmosDb

    #region Storage Account

    public static string StorageConnectionString => GetSettingFromConfig("StorageConnectionString", "");

    public static string CachingServiceFolder => GetSettingFromConfig("CachingServiceFolder", "");

    public static string ThumbnailContainer => GetSettingFromConfig("ThumbnailContainer", "thumbnails");

    public static int ThumbnailWidth => GetSettingFromConfig("ThumbnailWidth", 96);

    public static int ThumbnailHeight => GetSettingFromConfig("ThumbnailHeight", 96);

    #endregion Storage Account

    #region Logging

    public static bool LogAnalyticsEnabled
    {
        get
        {
            var debug = false;
#if DEBUG // To avoid logging to LogAnalytics from unittests etc.
            debug = true;
#endif
            if (debug) { return false; }

            return GetSettingFromConfig("LogAnalyticsEnabled", true);
        }
    }

    public static bool ConsoleLogEnabled => GetSettingFromConfig("ConsoleLogEnabled", false);

    public static string LogWorkpaceId => GetSettingFromConfig("LogWorkspaceId", "");

    public static string LogWorkspaceKey => GetSettingFromConfig("LogWorkspaceKey", "");

    public static string LogAnalyticsSystemName => GetSettingFromConfig("LogAnalyticsSystemName", "");

    #endregion Logging

    #region Service Bus

    public static string ServiceBusConnectionString => GetSettingFromConfig("ServiceBusConnectionString", "");

    public static string CommunicationTopicsSubscriberName => GetSettingFromConfig("Communication:TopicSubscriberName", "");

    #endregion Service Bus

    #region DepotTool External Api

    public static class Api
    {
        public static string BaseUrl => GetSettingFromConfig("Api:BaseUrl", "https://emersonpatapidev.azurewebsites.net");

        public static string ApiKey => GetSettingFromConfig("Api:ApiKey", "8AQW2GHbn1dyapXvuIIZ4ieqV548xyHE3NXPmq8wzmvHCkrFaUS6ZE69pv");
    }

    public static class VodafoneApi
    {
        public static string BaseUrl => GetSettingFromConfig("VodafoneApi:BaseUrl", "https://funcemersonvodafonediscdev.azurewebsites.net");

        public static string ApiKey => GetSettingFromConfig("VodafoneApi:ApiKey", "QQ3RHuRk4aTQqnQf8iymO4sygidVIE8Ruy0elBHOAh0b7DUNVHOGJQ==");
    }

    public static class AttApi
    {
        public static string BaseUrl => GetSettingFromConfig("AttApi:BaseUrl", "https://funcemersonattdiscdev.azurewebsites.net");

        public static string ApiKey => GetSettingFromConfig("AttApi:ApiKey", "DwqIzQkrFu69I9bHLce0P0A2n3Zzuyy3P83TRpDOxPVgyLJ97q2b/Q==");
    }

    public static class Google
    {
        public static string ApiKey => GetSettingFromConfig("Google:ApiKey", "AIzaSyARwLURZTtZF7pLWtT7lnmXeaSU3Z4G734");
    }

    #endregion DepotTool External Api

    #region HealthCheck

    public static class HealthCheck
    {
        public static string FunctionKey => GetSettingFromConfig("HealthCheck:FunctionKey", "HrJsT5I6N8eFo1lgFa8j89OxTZzJos/L/3hw6btA2SDmB8XuatcOmA==");

        public static string FunctionBaseAddress => GetSettingFromConfig("HealthCheck:FunctionBaseAddress", "https://funcemersonpathealthcheckreportdev.azurewebsites.net");

        public static int SourceProxitimityDistance => GetSettingFromConfig("HealthCheck:SourceProxitimityDistance", 200);

        public static int SourceProxitimityMaxCount => GetSettingFromConfig("HealthCheck:SourceProxitimityMaxCount", 50);
    }

    public static class MyFavorites
    {
        public static int RecentlyConnectedHours => GetSettingFromConfig("MyFavorites:RecentlyConnectedHours", 2);
    }

    public static class BicBoxTech
    {
        public static string UserName => GetSettingFromConfig("BicBoxTech:UserName", "emersonproact@gmail.com");

        public static string Password => GetSettingFromConfig("BicBoxTech:Password", "Richard5Refcon");
    }

    #endregion

    #region AuditData

    public static class AuditDataApi
    {
        public static string FunctionBaseUrl => GetSettingFromConfig("AuditDataApi:BaseUrl", "https://funcemersonpatauditservicedev.azurewebsites.net");

        public static string FunctionKey => GetSettingFromConfig("AuditDataApi:ApiKey", "xFbIat1qtHU/Kw5q7Z92DQzkvBUas2tZ69XEhqcIuB3oMHAxB4s5jQ==");
    }

    public static class Communication
    {
        public static string TopicSubscriberName => GetSettingFromConfig("Communication:TopicSubscriberName", "");

        public static string ServiceBusConnectionString => GetSettingFromConfig("Communication:ServiceBusConnectionString", "");
    }

    #endregion
}