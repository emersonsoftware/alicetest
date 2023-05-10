using System;
using System.Threading.Tasks;
using MailKit;
using Refcon.Common.Processing;

namespace RefconGatewayBase.Mail;

/// <summary>
/// Interface for REFCON Email Client. Implements methods required to process messages
/// </summary>
public interface IEmailClient : IDisposable
{
    /// <summary>
    /// Cancel the 'cancel' token which communicates request for cancellation to the server.
    /// Call this in the main program class when closing or ending the application.
    /// </summary>
    void Exit();

    /// <summary>
    /// Fetch mail from the server. Calls RefconMailHandler to process each message.
    /// Handles reconnect if disconnect occurs.
    /// </summary>
    /// <returns>Number of emails processed</returns>
    Task<int> ProcessInboxAsync();

    /// <summary>
    /// Checks if ImapClient IsConnected, call ConnectAsync if not connected.
    /// Then check if client IsAuthenticated, authenticate and open Inbox.
    /// </summary>
    /// <returns></returns>
    Task ReconnectAsync();

    /// <summary>
    /// Main method to start EmailClient.
    /// Connects to the mail server Inbox and fetches existing mail.
    /// Processes all emails with MailProcessor
    /// Expunges flagged emails
    /// </summary>
    /// <returns>Process results</returns>
    Task<ProcessResult<int>> RunAsync();
}