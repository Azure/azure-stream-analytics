using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using TollApp.Configs;
using TollApp.Events;
using TollApp.Models;
using TollApp.Senders;

namespace TollApp.Tests
{
    [TestClass]
    public class TollAppTests
    {
        private TollDataGenerator _tollDataGenerator;
        private Registration[] _commercialVehicleRegistration;

        [TestInitialize]
        public void Setup()
        {
            _commercialVehicleRegistration = new Registration[5];
            _commercialVehicleRegistration[0] = new Registration
            {
                LicensePlate = "SVT 6023",
                RegistrationId = "285429838",
                Expired = 1
            };
            _commercialVehicleRegistration[1] = new Registration
            {
                LicensePlate = "XLZ 3463",
                RegistrationId = "362715656",
                Expired = 0
            };
            _commercialVehicleRegistration[2] = new Registration
            {
                LicensePlate = "BAC 1005",
                RegistrationId = "876133137",
                Expired = 0
            };
            _commercialVehicleRegistration[3] = new Registration
            {
                LicensePlate = "RIV 8632",
                RegistrationId = "992711956",
                Expired = 0
            };
            _commercialVehicleRegistration[4] = new Registration
            {
                LicensePlate = "SNY 7188",
                RegistrationId = "592133890",
                Expired = 0
            };

            _tollDataGenerator = TollDataGenerator.Generator(_commercialVehicleRegistration);
        }


        [TestMethod]
        public void GenerateEventDataTest()
        {
            var startTime = DateTime.UtcNow;
            var interval = TimeSpan.FromSeconds(Convert.ToDouble(1));
            _tollDataGenerator.Next(startTime, interval, 5);

            var result = _tollDataGenerator.GetEvents(startTime);

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void CopyBlobTest()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfiguration.StorageAccountUrl);
            CloudBlobContainer container = storageAccount.CreateCloudBlobClient().GetContainerReference(CloudConfiguration.StorageAccountContainer);
            CloudBlockBlob registrationBlockBlob = container.GetBlockBlobReference(CloudConfiguration.RegistrationFileBlob);
            container.CreateIfNotExists();

            //Start host in separate thread
            var thread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var host = new JobHost();
                host.RunAndBlock();
            });

            thread.Start();

            using (var fileStream = File.OpenRead(@"Data\Registration.json"))
            {
                registrationBlockBlob.UploadFromStream(fileStream);
            }

            Registration[] result;
            using (var stream1 = new MemoryStream())
            {
                registrationBlockBlob.DownloadToStream(stream1);
                stream1.Position = 0; //resetting stream's position to 0
                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(stream1))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var jsonStream = serializer.Deserialize(jsonTextReader);
                        result = JsonConvert.DeserializeObject<Registration[]>(jsonStream.ToString());
                    }
                }
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count());

            //delete container after testing
            container.Delete();
        }



        [TestMethod]
        public void SendDataTest()
        {
            var eventHubSender = new EventHubSender();
            var startTime = DateTime.UtcNow;
            _tollDataGenerator.Next(startTime, TimeSpan.FromSeconds(Convert.ToDouble(1)), 10);

            foreach (var tollEvent in _tollDataGenerator.GetEvents(startTime))
            {
                eventHubSender.SendData(tollEvent);
            }
        }
    }
}

