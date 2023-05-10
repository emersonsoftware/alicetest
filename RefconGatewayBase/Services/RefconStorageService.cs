using System;
using System.IO;
using MimeKit;
using RefconGatewayBase.Contracts;
using RefconGatewayBase.Helpers;
using RefconGatewayBase.Logging;
using RefconGatewayBase.Storage.Blobs;

namespace RefconGatewayBase.Services;

/// <summary>
/// Inherits from IRefconFileStorageService.
/// This storage service uses Azure Blob Storage to save REFCON files
/// </summary>
public class RefconStorageService : IRefconStorageService
{
    private readonly IBlobService blobClient;

    /// <summary>
    /// Inject an instance of BlobService using GatewayServicesFactory
    /// </summary>
    /// <param name="client"></param>
    public RefconStorageService(IBlobService client)
    {
        blobClient = client;
    }

    /// <summary>
    /// Saves the attachment to a designated container and folder in blob storage
    /// </summary>
    /// <typeparam name="T">The object type of the attachment</typeparam>
    /// <param name="attachment">The attachment data to save</param>
    /// <param name="attachmentSummary">attachment summary object which contains attachment identifier information, including RefconEmailMessageSummary items</param>
    /// <returns></returns>
    public string SaveEmailAttachment<T>(T attachment, RefconAttachmentSummary attachmentSummary)
    {
        var entity = attachment as MimeEntity;

        string uriResult;
        var path = attachmentSummary.GetStorageFileName();

        using (var memory = new MemoryStream())
        {
            // attachments can be either message/rfc822 parts or regular MIME parts
            if (entity is MimePart part) { part.Content.DecodeTo(memory); }
            else { ((MessagePart)entity)?.Message.WriteTo(memory); }

            var byteStream = memory.ToArray();
            uriResult = blobClient.StoreFileAsync(new MemoryStream(byteStream), path, "application/octet-stream").GetAwaiter().GetResult();
        }

        return uriResult;
    }

    /// <summary>
    /// Saves the attachment to a designated container and folder in blob storage. If exception occurs, performs retries until maximum retries is exceeded
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="attachment">The attachment to save</param>
    /// <param name="attachmentSummary">Summary contains information such as unique attachment GUID and mail summary items</param>
    /// <param name="folder">blob folder to save the file</param>
    /// <returns></returns>
    public string SaveEmailAttachmentWithRetry<T>(T attachment, RefconAttachmentSummary attachmentSummary)
    {
        string uriResult;

        var retryCount = LocalConfig.BlobStorage.TransientErrorRetryCount;

        while (true)
        {
            try
            {
                uriResult = SaveEmailAttachment(attachment, attachmentSummary);
                return uriResult;
            }
            catch (Exception retryEx)
            {
                if (retryCount-- <= 0)
                {
                    Log.Error($"Retries expired for attachment {attachmentSummary.Id} having name {attachmentSummary.FileName} .", retryEx);
                    return ""; // send empty string back to RefconMailHandler because we could not get a URI
                }

                Log.Warning($"Exception while saving attachment {attachmentSummary.Id} having name {attachmentSummary.FileName} to blob, retrying (maxRetry = {LocalConfig.BlobStorage.TransientErrorRetryCount})", retryEx);
            }
        }
    }

    /// <summary>
    /// Get the attachment file from blob storage
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public Stream GetFile(IFileSummary summary, StorageFolder folder)
    {
        var fileName = summary.GetStorageFileName();
        return blobClient.FetchFileAsync(folder.Value + fileName, "application/octet-stream").GetAwaiter().GetResult();
    }

    /// <summary>
    /// Save the refcon file that was extracted from email attachment
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="summary"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public string SaveFile(Stream stream, IFileSummary summary, StorageFolder folder)
    {
        // test
        //throw new Exception("test exception");

        string uriResult;

        using (var ms = (MemoryStream)stream)
        {
            var path = $"{folder.Value}{summary.GetStorageFileName()}";

            var byteStream = ms.ToArray();
            uriResult = blobClient.StoreFileAsync(new MemoryStream(byteStream), path, "application/octet-stream").GetAwaiter().GetResult();
        }

        return uriResult;
    }

    /// <summary>
    /// Get the refcon file from its original location and save to the new location.
    /// Delete the file from the original location
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="fromFolder"></param>
    /// <param name="toFolder"></param>
    /// <returns></returns>
    public string MoveFile(IFileSummary summary, StorageFolder fromFolder, StorageFolder toFolder)
    {
        var file = GetFile(summary, fromFolder);

        var newUrl = SaveFile(file, summary, toFolder);

        DeleteFile(summary, fromFolder);

        return newUrl;
    }

    /// <summary>
    /// Delete the refcon file from the storage folder
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="folder"></param>
    public async void DeleteFile(IFileSummary summary, StorageFolder folder)
    {
        var path = folder.Value + summary.GetStorageFileName();
        await blobClient.DeleteFileAsync(path);
    }
}