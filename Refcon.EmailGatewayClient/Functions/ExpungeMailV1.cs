using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using RefconGatewayBase.Factories;
using RefconGatewayBase.Logging;

namespace Refcon.EmailGatewayClient.Functions;

public class ExpungeMailV1 : FunctionBase
{
    //[Function("ExpungeMailV1")]
    public static async Task Run([TimerTrigger("%ExpungeMailTimerInterval%", RunOnStartup = false, UseMonitor = false)] MyInfo myTimer, FunctionContext context)
    {
        var logger = context.GetLogger("ExpungeMailV1");
        await ExecuteWithRetries(
            logger,
            async () =>
            {
                Log.Info("ExpungeMail Timer triggered.");

                using var client = await ClientFactory.CreateImapClient();
                await client.Inbox.ExpungeAsync();
            },
            1,
            1,
            false);
    }
}