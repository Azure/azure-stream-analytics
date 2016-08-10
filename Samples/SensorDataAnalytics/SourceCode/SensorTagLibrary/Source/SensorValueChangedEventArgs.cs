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

namespace X2CodingLab.SensorTag
{
    public class SensorValueChangedEventArgs : EventArgs
    {
        public SensorValueChangedEventArgs(byte[] rawData, DateTimeOffset timestamp, SensorName origin)
        {
            RawData = rawData;
            Origin = origin;
            Timestamp = timestamp;
        }

        public SensorName Origin { get; private set; }

        public byte[] RawData { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }
    }
}
