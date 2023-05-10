using RefconGatewayBase.Mail;

namespace RefconGatewayBase.Factories;

/// <summary>
/// Creates the Processors for RefconGateway.
/// Returns the existing Processor if one is already created.
/// </summary>
public static class ProcessorFactory
{
    private static RefconMailProcessor EmailProcessor;

    /// <summary>
    /// Return shared static instance of RefconMailProcessor
    /// </summary>
    /// <returns></returns>
    public static IMailProcessor GetRefconMailProcessor()
    {
        if (EmailProcessor == null) { EmailProcessor = new RefconMailProcessor(ServiceFactory.GetRefconFileStorageService(), ServiceFactory.GetRefconQueueMessageHandler()); }

        return EmailProcessor;
    }
}