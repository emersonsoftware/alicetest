using System.Collections.Generic;
using System.IO;

namespace RefconGatewayBase.Helpers;

/// <summary>
/// Interface to handle the REFCON archive files
/// </summary>
public interface IArchiveHandler
{
    /// <summary>
    /// Extracts the filestreams from an archive file stream
    /// </summary>
    /// <param name="fileName">Use the AttachemntSummary.Id</param>
    /// <param name="archiveStream"></param>
    /// <returns></returns>
    Dictionary<string, Stream> Extract(string fileName, Stream archiveStream);
}