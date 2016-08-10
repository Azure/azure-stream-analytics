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
using X2CodingLab.SensorTag.Exceptions;
using X2CodingLab.Utils;

namespace X2CodingLab.SensorTag.Sensors
{
    public class Gyroscope : SensorBase
    {
        private GyroscopeAxis gyroscopeAxis;

        public Gyroscope()
            : base(SensorName.Gyroscope, SensorTagUuid.UUID_GYR_SERV, SensorTagUuid.UUID_GYR_CONF, SensorTagUuid.UUID_GYR_DATA)
        {
            
        }

        /// <summary>
        /// Returns the GyroscopeAxis used to enable the sensor
        /// </summary>
        public GyroscopeAxis GyroscopeAxis
        {
            get { return gyroscopeAxis; }
        }

        /// <summary>
        /// Calculates the value of the different gyroscope axis and scales it.
        /// </summary>
        /// <param name="data">Complete array of data retrieved from the sensor</param>
        /// <param name="axis">Specifies the axis the gyroscope was configured to read</param>
        /// <returns>Array of float with values in order of the GyroscopeAxis enum</returns>
        public static float[] CalculateAxisValue(byte[] data, GyroscopeAxis axis)
        {
            switch(axis)
            {
                case GyroscopeAxis.X:
                case GyroscopeAxis.Y:
                case GyroscopeAxis.Z:
                    return new float[] { BitConverter.ToInt16(data, 0) * (500f / 65536f) };
                case GyroscopeAxis.XY:
                case GyroscopeAxis.XZ:
                case GyroscopeAxis.YZ:
                    return new float[] { BitConverter.ToInt16(data, 0) * (500f / 65536f), 
                        BitConverter.ToInt16(data, 2) * (500f / 65536f) };
                case GyroscopeAxis.XYZ:
                    return new float[] { BitConverter.ToInt16(data, 0) * (500f / 65536f), 
                        BitConverter.ToInt16(data, 2) * (500f / 65536f), 
                        BitConverter.ToInt16(data, 4) * (500f / 65536f) };
                default:
                    return new float[] { 0, 0, 0 };
            }
        }

        /// <summary>
        /// Enables the sensor with the specified axis
        /// </summary>
        /// <param name="gyroscopeAxis">axis you want to record</param>
        /// <returns></returns>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        public async Task EnableSensor(GyroscopeAxis gyroscopeAxis)
        {
            Validator.Requires<DeviceNotInitializedException>(deviceService != null);
            this.gyroscopeAxis = gyroscopeAxis;
            await base.EnableSensor(new byte[] { (byte)gyroscopeAxis });
        }

        /// <summary>
        /// Enables the sensor to read all axis
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        public override async Task EnableSensor()
        {
            await EnableSensor(GyroscopeAxis.XYZ);
        }
    }

    /// <summary>
    /// Different options you have of reading values from the sensor
    /// </summary>
    public enum GyroscopeAxis
    {
        X = 1,
        Y = 2,
        XY = 3,
        Z = 4,
        XZ = 5,
        YZ = 6,
        XYZ = 7
    }
}
