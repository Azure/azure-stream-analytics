//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace DeviceSimulator
{
    public partial class DeviceSimulator : Form
    {
        private const int MaxEvents = 100;

        private bool isRunning = false;
        private NormalEventGenerator normalEventGenerator;
        private EventGeneratorBase currentEventGenerator;
        private EventBuffer eventBuffer = new EventBuffer();
        private DataPoint currentDataPoint;
        private object dataPointsLockObject = new object();
        private System.Timers.Timer repeatAnomalyTimer = new System.Timers.Timer();

        private int sensorId = 1;
        private DeviceClient deviceClient;
        private bool mockMode = false;

        public DeviceSimulator()
        {
            InitializeComponent();

            this.mockMode = this.checkBoxMockMode.Checked;

            this.repeatAnomalyTimer.Elapsed += OnTimerEvent;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!this.UpdateEventConfig())
            {
                return;
            }

            if (!this.UpdateIoTConfig())
            {
                return;
            }

            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            this.isRunning = true;

            this.chartEventsSent.Series[0].Points.Clear();

            this.SendDeviceToCloudMessagesAsync();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            this.isRunning = false;
            this.eventBuffer.Clear();
            this.buttonStart.Enabled = true;
            this.buttonStop.Enabled = false;
        }

        private async void SendDeviceToCloudMessagesAsync()
        {
            int messageId = 1;

            while (this.isRunning)
            {
                lock (dataPointsLockObject)
                {
                    if (this.eventBuffer.Count > 0)
                    {
                        this.currentDataPoint = this.eventBuffer.Next();
                    }
                    else
                    {
                        // If no data, just send a normal data point
                        this.currentDataPoint = this.normalEventGenerator.GetDataPoint();
                    }
                }

                var messagePayload = new
                {
                    messageId = messageId++,
                    sensorId = this.sensorId,
                    temperature = this.currentDataPoint.Value
                };

                var messagePayloadString = JsonConvert.SerializeObject(messagePayload);

                // Update the textbox and chart
                this.richTextBoxDeviceMessage.Text = $"{DateTime.Now} > Sending message: {messagePayloadString}";
                this.chartEventsSent.Series[0].Points.AddY(this.currentDataPoint.Value);

                if (this.chartEventsSent.Series[0].Points.Count > MaxEvents)
                {
                    // Remove the oldest one
                    this.chartEventsSent.Series[0].Points.RemoveAt(0);
                }

                if (!this.mockMode && this.deviceClient != null)
                {
                    var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagePayloadString));
                    await this.deviceClient.SendEventAsync(message);
                }

                await Task.Delay(this.currentDataPoint.EventDeltaMillisec);
            }
        }

        private bool UpdateEventConfig()
        {
            double variationPercentage = 0;
            double meanTemp = 0;
            int messageDeltaMillisec = 1;
            bool repeatAnomaly = this.checkBoxRepeatAnomaly.Checked;
            int repeatAnomalyAfterSec = 0;

            try
            {
                variationPercentage = Convert.ToDouble(textBoxTempVariation.Text);
                if (variationPercentage < 0 || variationPercentage > 100)
                {
                    MessageBox.Show("Invalid temperature variation percentage");
                    return false;
                }

                // Use current temp as mean so that we can insert relative anomalies; but use the UI mean value if want Normal behavior
                meanTemp = (isRunning && !this.radioButtonNormal.Checked) ? this.currentDataPoint.Value : Convert.ToInt32(this.textBoxTemp.Text);

                messageDeltaMillisec = (int)Convert.ToUInt32(this.textBoxDelay.Text);
                this.sensorId = (int)Convert.ToUInt32(this.textBoxSensorId.Text);

                // Stop the current timer
                this.repeatAnomalyTimer.Stop();
                if (repeatAnomaly)
                {
                    repeatAnomalyAfterSec = Convert.ToInt32(this.textBoxRepeatAnomaly.Text);
                    if (repeatAnomalyAfterSec <= 0)
                    {
                        MessageBox.Show("Invalid duration for repeating anomaly");
                        return false;
                    }
                }

                // Always create this event generator
                this.normalEventGenerator = new NormalEventGenerator(meanTemp, variationPercentage, messageDeltaMillisec, repeatAnomaly, repeatAnomalyAfterSec);

                if (this.radioButtonBiLevelChange.Checked)
                {
                    double biLevelChangeMultiplier = Convert.ToDouble(this.textBoxBiLevelChangeMultiplier.Text);
                    int biLevelChangeDurationSec = Convert.ToInt32(this.textBoxBiLevelChangeDuration.Text);

                    if (biLevelChangeMultiplier == 0)
                    {
                        MessageBox.Show("Specify a non-zero level change multiplier");
                        return false;
                    }

                    if (biLevelChangeDurationSec <= 0)
                    {
                        MessageBox.Show("Specify a non-zero level change duration");
                        return false;
                    }

                    // If repeat, start timer that inserts elements from the current eventGenerator with an interval = anomaly duration + repeat duration
                    if (repeatAnomaly)
                    {
                        this.repeatAnomalyTimer.Interval = repeatAnomalyAfterSec * 1000 + biLevelChangeDurationSec * 1000;
                        this.repeatAnomalyTimer.Start();
                    }

                    this.currentEventGenerator = new BiLevelChangeEventGenerator(meanTemp, variationPercentage, messageDeltaMillisec, biLevelChangeMultiplier, biLevelChangeDurationSec, repeatAnomaly, repeatAnomalyAfterSec);
                }
                else if (this.radioButtonSlowTrend.Checked)
                {
                    double slowTrendChangePercent = Convert.ToDouble(this.textBoxSlowTrendPercent.Text);
                    int slowTrendDurationSec = Convert.ToInt32(this.textBoxSlowTrendDuration.Text);

                    if (slowTrendChangePercent == 0)
                    {
                        MessageBox.Show("Specify a non-zero slow trend change percent");
                        return false;
                    }

                    if (slowTrendDurationSec <= 0)
                    {
                        MessageBox.Show("Specify a non-zero slow trend duration");
                        return false;
                    }

                    // If repeat, start timer that inserts elements from the current eventGenerator with an interval = anomaly duration + repeat duration
                    if (repeatAnomaly)
                    {
                        this.repeatAnomalyTimer.Interval = repeatAnomalyAfterSec * 1000 + slowTrendDurationSec * 1000;
                        this.repeatAnomalyTimer.Start();
                    }

                    this.currentEventGenerator = new SlowTrendEventGenerator(meanTemp, variationPercentage, messageDeltaMillisec, slowTrendChangePercent, slowTrendDurationSec, repeatAnomaly, repeatAnomalyAfterSec);
                }
                else if (this.radioButtonSpikeDip.Checked)
                {
                    double spikeAndDipMultiplier = Convert.ToDouble(this.textBoxSpikeDipMultiplier.Text);
                    int spikeAndDipCount = Convert.ToInt32(this.textBoxSpikeCount.Text);

                    if (spikeAndDipCount <= 0)
                    {
                        MessageBox.Show("Specify a spike count greater than 0");
                        return false;
                    }

                    // If repeat, start timer that inserts elements from the current eventGenerator with an interval = repeat duration
                    if (repeatAnomaly)
                    {
                        this.repeatAnomalyTimer.Interval = repeatAnomalyAfterSec * 1000;
                        this.repeatAnomalyTimer.Start();
                    }

                    this.currentEventGenerator = new SpikeAndDipEventGenerator(meanTemp, variationPercentage, messageDeltaMillisec, spikeAndDipMultiplier, spikeAndDipCount, repeatAnomaly, repeatAnomalyAfterSec);
                }
                else
                {
                    // Normal time series
                    // Clear out the event buffer because we want to return normal immediately
                    lock (this.dataPointsLockObject)
                    {
                        this.eventBuffer.Clear();
                    }

                    this.currentEventGenerator = new NormalEventGenerator(meanTemp, variationPercentage, messageDeltaMillisec, repeatAnomaly, repeatAnomalyAfterSec);
                }

                lock (this.dataPointsLockObject)
                {
                    this.eventBuffer.InsertRangeAtStart(this.currentEventGenerator.GetDataPoints());
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        private void OnTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            lock (this.dataPointsLockObject)
            {
                this.eventBuffer.InsertRangeAtStart(this.currentEventGenerator.GetDataPoints());
            }
        }

        private bool UpdateIoTConfig()
        {
            if (!this.mockMode)
            {
                try
                {
                    string iotHubHostname = textBoxHostname.Text;
                    string deviceKey = textBoxDeviceKey.Text;
                    string deviceID = textBoxDeviceID.Text;

                    if (string.IsNullOrWhiteSpace(iotHubHostname) || string.IsNullOrWhiteSpace(deviceKey) || string.IsNullOrWhiteSpace(deviceID))
                    {
                        MessageBox.Show("Please specify a value for one or more IoT Hub config settings");
                        return false;
                    }

                    this.deviceClient = DeviceClient.Create(iotHubHostname, new DeviceAuthenticationWithRegistrySymmetricKey(deviceID, deviceKey));
                    this.deviceClient.OpenAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }

            return true;
        }

        private void ButtonUpdateEventConfig_Click(object sender, EventArgs e)
        {
            this.UpdateEventConfig(); 
        }

        private void ButtonUpdateDeviceConfig_Click(object sender, EventArgs e)
        {
            this.UpdateIoTConfig();
        }

        private void CheckBoxMockMode_CheckedChanged(object sender, EventArgs e)
        {
            this.mockMode = ((CheckBox)sender).Checked;
            this.panelIoTHub.Enabled = !this.mockMode;
        }

        private void CheckBoxRepeatAnomaly_CheckedChanged(object sender, EventArgs e)
        {
            this.textBoxRepeatAnomaly.Enabled = ((CheckBox)sender).Checked;
        }

        private void RadioButtonNormal_CheckedChanged(object sender, EventArgs e)
        {
            this.checkBoxRepeatAnomaly.Enabled = !((RadioButton)sender).Checked;
            this.textBoxRepeatAnomaly.Enabled = !((RadioButton)sender).Checked && this.checkBoxRepeatAnomaly.Checked;
        }
    }
}
