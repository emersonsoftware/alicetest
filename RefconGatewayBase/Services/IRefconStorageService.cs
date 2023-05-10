using System.IO;
using RefconGatewayBase.Contracts;
using RefconGatewayBase.Helpers;

namespace RefconGatewayBase.Services;

/// <summary>
/// Interface for storing/retrieving data
/// Use this with FileStorageFolders
/// </summary>
public interface IRefconStorageService
{
    /// <summary>
    /// Get the file stream from blob storage
    /// </summary>
    /// <param name="summary">summary object for the file</param>
    /// <param name="folder">blob folder which the file is saved</param>
    /// <returns></returns>
    Stream GetFile(IFileSummary summary, StorageFolder folder);

    /// <summary>
    /// Save raw attachment file directly from the mail server to blob storage.
    /// </summary>
    /// <typeparam name="T">determined by the IMAP client class used</typeparam>
    /// <param name="attachment">the attachment object to save</param>
    /// <param name="attachmentSummary">summary object used to save blob location information</param>
    /// <returns></returns>
    string SaveEmailAttachment<T>(T attachment, RefconAttachmentSummary attachmentSummary);

    /// <summary>
    /// Calls SaveEmailAttachment but with retries in case of failure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="attachment"></param>
    /// <param name="attachmentSummary"></param>
    /// <returns></returns>
    string SaveEmailAttachmentWithRetry<T>(T attachment, RefconAttachmentSummary attachmentSummary);

    /// <summary>
    /// Save a file stream to blob storage
    /// </summary>
    /// <param name="stream">data stream to save</param>
    /// <param name="summary">summary object to save blob location information</param>
    /// <param name="folder">blob folder to save the data to</param>
    /// <returns></returns>
    string SaveFile(Stream stream, IFileSummary summary, StorageFolder folder);

    /// <summary>
    /// Move a data file from one folder to another
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="fromFolder"></param>
    /// <param name="toFolder"></param>
    /// <returns></returns>
    string MoveFile(IFileSummary summary, StorageFolder fromFolder, StorageFolder toFolder);
}