//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DeviceSimulator
{
    internal class NormalEventGenerator : EventGeneratorBase
    {
        public NormalEventGenerator(double meanNormalValue, double maxVariationPercent, int eventDeltaMillisec, bool repeatAnomaly, int repeatAnomalyAfterSec) : base(meanNormalValue, maxVariationPercent, eventDeltaMillisec, repeatAnomaly, repeatAnomalyAfterSec)
        {
        }

        public override IEnumerable<DataPoint> GetDataPoints()
        {
            double newValue = this.meanNormalValue - this.maxVariation + this.rng.NextDouble() * (this.maxVariation * 2);
            yield return new DataPoint(newValue, this.eventDeltaMillisec);
        }

        public DataPoint GetDataPoint()
        {
            double newValue = this.meanNormalValue - this.maxVariation + this.rng.NextDouble() * (this.maxVariation * 2);
            return new DataPoint(newValue, this.eventDeltaMillisec);
        }
    }
}
