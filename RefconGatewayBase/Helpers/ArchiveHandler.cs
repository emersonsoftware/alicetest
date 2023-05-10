using System;
using System.Collections.Generic;
using System.IO;
using RefconGatewayBase.Logging;
using SharpCompress.Archives;

namespace RefconGatewayBase.Helpers;

/// <summary>
/// Uses SharpCompress to handle archive files
/// </summary>
public class ArchiveHandler : IArchiveHandler
{
    /// <summary>
    /// Uses SharpCompress.ArchiveFactory to extract files within a zipped archive file
    /// We only expect one file, but return Dictionary in case there are multiple files in the zip
    /// </summary>
    /// <param name="fileName">name to use for the temporary local file</param>
    /// <param name="archiveStream">attachment stream to save</param>
    /// <returns></returns>
    public Dictionary<string, Stream> Extract(string fileName, Stream archiveStream)
    {
        var files = new Dictionary<string, Stream>();

        try
        {
            var tempFilePath = Path.GetTempPath() + fileName;

            SaveZip(archiveStream, tempFilePath);

            using (var reader = ArchiveFactory.Open(tempFilePath))
            {
                foreach (var entry in reader.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        using (var stream = new MemoryStream())
                        {
                            entry.WriteTo(stream);
                            files.TryAdd(entry.Key, stream);
                        }
                    }
                }
            }

            DeleteZip(tempFilePath);
        }
        catch (Exception ex) { Log.Error($"Error extracting file for {fileName}", ex); }

        return files;
    }

    #region Private Methods

    /// <summary>
    /// Private methods to save the zip stream to the local directory
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="tempFilePath"></param>
    private void SaveZip(Stream stream, string tempFilePath)
    {
        try
        {
            using (var ms = (MemoryStream)stream)
            {
                var file = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write);
                ms.WriteTo(file);
                file.Close();
            }
        }
        catch (Exception ex) { Log.Error($"Unable to save attachment to local {tempFilePath}", ex); }
    }

    /// <summary>
    /// Private methods to delete the zip file from the local directory
    /// </summary>
    /// <param name="tempFilePath"></param>
    private void DeleteZip(string tempFilePath)
    {
        try { File.Delete(tempFilePath); }
        catch (Exception ex) { Log.Warning($"Unable to delete file {tempFilePath}", ex); }
    }

    #endregion Private Methods
}