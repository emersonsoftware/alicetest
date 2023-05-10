using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using RefconGatewayBase.Configuration;
using RefconGatewayBase.Logging;

namespace RefconGatewayBase.Storage.Blobs;

public class BlobService : IBlobService
{
    private readonly string thumbnailContainer = Config.ThumbnailContainer;
    private readonly BlobContainerPublicAccessType accessType;
    private readonly CloudBlobClient blobClient;
    private readonly string containerName;

    public JsonSerializerSettings serializerSettings = new();

    public BlobService(BlobContainerPublicAccessType accessType) : this(string.Empty, Config.StorageConnectionString, accessType) { }

    public BlobService(string containerName) : this(containerName, Config.StorageConnectionString) { }

    public BlobService(string containerName, string connectionString, BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Off)
    {
        this.containerName = containerName;
        this.accessType = accessType;

        // Retrieve storage account information from connection string.
        var storageAccount = CloudStorageAccount.Parse(connectionString);

        // Create a blob client for interacting with the blob service
        blobClient = storageAccount.CreateCloudBlobClient();
    }

    public async Task AddOrReplaceAsync<T>(T objectToStore, string containerName, string blobName)
    {
        var container = await GetOrCreateBlobContainerAsync(containerName);

        var blob = container.GetBlockBlobReference(blobName);
        var serializedContent = JsonConvert.SerializeObject(objectToStore, serializerSettings);
        await blob.UploadTextAsync(serializedContent);
    }

    public async Task<List<T>> GetAllAsync<T>(string containerName, string prefix = null)
    {
        var result = new List<T>();
        var container = await GetOrCreateBlobContainerAsync(containerName);

        BlobContinuationToken continuationToken = null;
        var blobs = new List<IListBlobItem>();
        do
        {
            var response = await container.ListBlobsSegmentedAsync(prefix, continuationToken);
            continuationToken = response.ContinuationToken;
            blobs.AddRange(response.Results);
        }
        while (continuationToken != null);

        foreach (var blob in blobs)
        {
            string blobText = null;
            try
            {
                var blobReference = container.GetBlockBlobReference(GetBlobName(blob));
                blobText = await blobReference.DownloadTextAsync();

                result.Add(JsonConvert.DeserializeObject<T>(blobText, serializerSettings));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to download blob {blob.StorageUri.PrimaryUri}", ex);
                Console.Write(ex.ToString());
            }
        }

        return result;
    }

    public async Task<string> StoreAsync<T>(T value, string blobName)
    {
        var container = await GetOrCreateBlobContainerAsync(containerName);

        var blob = container.GetBlockBlobReference(blobName);

        var serializedContent = JsonConvert.SerializeObject(value);

        await blob.UploadTextAsync(serializedContent);

        return blob.Uri.AbsoluteUri;
    }

    public async Task<T> ReadAsync<T>(string blobName)
    {
        T result;

        var container = await GetOrCreateBlobContainerAsync(containerName);

        var blob = container.GetBlockBlobReference(blobName);

        using (var reader = new StreamReader(await blob.OpenReadAsync()))
        {
            var stringContent = reader.ReadToEnd();
            result = JsonConvert.DeserializeObject<T>(stringContent);
        }

        return result;
    }

    /// <summary>
    /// To upload the blob into from Stream into a specified container
    /// </summary>
    /// <param name="_stream"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public async Task<string> StoreFileAsync(MemoryStream _stream, string fileName, string contentType)
    {
        var blobContainer = await GetOrCreateBlobContainerAsync(containerName);
        var blockBlob = blobContainer.GetBlockBlobReference(fileName);

        blockBlob.Properties.ContentType = contentType;
        await blockBlob.UploadFromStreamAsync(_stream);

        return blockBlob.Uri.AbsoluteUri;
    }

    /// <summary>
    /// Fetch the blob into from Stream into a specified container
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public async Task<Stream> FetchFileAsync(string fileName, string contentType)
    {
        Stream stream = new MemoryStream();
        var blobContainer = await GetOrCreateBlobContainerAsync(containerName);
        var blockBlob = blobContainer.GetBlockBlobReference(fileName);

        blockBlob.Properties.ContentType = contentType;
        await blockBlob.DownloadToStreamAsync(stream);

        return stream;
    }

    /// <summary>
    /// Delete the file based on fileName.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task DeleteFileAsync(string fileName)
    {
        try
        {
            var blobContainer = await GetOrCreateBlobContainerAsync(containerName);
            var blockBlob = blobContainer.GetBlockBlobReference(fileName);

            await blockBlob.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to delete file {fileName} in container {containerName}.", ex);
            throw;
        }
    }

    public async Task DeleteAsync(string containerName, string blobName)
    {
        var container = await GetOrCreateBlobContainerAsync(containerName);

        var blob = container.GetBlockBlobReference(blobName);
        await blob.DeleteIfExistsAsync();
    }

    public async Task MoveBlobInsideContainerAsync(string containerName, string sourceName, string destinationName)
    {
        var container = await GetOrCreateBlobContainerAsync(containerName);

        var sourceBlob = container.GetBlockBlobReference(sourceName);
        var destinationBlob = container.GetBlockBlobReference(destinationName);

        await destinationBlob.StartCopyAsync(sourceBlob);
        await sourceBlob.DeleteAsync();
    }

    #region Private

    private static string GetBlobName(IListBlobItem blobItem)
    {
        var blobName = string.Empty;
        if (blobItem is CloudBlockBlob) { blobName = ((CloudBlockBlob)blobItem).Name; }
        else if (blobItem is CloudPageBlob) { blobName = ((CloudPageBlob)blobItem).Name; }
        else if (blobItem is CloudBlobDirectory) { blobName = ((CloudBlobDirectory)blobItem).Uri.ToString(); }

        return blobName;
    }

    private async Task<CloudBlobContainer> GetOrCreateBlobContainerAsync(string name)
    {
        if (string.IsNullOrEmpty(name)) { throw new Exception("containerName cannot be empty!"); }

        var container = blobClient.GetContainerReference(name.ToLowerInvariant());
        if (await container.CreateIfNotExistsAsync())
        {
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = accessType
            };
            await container.SetPermissionsAsync(permissions);
            Log.Info(string.Format("Container {0} was created successfully", container.Name));
        }

        return container;
    }

    #endregion Private
}