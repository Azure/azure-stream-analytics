using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SendDataToEHLibrary;
using System.Diagnostics;
//using System.Timers;
//using System.Timers;



namespace TISensorToEH
{
    public partial class MainForm : Form
    {

        Main cs = new Main();
        Timer timer ;
        int NumberOfBodyTagsShown = 0;
        string DisplayText = "";
        int TotalMessagesSent = 0;

        public MainForm()
        {
            InitializeComponent();

             
            txtStatus.Text = "Please ensure sensor is connected through bluetooth!";
            txtStatus.ForeColor = Color.Red;

            var appSettings = System.Configuration.ConfigurationSettings.AppSettings;

            txtSBNamespace.Text = appSettings["SBNameSpace"];
            txtEHName.Text = appSettings["EHName"];
            txtAccessPolicyName.Text = appSettings["AccessPolicyName"];
            txtAccessPolicyKey.Text = appSettings["AccessPolicyKey"];
            txtSensorName.Text = appSettings["SensorName"];

            
            timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000;             
            //aTimer = new Timer();
            //fire();

            //Utility.DeletePBIData();
        }

        
        private async void button1_Click(object sender, EventArgs e)
        {

            txtStatus.ForeColor = Color.Blue;

            this.Cursor = Cursors.WaitCursor;
            txtStatus.Text = "trying to read sensor data ....";

            SensorValues.Humidity = "30";

            try
            {
                //string sens = await cs.GetSensorData();
                SendDataToEventhub.deviceName = txtSensorName.Text;
                SendDataToEventhub.serviceNamespace = txtSBNamespace.Text;
                SendDataToEventhub.sharedAccessPolicyName = txtAccessPolicyName.Text;
                SendDataToEventhub.sharedAccessKey = txtAccessPolicyKey.Text;
                SendDataToEventhub.hubName = txtEHName.Text;

                await cs.InitializeSensor();
                button1.BackColor = Color.Green;
                this.Cursor = Cursors.Default;

            }

            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());                
                //MessageBox.Show("Not able to read the bluetooth sensor. Please ensure bluetooth sensor is paired properly and try again.", "Bluetooth Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
                //System.Windows.Forms.Application.Exit();
                txtStatus.ForeColor = Color.Red;
                button1.BackColor = Color.Blue;
                this.Cursor = Cursors.Default;
                txtStatus.Text = "Not able to read the bluetooth sensor. Please ensure bluetooth sensor is paired properly and try again.";

                return;
            }

            timer.Enabled = true;
            timer.Start();

            //timer.Enabled = true;
            //timer.Start();

            
            
        }

        async void timer_Tick(object sender, EventArgs e)
        {

            
            txtStatus.Text = SendDataToEventhub.status;

            try
            {
                //string sens = await cs.GetSensorData();
                //await cs.InitializeSensor();
                //button1.BackColor = Color.Green;
                //this.Cursor = Cursors.Default;
                //txtStatus.Text = await cs.GetSensorid();
                await cs.GetSensorDataNew ();

            }

            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());                
                //MessageBox.Show("Not able to read the bluetooth sensor. Please ensure bluetooth sensor is paired properly and try again.", "Bluetooth Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
                //System.Windows.Forms.Application.Exit();
                txtStatus.ForeColor = Color.Red;
                button1.BackColor = Color.Blue;
                this.Cursor = Cursors.Default;
                txtStatus.Text = "Not able to read the bluetooth sensor. Please ensure bluetooth sensor is paired properly and try again."+ex.ToString();
                timer.Enabled = false;
                timer.Stop();
                return;
            }


        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private async void rbStart_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked == true)
            {

                timer.Enabled = true;
                timer.Start();
            }
        }

        private void rbStop_CheckedChanged(object sender, EventArgs e)
        {
            
            if (((RadioButton)sender).Checked == true)
                timer.Stop();
        }
    }
}
