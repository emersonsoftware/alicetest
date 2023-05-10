using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MailKit;

namespace RefconGatewayBase.Mail;

/// <summary>
/// Used by RefconGatewayMailClient
/// The model for MailKit messages
/// </summary>
public class RefconEmailMessageSummary
{
    public RefconEmailMessageSummary()
    {
        From = new List<string>();
    }

    /// <summary>
    /// UTC DateTime when the message was fetched from the mail server for processing
    /// </summary>
    [Required]
    public DateTime ReceivedDate { get; set; }

    /// <summary>
    /// Email message unique ID
    /// </summary>
    [Required]
    public string UniqueId { get; set; }

    /// <summary>
    /// List of senders
    /// </summary>
    public List<string> From { get; set; }

    /// <summary>
    /// UTC DateTime when the email was sent, if available
    /// </summary>
    public DateTime? Sent { get; set; }

    /// <summary>
    /// Subject of the email message
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Transfer only the data we need from Mailkit IMessageSummary to our RefconMessageSummary object
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static RefconEmailMessageSummary MapToRefconMessage(IMessageSummary message)
    {
        return new RefconEmailMessageSummary
        {
            ReceivedDate = DateTime.UtcNow,
            UniqueId = message.UniqueId.Id.ToString(),
            From = message.Envelope.From.Mailboxes.Select(x => x.Address).ToList(),
            Sent = message.Envelope.Date?.UtcDateTime,
            Subject = message.Envelope.Subject
        };
    }
}