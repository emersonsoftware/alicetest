using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RefconGatewayBase.Storage.Blobs;

public interface IBlobService
{
    Task<List<T>> GetAllAsync<T>(string containerName, string prefix = null);

    Task<T> ReadAsync<T>(string blobName);

    Task AddOrReplaceAsync<T>(T objectToStore, string containerName, string blobName);

    Task<string> StoreAsync<T>(T value, string blobName);

    Task<string> StoreFileAsync(MemoryStream _stream, string fileName, string contentType);

    Task<Stream> FetchFileAsync(string fileName, string contentType);

    Task DeleteFileAsync(string fileName);
}