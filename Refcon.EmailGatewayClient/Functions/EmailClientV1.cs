using System;
using System.Threading.Tasks;
using MailKit.Net.Imap;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Refcon.Common.Utilities;
using RefconGatewayBase;
using RefconGatewayBase.Factories;
using RefconGatewayBase.Logging;
using RefconGatewayBase.Mail;
using RefconGatewayBase.Peripherals;
using RefconGatewayBase.Services;
using RefconGatewayBase.Storage.Blobs;

namespace Refcon.EmailGatewayClient.Functions;

public class EmailClientV1 : FunctionBase
{
    [Function("EmailClientV1")]
    public static async Task Run([TimerTrigger("%TimerInterval%", RunOnStartup = false, UseMonitor = false)] MyInfo myTimer, FunctionContext context)
    {
        var logger = context.GetLogger("EmailClientV1");
        await ExecuteWithRetries(
            logger,
            async () =>
            {
                logger.LogTrace("Starting Refcon Gateway EmailClient.");
                using var client = GetRefconEmailClient();
                var mailClientResult = await client.RunAsync();

                var status = mailClientResult.IsSuccessful ? "Succeeded" : "Failed";
                logger.LogInformation($"Status: {status}. Shutting down Refcon Gateway EmailClient");
                client.Exit();
            },
            LocalConfig.FunctionRetryCount,
            LocalConfig.FunctionRetryCount,
            false);
    }

    private static RefconEmailClient GetRefconEmailClient()
    {
        var mailProcessor = GetRefconMailProcessor();

        var mailClient = new RefconEmailClient(
            new ImapClient(),
            mailProcessor,
            new DateTimeHelper(),
            LocalConfig.EmailConfiguration.Host,
            LocalConfig.EmailConfiguration.Port,
            LocalConfig.EmailConfiguration.Username,
            LocalConfig.EmailConfiguration.Password);

        return mailClient;
    }

    private static IMailProcessor GetRefconMailProcessor()
    {
        return new RefconMailProcessor(GetRefconFileStorageService(), GetRefconQueueMessageHandler());
    }

    private static IRefconStorageService GetRefconFileStorageService()
    {
        var client = new BlobService(LocalConfig.BlobStorage.ContainerName, LocalConfig.BlobStorage.ConnectionString);
        return new RefconStorageService(client);
    }

    /// <summary>
    /// Used by the RefconEmailClient to send attachment summary notifications to refcon service bus queue
    /// </summary>
    /// <returns></returns>
    public static IQueueMessageHandler GetRefconQueueMessageHandler()
    {
        return new RefconQueueMessageHandler(
            new TopicClient(
                LocalConfig.ServiceBus.ConnectionString,
                LocalConfig.ServiceBus.TopicName));
    }
}

public class MyInfo
{
    public MyScheduleStatus ScheduleStatus { get; set; }

    public bool IsPastDue { get; set; }
}

public class MyScheduleStatus
{
    public DateTime Last { get; set; }

    public DateTime Next { get; set; }

    public DateTime LastUpdated { get; set; }
}