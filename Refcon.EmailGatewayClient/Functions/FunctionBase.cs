using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RefconGatewayBase.Logging;

namespace Refcon.EmailGatewayClient.Functions;

public class FunctionBase
{
    private static readonly Random Random = new();
    private static readonly object InitLock = new();
    private static bool LoggerInitialized;

    protected static async Task ExecuteWithRetries(ILogger logger, Func<Task> action, int retries = 0, int logErrorAfterRetries = 20, bool enabledLogger = true)
    {
        var retryCounter = 0;
        if (enabledLogger) { InitLogger(logger); }

        while (true)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception e)
            {
                if (retryCounter >= retries)
                {
                    logger.LogError($"Data processing failed after {retryCounter} retries", e);
                    throw;
                }

                await Task.Delay(Random.Next(200, 10000));
                retryCounter++;

                if (retryCounter % logErrorAfterRetries == 0)
                {
                    var message = $"Data processing still failing after {retryCounter} retries with message: " + e.Message;
                    logger.LogError(message, e);
                }
            }
        }
    }

    protected static void InitLogger(ILogger logger)
    {
        if (!LoggerInitialized)
        {
            lock (InitLock)
            {
                if (!LoggerInitialized)
                {
                    Log.AddLogger(new LogILogger(logger));
                    LoggerInitialized = true;
                }
            }
        }
    }
}