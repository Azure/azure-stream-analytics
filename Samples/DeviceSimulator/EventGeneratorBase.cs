//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DeviceSimulator
{
    public abstract class EventGeneratorBase
    {
        protected readonly double meanNormalValue = 0;
        protected readonly int eventDeltaMillisec = 1000;
        protected readonly List<double> anomalousValues = new List<double>();
        protected readonly bool repeatAnomaly = false;
        protected readonly int repeatAnomalyAfterSec = 0;

        protected Random rng = new Random(DateTime.UtcNow.Ticks.GetHashCode());
        protected double maxVariation = 0;
        protected DateTime repeatAnomalyTimestamp = DateTime.MaxValue;

        public EventGeneratorBase(double meanNormalValue, double maxVariationPercent, int eventDeltaMillisec, bool repeatAnomaly, int repeatAnomalyAfterSec)
        {
            this.meanNormalValue = meanNormalValue;
            this.maxVariation = maxVariationPercent / 100 * this.meanNormalValue;
            this.eventDeltaMillisec = eventDeltaMillisec;

            this.repeatAnomaly = repeatAnomaly;
            this.repeatAnomalyAfterSec = repeatAnomalyAfterSec;
        }

        public int EventDeltaMillisec
        {
            get
            {
                return this.eventDeltaMillisec;
            }
        }

        public abstract IEnumerable<DataPoint> GetDataPoints();
    }
}
