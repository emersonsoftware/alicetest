using System;
using RefconGatewayBase.Mail;

namespace RefconGatewayBase.Contracts;

/// <summary>
/// Storage information for REFCON mail Attachment
/// </summary>
public class RefconAttachmentSummary : IFileSummary
{
    public RefconAttachmentSummary()
    {
        EmailSummary = new RefconEmailMessageSummary();
    }

    /// <summary>
    /// The EmailMessageSummary for which the attachment belongs
    /// </summary>
    public RefconEmailMessageSummary EmailSummary { get; set; }

    /// <summary>
    /// Attachment identifier (GUID)
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Attachment File Name
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Date and time which the attachment was saved to storage
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Location of the saved attachment in blob storage.
    /// Includes baseurl/foldername/filename
    /// </summary>
    public string FileUrl { get; set; }

    /// <summary>
    /// Returned the file name as the file by the publisher follow certain naming pattern
    /// </summary>
    /// <returns></returns>
    public string GetStorageFileName()
    {
        return FileName;
    }
}