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
using X2CodingLab.Utils;

namespace X2CodingLab.SensorTag.Sensors
{
    public class IRTemperatureSensor : SensorBase
    {
        public IRTemperatureSensor()
            : base(SensorName.TemperatureSensor, SensorTagUuid.UUID_IRT_SERV, SensorTagUuid.UUID_IRT_CONF, SensorTagUuid.UUID_IRT_DATA)
        {
            
        }

        /// <summary>
        /// Calculates the ambient temperature.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="scale"></param>
        /// <returns>Ambient temperature as double</returns>
        public static double CalculateAmbientTemperature(byte[] sensorData, TemperatureScale scale)
        {
            Validator.RequiresNotNull(sensorData);
            if (scale == TemperatureScale.Celsius)
                return BitConverter.ToUInt16(sensorData, 2) / 128.0;
            else
                return (BitConverter.ToUInt16(sensorData, 2) / 128.0) * 1.8 + 32;
        }

        /// <summary>
        /// Calculates the target temperature.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double CalculateTargetTemperature(byte[] sensorData, TemperatureScale scale)
        {
            Validator.RequiresNotNull(sensorData);
            if(scale == TemperatureScale.Celsius)
                return CalculateTargetTemperature(sensorData, (BitConverter.ToUInt16(sensorData, 2) / 128.0));
            else
                return CalculateTargetTemperature(sensorData, (BitConverter.ToUInt16(sensorData, 2) / 128.0)) * 1.8 + 32;
        }

        /// <summary>
        /// Calculates the target temperature.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="ambientTemperature">calculated ambient temperature, saves another calculation of the ambient temoerature.</param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature, TemperatureScale scale)
        {
            Validator.RequiresNotNull(sensorData, "sensorData");
            Validator.RequiresNotNull(ambientTemperature, "ambientTemperature");
            Validator.RequiresArgument(!double.IsNaN(ambientTemperature), "ambientTemperature cannot be double.NaN");
            if (scale == TemperatureScale.Celsius)
                return CalculateTargetTemperature(sensorData, ambientTemperature);
            else
                return CalculateTargetTemperature(sensorData, ambientTemperature) * 1.8 + 32;
        }

        /// <summary>
        /// Calculates the target temperature of the sensor.
        /// More info about the calculation: http://www.ti.com/lit/ug/sbou107/sbou107.pdf
        /// </summary>
        /// <param name="sensorData"></param>
        /// <param name="ambientTemperature"></param>
        /// <returns></returns>
        private static double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature)
        {
            double Vobj2 = BitConverter.ToInt16(sensorData, 0);
            Vobj2 *= 0.00000015625;

            double Tdie = ambientTemperature + 273.15;

            double S0 = 5.593E-14;
            double a1 = 1.75E-3;
            double a2 = -1.678E-5;
            double b0 = -2.94E-5;
            double b1 = -5.7E-7;
            double b2 = 4.63E-9;
            double c2 = 13.4;
            double Tref = 298.15;
            double S = S0 * (1 + a1 * (Tdie - Tref) + a2 * Math.Pow((Tdie - Tref), 2));
            double Vos = b0 + b1 * (Tdie - Tref) + b2 * Math.Pow((Tdie - Tref), 2);
            double fObj = (Vobj2 - Vos) + c2 * Math.Pow((Vobj2 - Vos), 2);
            double tObj = Math.Pow(Math.Pow(Tdie, 4) + (fObj / S), .25);

            return tObj - 273.15;
        }
    }

    public enum TemperatureScale
    {
        Celsius,
        Farenheit
    }
}
