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

namespace X2CodingLab.SensorTag.Sensors
{
    public class DeviceInfoService : IDisposable
    {
        private GattDeviceService deviceService;
        private bool disposed;

        public string SensorServiceUuid
        {
            get { return SensorTagUuid.UUID_INF_SERV; }
        }

        /// <summary>
        /// Retrieves the sensor device and saves it for further usage.
        /// IMPORTANT: Has to be called from UI thread the first time the app uses the device to be able to ask the user for permission to use it
        /// </summary>
        /// <returns>Indicates if the gatt service could be retrieved and set successfully</returns>
        /// <exception cref="DeviceNotFoundException">Thrown if there isn't a device which matches the sensor service id.</exception>
        public async Task<bool> Initialize()
        {
            if (this.deviceService != null)
            {
                Clean();
            }
            this.deviceService = await GattUtils.GetDeviceService(SensorTagUuid.UUID_INF_SERV);
            if (this.deviceService == null)
                return false;
            return true;
        }

        /// <summary>
        /// Retrieves the sensors GATT device service from a specified DeviceInformation and saves it for further usage.
        /// IMPORTANT: Has to be called from UI thread the first time the app uses the device to be able to ask the user for permission to use it
        /// </summary>
        /// <returns>Indicates if the gatt service could be retrieved and set successfully</returns>
        public async Task<bool> Initialize(DeviceInformation deviceInfo)
        {
            Validator.RequiresNotNull(deviceInfo);
            if (!deviceInfo.Id.Contains(SensorTagUuid.UUID_INF_SERV))
                throw new ArgumentException("Wrong DeviceInformation passed. You need to get the right DeviceInformation via DeviceInfoService.ServiceUuid.");

            if (this.deviceService != null)
            {
                Clean();
            }

            this.deviceService = await GattDeviceService.FromIdAsync(deviceInfo.Id);
            if (this.deviceService == null)
                return false;
            return true;
        }

        /// <summary>
        /// Reads the system id from the sensor tag
        /// </summary>
        /// <returns>string of system id separated by ':'</returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadSystemId()
        {
            return await ReadSystemId(":");
        }

        /// <summary>
        /// Reads the system id from the sensor tag
        /// </summary>
        /// <param name="separator">system id will be separated by this parameter</param>
        /// <returns>string of system id separated by the specified parameter</returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadSystemId(string separator)
        {
            Validator.RequiresNotNull(separator);

            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_SYSID);

            return sdata[0].ToString("X2") + separator + sdata[1].ToString("X2") + separator +
                sdata[2].ToString("X2") + separator + sdata[3].ToString("X2") + separator +
                sdata[4].ToString("X2") + separator + sdata[5].ToString("X2") + separator +
                sdata[6].ToString("X2") + separator + sdata[7].ToString("X2");
        }

        /// <summary>
        /// Reads the model number from the sensor tag
        /// </summary>
        /// <returns>model number as string</returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadModelNumber()
        {
            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_MODEL_NR);
            return ConvertToString(sdata);
        }

        /// <summary>
        /// Reads the serial number from the sensor tag
        /// </summary>
        /// <returns>serial number as string</returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadSerialNumber()
        {
            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_SERIAL_NR);
            return ConvertToString(sdata);
        }

        /// <summary>
        /// Reads the firmware revision from the sensor tag
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadFirmwareRevision()
        {
            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_FW_NR);
            return ConvertToString(sdata);
        }

        /// <summary>
        /// Reads the hardware revision from the sensor tag
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadHardwareRevision()
        {
            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_HW_NR);
            return ConvertToString(sdata);
        }

        /// <summary>
        /// Reads the software revision from the sensor tag
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadSoftwareRevision()
        {
            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_SW_NR);
            return ConvertToString(sdata);
        }

        /// <summary>
        /// Reads the manufacturer name from the sensor tag
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadManufacturerName()
        {
            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_MANUF_NR);
            return ConvertToString(sdata);
        }

        /// <summary>
        /// Reads the cert from the sensor tag
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadCert()
        {
            return await ReadCert(" ");
        }

        /// <summary>
        /// Reads the cert from the sensor tag
        /// </summary>
        /// <param name="separator">segments of the cert will be separated by this string</param>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadCert(string separator)
        {
            Validator.RequiresNotNull(separator);

            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_CERT);

            return sdata[0].ToString("X2") + separator + sdata[1].ToString("X2") + separator +
                sdata[2].ToString("X2") + separator + sdata[3].ToString("X2") + separator +
                sdata[4].ToString("X2") + separator + sdata[5].ToString("X2") + separator +
                sdata[6].ToString("X2") + separator + sdata[7].ToString("X2") + separator +
                sdata[8].ToString("X2") + separator + sdata[9].ToString("X2") + separator +
                sdata[10].ToString("X2") + separator + sdata[11].ToString("X2") + separator +
                sdata[12].ToString("X2") + separator + sdata[13].ToString("X2");
        }

        /// <summary>
        /// Reads the pnpid from the sensor tag
        /// </summary>
        /// <returns>pnpid as string</returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadPnpId()
        {
            return await ReadPnpId(" ");
        }

        /// <summary>
        /// Reads the pnpid from the sensor tag
        /// </summary>
        /// <param name="separator">segments of the pnpid will be separated by this string</param>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<string> ReadPnpId(string separator)
        {
            Validator.RequiresNotNull(separator);

            byte[] sdata = await ReadValue(SensorTagUuid.UUID_INF_PNP_ID);

            return sdata[0].ToString("X2") + separator + sdata[1].ToString("X2") + separator +
                sdata[2].ToString("X2") + separator + sdata[3].ToString("X2") + separator +
                sdata[4].ToString("X2") + separator + sdata[5].ToString("X2") + separator +
                sdata[6].ToString("X2");
        }

        /// <summary>
        /// Cleans up resources, unregisters the notification event handler
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Clean();
                }
            }

            disposed = true;
        }

        /// <summary>
        /// Reads a value from the deviceinfo server with the specified Uuid
        /// </summary>
        /// <param name="Uuid"></param>
        /// <returns>Raw sensor data as byte array</returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        private async Task<byte[]> ReadValue(string Uuid)
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            GattCharacteristic sidCharacteristic = deviceService.GetCharacteristics(new Guid(Uuid))[0];

            GattReadResult res = await sidCharacteristic.ReadValueAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (res.Status == GattCommunicationStatus.Unreachable)
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);

            byte[] data = new byte[res.Value.Length];

            DataReader.FromBuffer(res.Value).ReadBytes(data);

            return data;
        }

        /// <summary>
        /// Converts a byte array to a string with UTF-8 encoding.
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        private string ConvertToString(byte[] dataBytes)
        {
            return Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);
        }

        /// <summary>
        /// Should clean the objects resources. Disposes the deviceservice if it's not null and removes possible event handler.
        /// </summary>
        private void Clean()
        {
            if (deviceService != null)
                deviceService.Dispose();
            deviceService = null;
        }
    }
}
