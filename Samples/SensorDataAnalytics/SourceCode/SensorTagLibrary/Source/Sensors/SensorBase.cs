#region License
// Copyright © X2Codinglab Sebastian Lang 2013
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
using System.Runtime.InteropServices.WindowsRuntime;
using X2CodingLab.SensorTag.Exceptions;
using Windows.Devices.Bluetooth;
using Windows.Storage.Streams;
using X2CodingLab.Utils;

namespace X2CodingLab.SensorTag.Sensors
{
    public abstract class SensorBase : IDisposable
    {
        public event EventHandler<SensorValueChangedEventArgs> SensorValueChanged;

        protected GattDeviceService deviceService;

        private string sensorServiceUuid;
        private string sensorConfigUuid;
        private string sensorDataUuid;

        private GattCharacteristic dataCharacteristic;

        private SensorName sensorName;

        private bool disposed;

        public SensorBase(SensorName sensorName, string sensorServiceUuid, string sensorConfigUuid, string sensorDataUuid)
        {
            this.sensorServiceUuid = sensorServiceUuid;
            this.sensorConfigUuid = sensorConfigUuid;
            this.sensorDataUuid = sensorDataUuid;
            this.sensorName = sensorName;
        }

        public string SensorServiceUuid
        {
            get { return sensorServiceUuid; }
        }

        protected string SensorConfigUuid
        {
            get { return sensorConfigUuid; }
        }

        protected string SensorDataUuid
        {
            get { return sensorDataUuid; }
        }

        /// <summary>
        /// Retrieves the sensors GATT device service from the first device which supports the service and saves it for further usage.
        /// IMPORTANT: Has to be called from UI thread the first time the app uses the device to be able to ask the user for permission to use it
        /// </summary>
        /// <returns>Indicates if the gatt service could be retrieved and set successfully</returns>
        /// <exception cref="DeviceNotFoundException">Thrown if there isn't a device which matches the sensor service id.</exception>
        public async Task<bool> Initialize()
        {
            if(this.deviceService != null)
            {
                Clean();
            }
            this.deviceService = await GetDeviceService();
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
            if (!deviceInfo.Id.Contains(SensorServiceUuid))
                throw new ArgumentException("Wrong DeviceInformation passed. You need to get the right DeviceInformation via SPECIFICSENSORCLASS.SensorServiceUuid.");

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
        /// Calls EnableSensor with data 1 to enable sensor
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if the device was found, but no communication was possible.</exception>
        public virtual async Task EnableSensor()
        {
            await EnableSensor(new byte[] { 1 });
        }

        /// <summary>
        /// Disables the sensor by writing a 0 to the config characteristic.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public virtual async Task DisableSensor()
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            GattCharacteristic configCharacteristic = deviceService.GetCharacteristics(new Guid(sensorConfigUuid))[0];

            GattCommunicationStatus status = await configCharacteristic.WriteValueAsync((new byte[] { 0 }).AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);
        }

        /// <summary>
        /// Enables data notifications from the sensor by setting the configurationDescriptorvalue to Notify.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        public virtual async Task EnableNotifications()
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            dataCharacteristic = deviceService.GetCharacteristics(new Guid(sensorDataUuid))[0];

            dataCharacteristic.ValueChanged -= dataCharacteristic_ValueChanged;
            dataCharacteristic.ValueChanged += dataCharacteristic_ValueChanged;

            GattCommunicationStatus status =
                    await dataCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);

            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);
            }
        }

        /// <summary>
        /// Disables notifications from the sensor by resetting the configuration description value
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public virtual async Task DisableNotifications()
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            dataCharacteristic = deviceService.GetCharacteristics(new Guid(sensorDataUuid))[0];

            dataCharacteristic.ValueChanged -= dataCharacteristic_ValueChanged;

            GattCommunicationStatus status =
                await dataCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.None);

            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);
            }
        }

        /// <summary>
        /// Reads the value field of the sensor data characteristics.
        /// </summary>
        /// <returns>Raw sensor data as byte array</returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        public async Task<byte[]> ReadValue()
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            if(dataCharacteristic == null)
                dataCharacteristic = deviceService.GetCharacteristics(new Guid(sensorDataUuid))[0];

            GattReadResult readResult = await dataCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

            if (readResult.Status == GattCommunicationStatus.Unreachable)
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);

            var sensorData = new byte[readResult.Value.Length];

            DataReader.FromBuffer(readResult.Value).ReadBytes(sensorData);

            return sensorData;
        }

        /// <summary>
        /// Cleans up resources, unregisters the notification event handler
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Enables the sensor by writing specified bytes to the config characteristic.
        /// </summary>
        /// <param name="sensorEnableData">bytes to enable the sensor</param>
        /// <returns></returns>
        /// <exception cref="DeviceNotInitializedException">Thrown if the object has not been successfully initialized using the initialize() method.</exception>
        /// <exception cref="DeviceUnreachableException">Thrown if the device was found, but no communication was possible.</exception>
        protected async Task EnableSensor(byte[] sensorEnableData)
        {
            Validator.Requires<ArgumentNullException>(sensorEnableData != null);

            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            GattCharacteristic configCharacteristic = deviceService.GetCharacteristics(new Guid(sensorConfigUuid))[0];

            GattCommunicationStatus status = await configCharacteristic.WriteValueAsync(sensorEnableData.AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);
        }

        protected virtual void Dispose(bool disposing)
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
        /// Finds the GattDeviceService by sensorServiceUuid.
        /// IMPORTANT: Has to be called from UI thread the first time the app uses the device to be able to ask the user for permission to use it
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceNotFoundException">Thrown if there isn't a device which matches the sensor service id.</exception>
        private async Task<GattDeviceService> GetDeviceService()
        {
            string selector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(sensorServiceUuid));
            var devices = await DeviceInformation.FindAllAsync(selector);
            DeviceInformation di = devices.FirstOrDefault();

            if (di == null)
                throw new DeviceNotFoundException();

            return await GattDeviceService.FromIdAsync(di.Id);
        }

        /// <summary>
        /// Called if the sensor sent a notification with new data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void dataCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            OnSensorValueChanged(data, args.Timestamp);
        }

        private void OnSensorValueChanged(byte[] rawData, DateTimeOffset timeStamp)
        {
            if (SensorValueChanged != null)
            {
                SensorValueChanged(this, new SensorValueChangedEventArgs(rawData, timeStamp, sensorName));
            }
        }
        
        /// <summary>
        /// Should clean the objects resources. Disposes the deviceservice if it's not null and removes possible event handler.
        /// </summary>
        private void Clean()
        {
            if (deviceService != null)
                deviceService.Dispose();
            deviceService = null;
            if (dataCharacteristic != null)
                dataCharacteristic.ValueChanged -= dataCharacteristic_ValueChanged;
        }
    }
}
