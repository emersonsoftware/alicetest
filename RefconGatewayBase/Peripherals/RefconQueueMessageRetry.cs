using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RefconGatewayBase.Contracts;
using RefconGatewayBase.Logging;

namespace RefconGatewayBase.Peripherals;

/// <summary>
/// Retry strategy for failure to post attachment summary notifications to Service Bus
/// </summary>
public class RefconQueueMessageRetry
{
    public async Task<bool> RetryMessageSend(Exception ex, IQueueMessageHandler client, RefconAttachmentSummary attachment)
    {
        var retryCount = LocalConfig.ServiceBus.TransientErrorRetryCount;

        Log.Info($"Hit exception {ex} Retrying refcon attachment notification message");

        while (true)
        {
            try
            {
                var messageBody = JsonConvert.SerializeObject(attachment);
                var message = new RefconQueueMessage(Encoding.UTF8.GetBytes(messageBody));
                await client.SendAsync(message);

                Log.Info($"{retryCount} retries succeeded on {LocalConfig.ServiceBus.QueueName} queue");
                return true;
            }
            catch (Exception retryEx)
            {
                if (retryEx is TimeoutException)
                {
                    if (retryCount-- <= 0)
                    {
                        // Done retries. Giving up;
                        Log.Error($"Retries expired for {LocalConfig.ServiceBus.QueueName} queue. giving up !!");
                        return false;
                    }
                }
                else
                {
                    Log.Error($"Hit exception {retryEx} Giving up for the {LocalConfig.ServiceBus.QueueName} !!");
                    return false;
                }
            }
        }
    }
}