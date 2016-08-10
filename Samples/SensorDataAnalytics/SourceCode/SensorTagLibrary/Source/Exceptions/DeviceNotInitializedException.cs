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

namespace X2CodingLab.SensorTag.Exceptions
{
    /// <summary>
    /// Occuree when a sensor has not been initalized and an other operation has been executed on the sensor.
    /// </summary>
    public class DeviceNotInitializedException : Exception
    {
        public DeviceNotInitializedException()
            : base()
        {

        }

        public DeviceNotInitializedException(string message)
            : base(message)
        {

        }

        public DeviceNotInitializedException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
