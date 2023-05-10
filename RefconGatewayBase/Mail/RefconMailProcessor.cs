using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Azure.ServiceBus;
using MimeKit;
using Newtonsoft.Json;
using RefconGatewayBase.Contracts;
using RefconGatewayBase.Logging;
using RefconGatewayBase.Peripherals;
using RefconGatewayBase.Services;

namespace RefconGatewayBase.Mail;

/// <summary>
/// Manages REFCON mails fetched by the EmailClient.
/// Extract attachments, save to storage, and post message with storage location
/// </summary>
public class RefconMailProcessor : IMailProcessor
{
    private readonly IQueueMessageHandler _messageHandler;
    private readonly IRefconStorageService _storageService;
    private readonly RefconQueueMessageRetry retry;

    /// <summary>
    /// Constructor for Unit Tests
    /// </summary>
    /// <param name="storageService">Service class to manage storing of the mail attachments</param>
    /// <param name="messageHandler">Handler class to manage notifications for saved attachments</param>
    public RefconMailProcessor(IRefconStorageService storageService, IQueueMessageHandler messageHandler)
    {
        _storageService = storageService;
        _messageHandler = messageHandler;

        retry = new RefconQueueMessageRetry();
    }

    /// <summary>
    /// Requires existing ImapClient for executing actions against mail server.
    /// Parse message attachments and save/post to blob storage and queues.
    /// Catch any exceptions against message and do not mark error message for delete
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ProcessAsync(IImapClient client, IMessageSummary message)
    {
        if (SkipMessageProcessing(message)) { return; }

        var mailSummary = RefconEmailMessageSummary.MapToRefconMessage(message);
        var mailJson = JsonConvert.SerializeObject(mailSummary);

        Log.Info($"Processing REFCON stats message: {mailJson}. Start time {DateTime.UtcNow}");

        if (message.Attachments.Any())
        {
            var attachmentNames = new List<string>();
            var serviceBusMessageSuccessful = false;
            foreach (var attachment in message.Attachments)
            {
                if (attachment.Octets == 0)
                {
                    // Octets is byte size of attachment. 0 means attachment is empty.
                    // log warning and allow email to continue processing remaining attachments
                    Log.Warning($"Attachment {attachment.FileName} for email {mailJson} contains no content.");
                    attachmentNames.Add($"{attachment.FileName}-NoContent");
                    continue;
                }

                var attachmentSummary = new RefconAttachmentSummary { Id = Guid.NewGuid().ToString(), EmailSummary = mailSummary, FileName = attachment.FileName };

                // since message is just a mail summary, we need to call mail server again and get the actual attachment
                var attachmentEntity = await client.Inbox.GetBodyPartAsync(message.UniqueId, attachment);

                // blob client returns URI where the file is saved. update the attachment summary with the blob URI
                var attachmentUrl = SaveToBlobStorage(attachmentEntity, attachmentSummary);
                attachmentSummary.FileUrl = attachmentUrl;

                if (!string.IsNullOrEmpty(attachmentUrl)) // if uri is empty, then there was an error saving to blob
                {
                    attachmentSummary.Created = DateTime.UtcNow;
                    attachmentNames.Add(attachmentSummary.GetStorageFileName());

                    // post the blob URI to service bus queues so REFCON Gateway knows to start processing the attachment saved in blob storage
                    serviceBusMessageSuccessful = await SendNotificationToServiceBusQueue(attachmentSummary);
                }
            }

            if (attachmentNames.Count == message.Attachments.Count() && serviceBusMessageSuccessful)
            {
                Log.Info($"All {attachmentNames.Count} attachments saved for email {mailJson}. Flagging email for delete.");
                await AddFlagAsync(client, mailSummary, MessageFlags.Deleted);
            }
            else { Log.Error($"Failed to save all attachments for email {mailJson}. Email will not be deleted from server. Urls saved: {string.Join(",", attachmentNames)}"); }
        }
        else
        {
            Log.Error($"No attachments found in email {mailJson}. Flagging email for delete.");
            await AddFlagAsync(client, mailSummary, MessageFlags.Deleted);
        }
    }

    #region Private Methods

    /// <summary>
    /// Returns true if the message should not be processed.
    /// Ex: if message contains Deleted flag, it has already been processed, so skip.
    /// </summary>
    /// <returns></returns>
    private bool SkipMessageProcessing(IMessageSummary message)
    {
        // message has flags
        if (message.Flags != null || message.Flags.HasValue)
        {
            // message has Deleted flag
            if (message.Flags.Value.HasFlag(MessageFlags.Deleted))
            {
                Log.Info($"message contains Deleted flag. Skip processing. Message: Id={message.UniqueId.Id}, From={message.Envelope.From.Mailboxes.Select(x => x.Address).First()}, Sent={message.Envelope.Date?.UtcDateTime}, Subject={message.Envelope.Subject}");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Saves attachment to the blob storage.
    /// AttachmentSummary contains info such as GUID of the attachment and Email summary items to help build the unique file name for blob storage
    /// </summary>
    /// <param name="attachmentEntity">The MailKit object for attachment content</param>
    /// <param name="attachmentSummary">Contract for attachments in RefconGateway</param>
    /// <returns>Returns storage URI string or empty string if error</returns>
    private string SaveToBlobStorage(MimeEntity attachmentEntity, RefconAttachmentSummary attachmentSummary)
    {
        var attachmentUrl = string.Empty;

        try
        {
            attachmentUrl = _storageService.SaveEmailAttachment(attachmentEntity, attachmentSummary);

            Log.Info($"Attachment saved to blob. {JsonConvert.SerializeObject(attachmentSummary)}. Save time {DateTime.UtcNow}");
        }
        catch (Exception ex) { Log.Error($"Exception while saving attachment to blob for attachment {JsonConvert.SerializeObject(attachmentSummary)}", ex); }

        return attachmentUrl;
    }

    /// <summary>
    /// Send attachment saved notification to service bus queues. Logs exception, if any.
    /// </summary>
    /// <param name="summary"></param>
    /// <returns></returns>
    private async Task<bool> SendNotificationToServiceBusQueue(RefconAttachmentSummary summary)
    {
        try
        {
            var message = new RefconQueueMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(summary)));
            await _messageHandler.SendAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            if (ex is TimeoutException || ex is ServiceBusCommunicationException || ex is ServerBusyException)
            {
                if (await retry.RetryMessageSend(ex, _messageHandler, summary)) { return true; }
            }

            Log.Error($"Exception while sending attachment saved notification to service bus queue. Attachment may not process: {JsonConvert.SerializeObject(summary)}", ex);
            return false;
        }
    }

    /// <summary>
    /// Add a flag to the message on the mail server
    /// </summary>
    /// <param name="client">ImapClient with connection to the server</param>
    /// <param name="message">EmailMessage to flag. Contains the UniqueId</param>
    /// <param name="flag">MailKit.MessageFlags enumeration</param>
    /// <returns></returns>
    private async Task AddFlagAsync(IImapClient client, RefconEmailMessageSummary message, MessageFlags flag)
    {
        try
        {
            var uid = new UniqueId(Convert.ToUInt32(message.UniqueId));
            await client.Inbox.AddFlagsAsync(uid, flag, false); // set silent = false, we want to raise MessageFlagsChanged event
        }
        catch (Exception ex) { Log.Error($"Failed to flag email {JsonConvert.SerializeObject(message)} with flag {flag}", ex); }
    }

    #endregion Private Methods
}