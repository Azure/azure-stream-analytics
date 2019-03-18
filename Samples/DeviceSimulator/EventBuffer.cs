//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceSimulator
{
    public class DataPoint
    {
        public DataPoint(double val, int eventDeltaMs)
        {
            this.Value = val;
            this.EventDeltaMillisec = eventDeltaMs;
        }

        public double Value
        {
            get;
            private set;
        }

        public int EventDeltaMillisec
        {
            get;
            private set;
        }
    }

    public class EventBuffer
    {
        private List<DataPoint> dataPoints = new List<DataPoint>();

        public int Count
        {
            get
            {
                return this.dataPoints.Count;
            }
        }

        public void InsertAtStart(DataPoint item)
        {
            this.dataPoints.Insert(0, item);
        }

        public void InsertRangeAtStart(IEnumerable<DataPoint> items)
        {
            this.dataPoints.InsertRange(0, items);
        }

        public DataPoint Next()
        {
            var dataPoint = this.dataPoints[0];
            this.dataPoints.RemoveAt(0);
            return dataPoint;
        }

        public void Clear()
        {
            this.dataPoints.Clear();
        }
    }
}
