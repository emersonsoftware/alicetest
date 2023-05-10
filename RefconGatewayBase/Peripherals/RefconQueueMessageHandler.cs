using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using RefconGatewayBase.Logging;

namespace RefconGatewayBase.Peripherals;

/// <summary>
/// Manage the transfer of messages in the refcon service bus queue
/// </summary>
public class RefconQueueMessageHandler : IQueueMessageHandler
{
    private readonly ITopicClient topicClient;

    public RefconQueueMessageHandler(ITopicClient topicClient)
    {
        this.topicClient = topicClient;
    }

    public async Task CloseAsync()
    {
        if (!topicClient.IsClosedOrClosing) { await topicClient.CloseAsync(); }
        else { Log.Error($"CloseAsync for the queue target: {topicClient.TopicName} was already closed "); }
    }

    public Task CompleteAsync(IQueueMessage message)
    {
        throw new NotImplementedException();
    }

    public Task DeadLetter(IQueueMessage message, string deadLetterReason, string deadLetterErrorDescription)
    {
        throw new NotImplementedException();
    }

    public void OnMessage(Action<IQueueMessage> callback)
    {
        throw new NotImplementedException();
    }

    public async Task SendAsync(IQueueMessage message)
    {
        if (!topicClient.IsClosedOrClosing) { await topicClient.SendAsync(message.Message); }
        else
        {
            Log.Error($"SendAsync for the queue target: {topicClient.TopicName} was already closed ");
            throw new ServiceBusCommunicationException("Service bus topic is closed");
        }
    }
}