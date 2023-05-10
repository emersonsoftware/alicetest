using Microsoft.Azure.ServiceBus;
using RefconGatewayBase.Peripherals;
using RefconGatewayBase.Services;
using RefconGatewayBase.Storage.Blobs;

namespace RefconGatewayBase.Factories;

/// <summary>
/// Creates the Services for RefconGateway processors.
/// Returns the existing Service/Client if one is already created.
/// </summary>
public static class ServiceFactory
{
    private static IQueueMessageHandler QueueMessageHandler;
    private static RefconStorageService StorageService;

    /// <summary>
    /// Returns the FileStorageService to get and save REFCON Attachments/Files
    /// Uses the BlobService (shared). Injects new or existing service into
    /// </summary>
    /// <returns></returns>
    public static IRefconStorageService GetRefconFileStorageService()
    {
        if (StorageService == null)
        {
            var client = new BlobService(LocalConfig.BlobStorage.ContainerName, LocalConfig.BlobStorage.ConnectionString);
            StorageService = new RefconStorageService(client);
        }

        return StorageService;
    }

    /// <summary>
    /// Used by the RefconEmailClient to send attachment summary notifications to refcon service bus queue
    /// </summary>
    /// <returns></returns>
    public static IQueueMessageHandler GetRefconQueueMessageHandler()
    {
        return QueueMessageHandler ??= new RefconQueueMessageHandler(
            new TopicClient(
                LocalConfig.ServiceBus.ConnectionString,
                LocalConfig.ServiceBus.TopicName));
    }
}