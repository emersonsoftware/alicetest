using System;
using System.Threading.Tasks;

namespace RefconGatewayBase.Peripherals;

/// <summary>
/// Interface for handling ServiceBus messages. Copied from CommonStandard.Peripherals.IMessageHandler
/// </summary>
public interface IQueueMessageHandler
{
    /// <summary>
    /// Sends a brokered message on the quaue
    /// </summary>
    /// <param name="message">The message to send to the queue</param>
    Task SendAsync(IQueueMessage message);

    void OnMessage(Action<IQueueMessage> callback);

    Task CloseAsync();

    Task CompleteAsync(IQueueMessage message);

    Task DeadLetter(IQueueMessage message, string deadLetterReason, string deadLetterErrorDescription);
}