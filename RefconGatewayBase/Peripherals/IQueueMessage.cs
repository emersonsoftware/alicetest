using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;

namespace RefconGatewayBase.Peripherals;

/// <summary>
/// Interface for ServiceBus messages (taken from CommonStandard.Peripherals.IPushMessage)
/// </summary>
public interface IQueueMessage
{
    /// <summary>
    /// The custom message metadata
    /// </summary>
    IDictionary<string, object> Properties { get; }

    /// <summary>
    /// Get or set the enqueue delay minutes for the message
    /// </summary>
    DateTime ScheduledEnqueueTimeUtc { get; set; }

    /// <summary>
    /// ServiceBus message
    /// </summary>
    Message Message { get; }

    /// <summary>
    /// Gets the body of the message
    /// </summary>
    /// <typeparam name="T">The data contract of the message body</typeparam>
    /// <returns></returns>
    T GetBody<T>();

    /// <summary>
    /// Create a new IMessage which contains the cloned ServiceBus.Message object
    /// </summary>
    /// <returns></returns>
    IQueueMessage Clone();
}