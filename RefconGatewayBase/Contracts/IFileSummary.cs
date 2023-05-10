using System;

namespace RefconGatewayBase.Contracts;

/// <summary>
/// Interface for REFCON File Summaries (Attachments and Files)
/// Defines the storage information of files
/// </summary>
public interface IFileSummary
{
    /// <summary>
    /// Used to identify the file for tracking and part of file naming
    /// For Attachments, it is unique GUID.
    /// For REFCON Files, it is the name of the file in the archive.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Attachment File Name
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Full path of the location of the file in blob storage
    /// </summary>
    string FileUrl { get; set; }

    /// <summary>
    /// Date the file was saved to blob
    /// </summary>
    DateTime Created { get; set; }

    /// <summary>
    /// Define consistent way to name the file in blob storage
    /// </summary>
    /// <returns></returns>
    string GetStorageFileName();
}