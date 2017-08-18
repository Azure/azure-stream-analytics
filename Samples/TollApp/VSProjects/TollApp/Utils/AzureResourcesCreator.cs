using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using TollApp.Configs;
using TollApp.Models;

namespace TollApp.Utils
{
    public static class AzureResourcesCreator
    {
        public static Registration[] CreateBlob()
        {
            try
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
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async void CreateAzureCosmosDb()
        {
            try
            {
                var client = new DocumentClient(new Uri(CloudConfiguration.DocumentDbUri), CloudConfiguration.DocumentDbKey);
                await client.CreateDatabaseIfNotExistsAsync(new Database {Id = CloudConfiguration.DocumentDbDatabaseName});

                await client.CreateDocumentCollectionIfNotExistsAsync( UriFactory.CreateDatabaseUri(CloudConfiguration.DocumentDbDatabaseName), new DocumentCollection {Id = CloudConfiguration.DocumentDbCollectionName});
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}
