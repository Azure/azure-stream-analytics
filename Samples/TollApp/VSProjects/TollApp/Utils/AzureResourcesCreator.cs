using System;
using System.IO;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using TollApp.Configs;
using TollApp.Models;

namespace TollApp.Utils
{
    /// <summary>
    /// Create the blob container and uploads the json file.
    /// <para>Also creates the Azure Cosmos DB database</para>
    /// </summary>
    public static class AzureResourcesCreator
    {
        public static Registration[] CreateBlob()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfiguration.StorageAccountUrl);
            CloudBlobContainer container = storageAccount.CreateCloudBlobClient().GetContainerReference(CloudConfiguration.StorageAccountContainer);
            CloudBlockBlob registrationBlockBlob = container.GetBlockBlobReference(CloudConfiguration.RegistrationFileBlob);

            //Create a new container, if it does not exist
            container.CreateIfNotExists();

            using (var fileStream = File.OpenRead(@"registration.json"))
            {
                registrationBlockBlob.UploadFromStream(fileStream);
            }

            //read Registration.json from BLOB to be used for random data generator
            using (var stream = new MemoryStream())
            {
                registrationBlockBlob.DownloadToStream(stream);
                stream.Position = 0; //resetting stream's position to 0
                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var jsonStream = serializer.Deserialize(jsonTextReader);
                        return JsonConvert.DeserializeObject<Registration[]>(jsonStream.ToString());
                    }
                }
            }
        }


        public static async void CreateAzureCosmosDb()
        {
            var client = new DocumentClient(new Uri(CloudConfiguration.DocumentDbUri), CloudConfiguration.DocumentDbKey);
            await client.CreateDatabaseIfNotExistsAsync(new Database {Id = CloudConfiguration.DocumentDbDatabaseName});

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(CloudConfiguration.DocumentDbDatabaseName), new DocumentCollection {Id = CloudConfiguration.DocumentDbCollectionName});

        }
    }
}
