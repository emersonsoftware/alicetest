namespace RefconGatewayBase.Helpers;

/// <summary>
/// This class predefines the folder names which REFCON files will move through within the same directory.
/// Use this with the IFileStorageService.
/// </summary>
public class StorageFolder
{
    private StorageFolder(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// The attachments in a REFCON email. Each attachment Id is GUID
    /// </summary>
    public static StorageFolder Attachments => new StorageFolder("Attachments/");

    /// <summary>
    /// REFCON files extracted from an Attachment archive. Each file Id is AttachmentId-FileKey
    /// </summary>
    public static StorageFolder Pending => new StorageFolder("Pending/");

    /// <summary>
    /// REFCON file mapped to UDM. Contains same file Id as the REFCON file
    /// </summary>
    public static StorageFolder Mapped => new StorageFolder("Mapped/");

    /// <summary>
    /// REFCON files which successfully completed processing
    /// </summary>
    public static StorageFolder Completed => new StorageFolder("Completed/");

    /// <summary>
    /// REFCON files or Attachment files which has error
    /// </summary>
    public static StorageFolder Error => new StorageFolder("Error/");
}