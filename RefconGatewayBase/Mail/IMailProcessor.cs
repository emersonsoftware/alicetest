using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;

namespace RefconGatewayBase.Mail;

/// <summary>
/// Interface to process an email using MailKit ImapClient and IMessageSummary
/// </summary>
public interface IMailProcessor
{
    /// <summary>
    /// Process the message.
    /// Requires a connected/authenticated ImapClient in order to fetch the actual email contents based on the MessageSummary.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task ProcessAsync(IImapClient client, IMessageSummary message);
}