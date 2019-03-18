//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace DeviceSimulator
{
    internal class SpikeAndDipEventGenerator : EventGeneratorBase
    {
        private double multiplier = 1.0;
        private double spikeMeanValue = 0;
        private int maxCount = 0;

        public SpikeAndDipEventGenerator(double meanNormalValue, double maxVariationPercent, int eventDeltaMillisec, double multiplier, int maxCount, bool repeatAnomaly, int repeatAnomalyAfterSec) : base(meanNormalValue, maxVariationPercent, eventDeltaMillisec, repeatAnomaly, repeatAnomalyAfterSec)
        {
            this.multiplier = multiplier;
            this.spikeMeanValue = this.meanNormalValue * this.multiplier;
            this.maxVariation = maxVariationPercent / 100 * this.spikeMeanValue;
            this.maxCount = maxCount;
        }

        public override IEnumerable<DataPoint> GetDataPoints()
        {
            var values = new List<DataPoint>();
            double newValue = 0;
            for (int i = 0; i < this.maxCount; i++)
            {
                newValue = this.spikeMeanValue - this.maxVariation + this.rng.NextDouble() * (this.maxVariation * 2);
                values.Add(new DataPoint(newValue, this.eventDeltaMillisec));
            }

            return values;
        }
    }
}
