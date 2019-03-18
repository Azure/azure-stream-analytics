//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace DeviceSimulator
{
    internal class SlowTrendEventGenerator : EventGeneratorBase
    {
        private double changePercent = 1.0;
        private int durationSec = 0;

        public SlowTrendEventGenerator(double meanNormalValue, double maxVariationPercent, int eventDeltaMillisec, double changePercent, int durationSec, bool repeatAnomaly, int repeatAnomalyAfterSec) : base(meanNormalValue, maxVariationPercent, eventDeltaMillisec, repeatAnomaly, repeatAnomalyAfterSec)
        {
            this.changePercent = changePercent;
            this.maxVariation = maxVariationPercent / 100 * this.meanNormalValue;
            this.durationSec = durationSec;
        }

        public override IEnumerable<DataPoint> GetDataPoints()
        {
            var values = new List<DataPoint>();
            double newMeanValue = 0;
            double newValue = 0;
            int maxCount = durationSec * 1000 / eventDeltaMillisec;
            double delta = (this.changePercent / 100 * this.meanNormalValue);
            for (int i = 1; i <= maxCount; i++)
            {
                newMeanValue = this.meanNormalValue + i * delta;
                newValue = newMeanValue - this.maxVariation + this.rng.NextDouble() * (this.maxVariation * 2);
                values.Add(new DataPoint(newValue, this.eventDeltaMillisec));
            }

            return values;
        }
    }
}
