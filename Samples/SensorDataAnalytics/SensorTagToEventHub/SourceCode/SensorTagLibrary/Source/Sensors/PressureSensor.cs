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
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using X2CodingLab.SensorTag.Exceptions;
using X2CodingLab.Utils;

namespace X2CodingLab.SensorTag.Sensors
{
    public class PressureSensor : SensorBase
    {
        private int[] calibrationData = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Returns the calibration data read from the sensor after EnableSensor() was called. 
        /// </summary>
        public int[] CalibrationData
        {
            get { return calibrationData; }
        }

        public PressureSensor()
            : base(SensorName.PressureSensor, SensorTagUuid.UUID_BAR_SERV, SensorTagUuid.UUID_BAR_CONF, SensorTagUuid.UUID_BAR_DATA)
        {
            
        }

        /// <summary>
        /// Calculates the pressure from the raw sensor data.
        /// </summary>
        /// <param name="sensorData"></param>
        /// <param name="calibrationData"></param>
        /// <param name="calibrationDataSigned"></param>
        /// <returns>Pressure in pascal</returns>
        public static double CalculatePressure(byte[] sensorData, int[] calibrationData)
        {
            Validator.RequiresNotNull(sensorData, "sensorData");
            Validator.RequiresNotNull(calibrationData, "sensorData");
            Validator.RequiresArgument(calibrationData.Length == 8, "Calibration data doesn't have the appropriate lenth. Use PressureSensorInstance.CalibrationData.");

            //more info about the calculation:
            //http://www.epcos.com/web/generator/Web/Sections/ProductCatalog/Sensors/PressureSensors/T5400-ApplicationNote,property=Data__en.pdf;/T5400_ApplicationNote.pdf
            int t_r, p_r;	// Temperature raw value, Pressure raw value from sensor
            double t_a, S, O; 	// Temperature actual value in unit centi degrees celsius, interim values in calculation

            t_r = BitConverter.ToInt16(sensorData, 0);
            p_r = BitConverter.ToUInt16(sensorData, 2);

            t_a = (100 * (calibrationData[0] * t_r / Math.Pow(2, 8) + calibrationData[1] * Math.Pow(2, 6))) / Math.Pow(2, 16);
            S = calibrationData[2] + calibrationData[3] * t_r / Math.Pow(2, 17) + ((calibrationData[4] * t_r / Math.Pow(2, 15)) * t_r) / Math.Pow(2, 19);
            O = calibrationData[5] * Math.Pow(2, 14) + calibrationData[6] * t_r / Math.Pow(2, 3) + ((calibrationData[7] * t_r / Math.Pow(2, 15)) * t_r) / Math.Pow(2, 4);
            return (S * p_r + O) / Math.Pow(2, 14);
        }

        /// <summary>
        /// Reads the calibration values of the sensor and then enables the sensor for pressor reads
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        public override async Task EnableSensor()
        {
            await StoreAndReadCalibrationValues();
            await base.EnableSensor();
        }

        /// <summary>
        /// Makes the sensor store calibration data, reads and processes it afterwards.
        /// </summary>
        /// <returns></returns>
        private async Task StoreAndReadCalibrationValues()
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);

            GattCharacteristic tempConfig = deviceService.GetCharacteristics(new Guid(SensorConfigUuid))[0];

            byte[] confdata = new byte[] { 2 };
            GattCommunicationStatus status = await tempConfig.WriteValueAsync(confdata.AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);

            GattCharacteristic calibrationCharacteristic = deviceService.GetCharacteristics(new Guid(SensorTagUuid.UUID_BAR_CALI))[0];

            GattReadResult res = await calibrationCharacteristic.ReadValueAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (res.Status == GattCommunicationStatus.Unreachable)
                throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);

            var sdata = new byte[res.Value.Length];

            DataReader.FromBuffer(res.Value).ReadBytes(sdata);

            calibrationData[0] = BitConverter.ToUInt16(sdata, 0);
            calibrationData[1] = BitConverter.ToUInt16(sdata, 2);
            calibrationData[2] = BitConverter.ToUInt16(sdata, 4);
            calibrationData[3] = BitConverter.ToUInt16(sdata, 6);
            calibrationData[4] = BitConverter.ToInt16(sdata, 8);
            calibrationData[5] = BitConverter.ToInt16(sdata, 10);
            calibrationData[6] = BitConverter.ToInt16(sdata, 12);
            calibrationData[7] = BitConverter.ToInt16(sdata, 14);
        }
    }
}
