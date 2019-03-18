//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace DeviceSimulator
{
    internal class BiLevelChangeEventGenerator : EventGeneratorBase
    {
        private double multiplier = 1.0;
        private double biLevelMeanValue = 0;
        private int durationSec = 0;

        public BiLevelChangeEventGenerator(double meanNormalValue, double maxVariationPercent, int eventDeltaMillisec, double multiplier, int durationSec, bool repeatAnomaly, int repeatAnomalyAfterSec) : base(meanNormalValue, maxVariationPercent, eventDeltaMillisec, repeatAnomaly, repeatAnomalyAfterSec)
        {
            this.multiplier = multiplier;
            this.biLevelMeanValue = this.meanNormalValue * this.multiplier;
            this.maxVariation = maxVariationPercent / 100 * this.biLevelMeanValue;
            this.durationSec = durationSec;
        }

        public override IEnumerable<DataPoint> GetDataPoints()
        {
            var values = new List<DataPoint>();
            double newValue = 0;
            int maxCount = durationSec * 1000 / eventDeltaMillisec;
            for (int i = 0; i < maxCount; i++)
            {
                newValue = this.biLevelMeanValue - this.maxVariation + this.rng.NextDouble() * (this.maxVariation * 2);
                values.Add(new DataPoint(newValue, this.eventDeltaMillisec));
            }

            return values;
        }
    }
}
