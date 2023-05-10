using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using RefconGatewayBase.Logging;
using Refcon.Common.Processing;
using Refcon.Common.Utilities;

namespace RefconGatewayBase.Mail;

/// <summary>
/// Inherits from IEmailService and based on MailKit nuget package.
/// Opens a connection to the mail server for REFCON emails and retrieves emails in the inbox.
/// Sets client to idle state when all emails are retrieved.
/// Reference documentation : http://www.mimekit.net/docs/html/M_MailKit_Net_Imap_ImapClient_Idle.htm
/// </summary>
public class RefconEmailClient : IEmailClient
{
    private readonly string _host;
    private readonly string _password;
    private readonly int _port;
    private readonly SecureSocketOptions _sslOptions;
    private readonly string _username;
    private readonly CancellationTokenSource cancel;
    private readonly IDateTimeHelper dateTimeHelper;

    private readonly IImapClient client;
    private readonly IMailProcessor mailProcessor;

    /// <summary>
    /// Constructor for ClientFactory
    /// Creates new instance of RefconEmailService with configuration settings in LocalConfig and local.settings.json
    /// </summary>
    /// <param name="host">Mail Server Host Name</param>
    /// <param name="port">Port Number</param>
    /// <param name="user">Username Credential</param>
    /// <param name="password">Password Credential</param>
    public RefconEmailClient(IImapClient client, IMailProcessor mailProcessor, IDateTimeHelper dateTimeHelper, string host, int port, string user, string password)
    {
        _host = host;
        _port = port;
        _sslOptions = SecureSocketOptions.None;
        _username = user;
        _password = password;

        this.client = client;
        this.mailProcessor = mailProcessor;
        this.dateTimeHelper = dateTimeHelper;
        cancel = new CancellationTokenSource();
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        client.Dispose();
        cancel.Dispose();
    }

    /// <summary>
    /// Create and open the connection to mail server
    /// </summary>
    /// <returns></returns>
    public async Task ReconnectAsync()
    {
        Log.Info($"ReconnectAsync() called. client.IsConnected: {client.IsConnected}, client.IsAuthenticated: {client.IsAuthenticated}");

        if (!client.IsConnected) { await client.ConnectAsync(_host, _port, _sslOptions, cancel.Token); }

        if (!client.IsAuthenticated)
        {
            await client.AuthenticateAsync(_username, _password, cancel.Token);
            await client.Inbox.OpenAsync(FolderAccess.ReadWrite, cancel.Token);
        }
    }

    /// <summary>
    /// Fetch mail from the server. Calls RefconMailHandler to process each message.
    /// Handles reconnect if disconnect occurs.
    /// </summary>
    /// <returns></returns>
    public async Task<int> ProcessInboxAsync()
    {
        int numRetries = LocalConfig.EmailProcessRetryCount;

        do
        {
            try
            {
                var fetched = client.Inbox.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure | MessageSummaryItems.Envelope | MessageSummaryItems.Flags, cancel.Token);
                Log.Info($"{fetched.Count} mails fetched.");

                foreach (var message in fetched) { await mailProcessor.ProcessAsync(client, message); }

                return fetched.Count;
            }
            catch (ImapProtocolException ex)
            {
                // protocol exceptions often result in the client getting disconnected
                Log.Warning("ImapProtocolException occurred", ex);
                await ReconnectAsync();
            }
            catch (IOException ex)
            {
                // I/O exceptions always result in the client getting disconnected
                Log.Warning("IOException occurred", ex);
                await ReconnectAsync();
            }
            catch (ServiceNotConnectedException ex)
            {
                // client is disconnected. try again.
                Log.Warning("ServiceNotConnectedException occurred", ex);
                await ReconnectAsync();
            }
            catch (ServiceNotAuthenticatedException ex)
            {
                Log.Warning("ServiceNotAuthenticatedException occurred", ex);
                await ReconnectAsync();
            }
            catch (Exception ex) { Log.Error("Unhandled exception occurred", ex); }
        }
        while (numRetries-- > 0);

        return 0;
    }

    /// <summary>
    /// Expunge all flagged emails
    /// </summary>
    /// <returns></returns>
    public async Task ExpungeEmailsAsync()
    {
        await client.Inbox.ExpungeAsync();
    }

    /// <summary>
    /// Main method to start EmailClient.
    /// Connects to the mail server Inbox and fetches existing mail.
    /// Processes all emails with MailProcessor
    /// Expunges flagged emails
    /// </summary>
    /// <returns></returns>
    public async Task<ProcessResult<int>> RunAsync()
    {
        var retVal = new ProcessResult<int>();
        var msgCode = $"{GetType()} - {nameof(RunAsync)}";

        // connect to the IMAP server and get our initial list of messages
        try
        {
            await ReconnectAsync();
            int emailsProcessed = await ProcessInboxAsync();
            await ExpungeEmailsAsync();

            retVal.IsSuccessful = true;
            retVal.Data = emailsProcessed;
        }
        catch (OperationCanceledException ex)
        {
            Log.Error("OperationCanceledException occurred. Disconnecting client...", ex);
            await client.DisconnectAsync(true);

            retVal.IsSuccessful = false;
            retVal.AddMessage(
                new ResultMessage
                {
                    Timestamp = dateTimeHelper.CurrentTime,
                    Code = msgCode,
                    Message = "OperationCanceledException occurred. Disconnecting client...",
                    Detail = ex.Message
                });
        }
        catch (Exception ex)
        {
            retVal.IsSuccessful = false;
            retVal.AddMessage(
                new ResultMessage
                {
                    Timestamp = dateTimeHelper.CurrentTime,
                    Code = msgCode,
                    Message = "Unexpected error occurred while running email client. Disconnecting client...",
                    Detail = ex.Message
                });
        }

        return retVal;
    }

    public void Exit()
    {
        cancel.Cancel();
    }
}