using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using X2CodingLab.SensorTag;
using X2CodingLab.SensorTag.Exceptions;
using X2CodingLab.SensorTag.Sensors;

namespace SendDataToEHLibrary
{
    public class Main
    {

        public string humidity = "30";
        
        HumiditySensor hum = new HumiditySensor();
        IRTemperatureSensor temp = new IRTemperatureSensor();


        public async Task<string> tagstext ()
        {            
            Accelerometer acc = new Accelerometer();
            List<DeviceInformation> list = await GattUtils.GetDevicesOfService(acc.SensorServiceUuid);
            //lbTags.ItemsSource = list;
            if (list != null)
            {
                return("Total: " + list.Count);
            }
            else
                return(string.Empty);

            
        }
        public async Task<string>  GetSensorid()
        {
            Exception exc = null;
            string SensorData = "";

            try
            {
                using (DeviceInfoService dis = new DeviceInfoService())
                {
                    await dis.Initialize();
                    SensorData += "System ID: " + await dis.ReadSystemId();
                    SensorData += "Model Nr: " + await dis.ReadModelNumber();
                    SensorData += "Serial Nr: " + await dis.ReadSerialNumber();
                    SensorData += "Firmware Revision: " + await dis.ReadFirmwareRevision();
                    SensorData += "Hardware Revision: " + await dis.ReadHardwareRevision();
                    SensorData += "Sofware Revision: " + await dis.ReadSoftwareRevision();
                    SensorData += "Manufacturer Name: " + await dis.ReadManufacturerName();
                    SensorData += "Cert: " + await dis.ReadCert();
                    SensorData += "PNP ID: " + await dis.ReadPnpId();
                }

                return SensorData;
            }
            catch (Exception ex)
            {
                exc = ex;
            }

            if (exc != null)
                SensorData += exc.Message;

            return SensorData;
        }

        public async void GetSensoridNew()
        {
            //Exception exc = null;
            string SensorData = "";

            using (DeviceInfoService dis = new DeviceInfoService())
                {
                    await dis.Initialize();
                    SensorData += "System ID: " + await dis.ReadSystemId();
                    SensorData += "Model Nr: " + await dis.ReadModelNumber();
                    SensorData += "Serial Nr: " + await dis.ReadSerialNumber();
                    SensorData += "Firmware Revision: " + await dis.ReadFirmwareRevision();
                    SensorData += "Hardware Revision: " + await dis.ReadHardwareRevision();
                    SensorData += "Sofware Revision: " + await dis.ReadSoftwareRevision();
                    SensorData += "Manufacturer Name: " + await dis.ReadManufacturerName();
                    SensorData += "Cert: " + await dis.ReadCert();
                    SensorData += "PNP ID: " + await dis.ReadPnpId();
                }
            
        }

        public async Task<string> InitializeSensor()
        {
            await hum.Initialize();
            await hum.EnableSensor();
            await temp.Initialize();
            await temp.EnableSensor();


            hum.SensorValueChanged += SensorValueChanged;
            temp.SensorValueChanged += SensorValueChanged;

            await hum.EnableNotifications();

            await temp.EnableNotifications();

            return ("done");
            
        }
        public async Task<string> GetSensorData()
        {

            HumiditySensor hum = new HumiditySensor();
            IRTemperatureSensor temp = new IRTemperatureSensor();

            //hum.SensorValueChanged += SensorValueChanged;
            //temp.SensorValueChanged += SensorValueChanged;

            await hum.Initialize();
            await hum.EnableSensor();
            //await hum.EnableNotifications();


            await temp.Initialize();
            await temp.EnableSensor();
            //await temp.EnableNotifications();



            hum.SensorValueChanged += SensorValueChanged;
            temp.SensorValueChanged += SensorValueChanged;

            await hum.EnableNotifications();

            await temp.EnableNotifications();
            

            return ("done");

        }


        public async Task<string> GetSensorDataNew()
        {

            byte[] x = await hum.ReadValue();
            
            SensorValues.Humidity = HumiditySensor.CalculateHumidityInPercent(x).ToString("0.00"); ;
            
            x = await temp.ReadValue();

            SensorValues.AmbientTemperature = IRTemperatureSensor.CalculateAmbientTemperature(x, TemperatureScale.Farenheit) ;
            SensorValues.TargetTemperature = IRTemperatureSensor.CalculateTargetTemperature(x, TemperatureScale.Farenheit);
                    
            return ("done");

        }


        async void SensorValueChanged(object sender, X2CodingLab.SensorTag.SensorValueChangedEventArgs e)
        {
            switch (e.Origin)
            {
                case SensorName.Accelerometer:
                    double[] accValues = Accelerometer.CalculateCoordinates(e.RawData, 1 / 64.0);
                    //tbAccelerometer.Text = "X: " + accValues[0].ToString("0.00") + " Y: " + accValues[1].ToString("0.00") + " Z: " + accValues[2].ToString("0.00");
                    break;
                case SensorName.Gyroscope:
                    float[] axisValues = Gyroscope.CalculateAxisValue(e.RawData, GyroscopeAxis.XYZ);
                    //tbGyroscope.Text = "X: " + axisValues[0].ToString("0.00") + " Y: " + axisValues[1].ToString("0.00") + " Z: " + axisValues[2].ToString("0.00");
                    break;
                case SensorName.HumiditySensor:
                    humidity = HumiditySensor.CalculateHumidityInPercent(e.RawData).ToString("0.00");
                    SensorValues.Humidity = humidity;
                    //tbHumidity.Text = humidity + "%";
                    break;
                case SensorName.TemperatureSensor:
                    double ambient = IRTemperatureSensor.CalculateAmbientTemperature(e.RawData, TemperatureScale.Farenheit);
                    double target = IRTemperatureSensor.CalculateTargetTemperature(e.RawData, ambient, TemperatureScale.Farenheit);
                    SensorValues.AmbientTemperature = ambient;
                    SensorValues.TargetTemperature = target;
                    //temp = ambient;
                    break;
            }


            try
            {
                
                //SendDataToEventhub.serviceNamespace = "tisensordemosh";
                //SendDataToEventhub.hubName = "tisensordemoeh";
                //SendDataToEventhub.sharedAccessPolicyName = "all";
                //SendDataToEventhub.sharedAccessKey = "mcxBEocF6f0KHQGhP2MtT7A44tUC+zhqyDcetP/Jt0o=";
                //SendDataToEventhub.deviceName = "sudhesh";

                string body = "";
                body = "{ \"from\":\"" + SendDataToEventhub.deviceName + "\"";
                body += ", \"dspl\":\"" + SendDataToEventhub.deviceName + "\"";
                body += ", \"time\":\"" + DateTime.UtcNow.ToString("O") + "\"";
                body += ", \"Subject\":\"wthr\"";
                if (!String.IsNullOrEmpty(SensorValues.Humidity))
                    body += ", \"hmdt\":" + SensorValues.Humidity;
                body += ", \"temp\":" + Math.Round(SensorValues.AmbientTemperature, 3);
                body += "}";
                
                
                bool code = await SendDataToEventhub.SendMessage(body);
                SendDataToEventhub.status = body;
                //txtStatus.Text = code.ToString();
                //this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.ToString());
                SendDataToEventhub.status = "error in sending data to eventhub";
            }



        }

    }
}
