using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;

namespace RefconGatewayBase.Peripherals;

/// <summary>
/// ServiceBus Queues message object
/// </summary>
public class RefconQueueMessage : IQueueMessage
{
    /// <summary>
    /// For refcon service bus queues, use the RefconAttachmentSummary class in RefconGatewayBase.Contracts
    /// </summary>
    /// <param name="attachmentSummary"></param>
    public RefconQueueMessage(byte[] attachmentSummary)
        : this(new Message(attachmentSummary)) { }

    public RefconQueueMessage(Message message)
    {
        Message = message;
    }

    public Message Message { get; }

    public T GetBody<T>()
    {
        return Message.GetBody<T>();
    }

    public IDictionary<string, object> Properties => Message.UserProperties;

    public DateTime ScheduledEnqueueTimeUtc
    {
        get => Message.ScheduledEnqueueTimeUtc;
        set => Message.ScheduledEnqueueTimeUtc = value;
    }

    public IQueueMessage Clone()
    {
        return new RefconQueueMessage(Message.Clone());
    }
}