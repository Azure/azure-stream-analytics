#region License
// Copyright © X2CodingLab Sebastian Lang 2013
// <author>Sebastian Lang</author>
// <project>TI Sensor Tag Library</project>
// <website>https://sensortag.codeplex.com/</website>
// <license>See https://sensortag.codeplex.com/license </license>
#endregion License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using X2CodingLab.SensorTag.Exceptions;
using X2CodingLab.Utils;

namespace X2CodingLab.SensorTag
{
    public class GattUtils
    {
        private GattUtils()
        {

        }

        /// <summary>
        /// Retrieves a list of devices which offer the service specified with the Uuid. In case of a sensor tag service (e.g. temperature),
        /// this lists all Sensor Tags.
        /// </summary>
        /// <param name="serviceUuid">Uuid for the type of service u're looking for.</param>
        /// <returns>List of DeviceInformation</returns>
        public static async Task<List<DeviceInformation>> GetDevicesOfService(string serviceUuid)
        {
            Validator.RequiresNotNullOrEmpty(serviceUuid);

            string selector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(serviceUuid));
            var devices = await DeviceInformation.FindAllAsync(selector);
            return devices.ToList<DeviceInformation>();
        }

        /// <summary>
        /// Finds the GattDeviceService by sensorServiceUuid.
        /// IMPORTANT: Has to be called from UI thread the first time the app uses the device to be able to ask the user for permission to use it.
        /// </summary>
        /// <returns>Returns the gatt device service of the first device that supports it. Returns null if access is denied.</returns>
        /// <exception cref="DeviceNotFoundException">Thrown if there isn't a device which provides the service Uuid.</exception>
        public async static Task<GattDeviceService> GetDeviceService(string serviceUuid)
        {
            Validator.RequiresNotNullOrEmpty(serviceUuid);

            string selector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(serviceUuid));
            var devices = await DeviceInformation.FindAllAsync(selector);
            DeviceInformation di = devices.FirstOrDefault();

            if (di == null)
                throw new DeviceNotFoundException();

            return await GattDeviceService.FromIdAsync(di.Id);
        }

        /// <summary>
        /// Finds the GattDeviceService for a specified device by serviceUuid
        /// IMPORTANT: Has to be called from UI thread the first time the app uses the device to be able to ask the user for permission to use it.
        /// </summary>
        /// <returns>Returns the gatt device service of the first device that supports it. Returns null if access is denied.</returns>
        /// <exception cref="DeviceNotFoundException">Thrown if there isn't a device which provides the service Uuid.</exception>
        public async static Task<GattDeviceService> GetDeviceService(DeviceInformation device, string serviceUuid)
        {
            Validator.RequiresNotNullOrEmpty(serviceUuid, "serviceUuid");

            Validator.RequiresNotNull(device, "device");

            return await GattDeviceService.FromIdAsync(device.Id);
        }

        /// <summary>
        /// Reads a value from from a service of a device
        /// </summary>
        /// <param name="gattDeviceService">GattDeviceService of a connected bluetooth device</param>
        /// <param name="valueServiceUuid">Uuid of the characteristic you want to read from</param>
        /// <returns>Raw data read from the sensor</returns>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="Exception">Thrown on purpose if the GattDeviceService doesn't provide the specified characteristic.</exception>
        public async static Task<byte[]> ReadValue(GattDeviceService gattDeviceService, string valueServiceUuid)
        {
            Validator.RequiresNotNull(gattDeviceService, "gattDeviceService");
            Validator.RequiresNotNullOrEmpty(valueServiceUuid, "valueServiceUuid");

            IReadOnlyList<GattCharacteristic> characteristics = gattDeviceService.GetCharacteristics(new Guid(valueServiceUuid));

            if (characteristics.Count == 0)
                throw new Exception("Could not find specified characteristic.");

            GattCharacteristic sidCharacteristic = characteristics[0];

            GattReadResult res = await sidCharacteristic.ReadValueAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (res.Status == GattCommunicationStatus.Unreachable)
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);

            var data = new byte[res.Value.Length];

            DataReader.FromBuffer(res.Value).ReadBytes(data);

            return data;
        }
    }
}
