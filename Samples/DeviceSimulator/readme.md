The DeviceSimulator is used to simulate anomalies being sent from a device to IoT Hub. The schema uses temperature and a sensor ID. These events can then be consumed by an Azure Stream Analytics job configured to read from this IoT Hub.

Event Configuration:

Anomaly Settings:

Set the type of events to send at an interval specified by "Delay between messages (ms)", specified in milliseconds.

Normal Events - Send normal events centered around the "Mean normal temperature" with a variation of "Max temp % variation" specified as a a  percentage.
Spike/Dip - Use a positive multiplier for a spike and a negative multiplier for a dip. This factor is multiplied with the "Mean normal temperature". Use Count to specified how many back-to-back spikes or dips to send.
Level change - The mean value of events changes to the mean multiplied by the Multiplier factor (positive or negative) and for time specified by Duration (sec).
Slow trend - The mean value of events starts rising or declining by the percentage specified over the course of the specified Duration (sec).

Note that the anomaly settings are "composable". For eg, while an anomaly is in play for a certain duration, if a different anomaly setting is chosen for a certain amount of time, once the second anomaly finishes, the first anomaly is resumed. 

Repeat anomaly after seconds - Used to simulate a periodic anomaly after set number of seconds.

Sensor ID: Set the sensor ID sent in each event. Use multiple instances of this simulator to use different sensor IDs.

Update Event Config: Press this button to put the new event settings into effect.

IoT Hub config:

Configure the target for the events.

Mock mode: Use this to experiment with the simulator and anomaly patterns without actually sending anything to a real IoT Hub.

Iot Hub Hostname: Obtain this value from your Iot Hub Azure Portal.
Device ID: Configure a device in your Iot Hub Azure Portal and copy the name here.
Device Key: Copy the device key from your Iot Hub Azure Portal.
Update IoT Hub Config: Press this button to put the new IoT Hub settings into effect.

Start: Press this button to start sending events with the specified configuration to the specified target.
Stop: Press this button to stop sending events.

Known bugs:

The GUI does not scale appropriately when the event value jumps off scale suddenly. Pressing Stop and then Start fixes it.
