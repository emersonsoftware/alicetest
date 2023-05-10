using RefconGatewayBase.Configuration;

namespace RefconGatewayBase;

public class LocalConfig : Config
{
    public static string RefconHubConnectionString => GetSettingFromConfig("RefconHubConnectionString", "");

    public static string MinRefconFileVersion => GetSettingFromConfig("MinRefconFileVersion", "6.0.0");

    public static int IdleDurationMinutes => GetSettingFromConfig("IdleDurationMinutes", 3);

    public static int FunctionRetryCount => GetSettingFromConfig("FunctionRetryCount", 2);

    public static int EmailProcessRetryCount => GetSettingFromConfig("EmailProcessRetryCount", 5);

    public class EmailConfiguration
    {
        public static string Host => GetSettingFromConfig("EmailConfiguration:Host", "sitestatistics@reefers-online.com");

        public static int Port => GetSettingFromConfig("EmailConfiguration:Port", 143);

        public static string Username => GetSettingFromConfig("EmailConfiguration:Username", "");

        public static string Password => GetSettingFromConfig("EmailConfiguration:Password", "");
    }

    public class BlobStorage
    {
        public static string ContainerName => GetSettingFromConfig("BlobStorage:ContainerName", "");

        public static string ConnectionString => GetSettingFromConfig("StorageConnectionString", "");

        public static int TransientErrorRetryCount => GetSettingFromConfig("BlobStorage:TransientErrorRetryCount", 5);
    }

    public class ServiceBus
    {
        public static string ConnectionString => GetSettingFromConfig("ServiceBus:ConnectionString", "");

        public static string QueueName => GetSettingFromConfig("ServiceBus:QueueName", "sbus-dev-stats");

        public static string TopicName => GetSettingFromConfig("ServiceBus:TopicName", "blestats");

        public static int TransientErrorRetryCount => GetSettingFromConfig("ServiceBus:TransientErrorRetryCount", 5);

        public static int TransientErrorSleepDurationMS => 5000;
    }
}