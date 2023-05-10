using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Refcon.Common.Utilities;
using RefconGatewayBase.Mail;

namespace RefconGatewayBase.Factories;

public static class ClientFactory
{
    private static RefconEmailClient mailClient;

    /// <summary>
    /// Manages single instance of RefconEmailClient.
    /// </summary>
    /// <returns></returns>
    public static RefconEmailClient GetRefconEmailClient()
    {
        if (mailClient == null)
        {
            var mailProcessor = ProcessorFactory.GetRefconMailProcessor();

            mailClient = new RefconEmailClient(
                new ImapClient(),
                mailProcessor,
                new DateTimeHelper(),
                LocalConfig.EmailConfiguration.Host,
                LocalConfig.EmailConfiguration.Port,
                LocalConfig.EmailConfiguration.Username,
                LocalConfig.EmailConfiguration.Password);
        }

        return mailClient;
    }

    /// <summary>
    /// Returns new ImapClient that is connected, authenticated, and Inbox is opened.
    /// Use this for the ExpungeMail Timer Trigger.
    /// </summary>
    /// <returns></returns>
    public static async Task<IImapClient> CreateImapClient()
    {
        var client = new ImapClient();
        var cancel = new CancellationTokenSource();

        await client.ConnectAsync(LocalConfig.EmailConfiguration.Host, LocalConfig.EmailConfiguration.Port, SecureSocketOptions.None, cancel.Token);
        await client.AuthenticateAsync(LocalConfig.EmailConfiguration.Username, LocalConfig.EmailConfiguration.Password, cancel.Token);
        await client.Inbox.OpenAsync(FolderAccess.ReadWrite, cancel.Token);

        return client;
    }
}