//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace DeviceSimulator
{
    partial class DeviceSimulator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBoxEventConfig = new System.Windows.Forms.GroupBox();
            this.labelTempVariationPercent = new System.Windows.Forms.Label();
            this.textBoxSensorId = new System.Windows.Forms.TextBox();
            this.labelSensorId = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBoxAnomalyConfig = new System.Windows.Forms.GroupBox();
            this.textBoxSpikeCount = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.labelSpikeDipX = new System.Windows.Forms.Label();
            this.labelLevelChangeX = new System.Windows.Forms.Label();
            this.labelSlowTrendPercent = new System.Windows.Forms.Label();
            this.textBoxRepeatAnomaly = new System.Windows.Forms.TextBox();
            this.textBoxBiLevelChangeDuration = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.textBoxSlowTrendDuration = new System.Windows.Forms.TextBox();
            this.labelSlowTrendDuration = new System.Windows.Forms.Label();
            this.checkBoxRepeatAnomaly = new System.Windows.Forms.CheckBox();
            this.labelSlowTrendPercentage = new System.Windows.Forms.Label();
            this.textBoxSlowTrendPercent = new System.Windows.Forms.TextBox();
            this.labelLevelChangeMultiplier = new System.Windows.Forms.Label();
            this.textBoxBiLevelChangeMultiplier = new System.Windows.Forms.TextBox();
            this.labelSpikeDipMultiplier = new System.Windows.Forms.Label();
            this.textBoxSpikeDipMultiplier = new System.Windows.Forms.TextBox();
            this.radioButtonSlowTrend = new System.Windows.Forms.RadioButton();
            this.radioButtonBiLevelChange = new System.Windows.Forms.RadioButton();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.radioButtonSpikeDip = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDelay = new System.Windows.Forms.TextBox();
            this.textBoxTemp = new System.Windows.Forms.TextBox();
            this.textBoxTempVariation = new System.Windows.Forms.TextBox();
            this.buttonUpdateEventConfig = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBoxIoTHubConfig = new System.Windows.Forms.GroupBox();
            this.panelIoTHub = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonUpdateDeviceConfig = new System.Windows.Forms.Button();
            this.textBoxDeviceKey = new System.Windows.Forms.TextBox();
            this.textBoxHostname = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxDeviceID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxMockMode = new System.Windows.Forms.CheckBox();
            this.chartEventsSent = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.richTextBoxDeviceMessage = new System.Windows.Forms.RichTextBox();
            this.Tabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBoxEventConfig.SuspendLayout();
            this.groupBoxAnomalyConfig.SuspendLayout();
            this.groupBoxIoTHubConfig.SuspendLayout();
            this.panelIoTHub.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartEventsSent)).BeginInit();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.tabPage1);
            this.Tabs.Location = new System.Drawing.Point(11, 11);
            this.Tabs.Margin = new System.Windows.Forms.Padding(2);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(1200, 266);
            this.Tabs.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBoxEventConfig);
            this.tabPage1.Controls.Add(this.groupBoxIoTHubConfig);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(1192, 240);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Device";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBoxEventConfig
            // 
            this.groupBoxEventConfig.Controls.Add(this.labelTempVariationPercent);
            this.groupBoxEventConfig.Controls.Add(this.textBoxSensorId);
            this.groupBoxEventConfig.Controls.Add(this.labelSensorId);
            this.groupBoxEventConfig.Controls.Add(this.label9);
            this.groupBoxEventConfig.Controls.Add(this.groupBoxAnomalyConfig);
            this.groupBoxEventConfig.Controls.Add(this.label2);
            this.groupBoxEventConfig.Controls.Add(this.textBoxDelay);
            this.groupBoxEventConfig.Controls.Add(this.textBoxTemp);
            this.groupBoxEventConfig.Controls.Add(this.textBoxTempVariation);
            this.groupBoxEventConfig.Controls.Add(this.buttonUpdateEventConfig);
            this.groupBoxEventConfig.Controls.Add(this.label3);
            this.groupBoxEventConfig.Location = new System.Drawing.Point(5, 24);
            this.groupBoxEventConfig.Name = "groupBoxEventConfig";
            this.groupBoxEventConfig.Size = new System.Drawing.Size(734, 209);
            this.groupBoxEventConfig.TabIndex = 15;
            this.groupBoxEventConfig.TabStop = false;
            this.groupBoxEventConfig.Text = "Event configuration";
            // 
            // labelTempVariationPercent
            // 
            this.labelTempVariationPercent.AutoSize = true;
            this.labelTempVariationPercent.Location = new System.Drawing.Point(704, 84);
            this.labelTempVariationPercent.Name = "labelTempVariationPercent";
            this.labelTempVariationPercent.Size = new System.Drawing.Size(15, 13);
            this.labelTempVariationPercent.TabIndex = 45;
            this.labelTempVariationPercent.Text = "%";
            // 
            // textBoxSensorId
            // 
            this.textBoxSensorId.Location = new System.Drawing.Point(633, 134);
            this.textBoxSensorId.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSensorId.Name = "textBoxSensorId";
            this.textBoxSensorId.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxSensorId.Size = new System.Drawing.Size(68, 20);
            this.textBoxSensorId.TabIndex = 16;
            this.textBoxSensorId.Text = "1";
            // 
            // labelSensorId
            // 
            this.labelSensorId.AutoSize = true;
            this.labelSensorId.Location = new System.Drawing.Point(474, 137);
            this.labelSensorId.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSensorId.Name = "labelSensorId";
            this.labelSensorId.Size = new System.Drawing.Size(54, 13);
            this.labelSensorId.TabIndex = 15;
            this.labelSensorId.Text = "Sensor ID";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(474, 57);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(127, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Mean normal temperature";
            // 
            // groupBoxAnomalyConfig
            // 
            this.groupBoxAnomalyConfig.Controls.Add(this.textBoxSpikeCount);
            this.groupBoxAnomalyConfig.Controls.Add(this.label20);
            this.groupBoxAnomalyConfig.Controls.Add(this.labelSpikeDipX);
            this.groupBoxAnomalyConfig.Controls.Add(this.labelLevelChangeX);
            this.groupBoxAnomalyConfig.Controls.Add(this.labelSlowTrendPercent);
            this.groupBoxAnomalyConfig.Controls.Add(this.textBoxRepeatAnomaly);
            this.groupBoxAnomalyConfig.Controls.Add(this.textBoxBiLevelChangeDuration);
            this.groupBoxAnomalyConfig.Controls.Add(this.label14);
            this.groupBoxAnomalyConfig.Controls.Add(this.textBoxSlowTrendDuration);
            this.groupBoxAnomalyConfig.Controls.Add(this.labelSlowTrendDuration);
            this.groupBoxAnomalyConfig.Controls.Add(this.checkBoxRepeatAnomaly);
            this.groupBoxAnomalyConfig.Controls.Add(this.labelSlowTrendPercentage);
            this.groupBoxAnomalyConfig.Controls.Add(this.textBoxSlowTrendPercent);
            this.groupBoxAnomalyConfig.Controls.Add(this.labelLevelChangeMultiplier);
            this.groupBoxAnomalyConfig.Controls.Add(this.textBoxBiLevelChangeMultiplier);
            this.groupBoxAnomalyConfig.Controls.Add(this.labelSpikeDipMultiplier);
            this.groupBoxAnomalyConfig.Controls.Add(this.textBoxSpikeDipMultiplier);
            this.groupBoxAnomalyConfig.Controls.Add(this.radioButtonSlowTrend);
            this.groupBoxAnomalyConfig.Controls.Add(this.radioButtonBiLevelChange);
            this.groupBoxAnomalyConfig.Controls.Add(this.radioButtonNormal);
            this.groupBoxAnomalyConfig.Controls.Add(this.radioButtonSpikeDip);
            this.groupBoxAnomalyConfig.Location = new System.Drawing.Point(6, 36);
            this.groupBoxAnomalyConfig.Name = "groupBoxAnomalyConfig";
            this.groupBoxAnomalyConfig.Size = new System.Drawing.Size(463, 164);
            this.groupBoxAnomalyConfig.TabIndex = 14;
            this.groupBoxAnomalyConfig.TabStop = false;
            this.groupBoxAnomalyConfig.Text = "Anomaly settings";
            // 
            // textBoxSpikeCount
            // 
            this.textBoxSpikeCount.Location = new System.Drawing.Point(389, 44);
            this.textBoxSpikeCount.Name = "textBoxSpikeCount";
            this.textBoxSpikeCount.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxSpikeCount.Size = new System.Drawing.Size(68, 20);
            this.textBoxSpikeCount.TabIndex = 44;
            this.textBoxSpikeCount.Text = "1";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(306, 46);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(35, 13);
            this.label20.TabIndex = 43;
            this.label20.Text = "Count";
            // 
            // labelSpikeDipX
            // 
            this.labelSpikeDipX.AutoSize = true;
            this.labelSpikeDipX.Location = new System.Drawing.Point(277, 48);
            this.labelSpikeDipX.Name = "labelSpikeDipX";
            this.labelSpikeDipX.Size = new System.Drawing.Size(12, 13);
            this.labelSpikeDipX.TabIndex = 42;
            this.labelSpikeDipX.Text = "x";
            // 
            // labelLevelChangeX
            // 
            this.labelLevelChangeX.AutoSize = true;
            this.labelLevelChangeX.Location = new System.Drawing.Point(277, 73);
            this.labelLevelChangeX.Name = "labelLevelChangeX";
            this.labelLevelChangeX.Size = new System.Drawing.Size(12, 13);
            this.labelLevelChangeX.TabIndex = 40;
            this.labelLevelChangeX.Text = "x";
            // 
            // labelSlowTrendPercent
            // 
            this.labelSlowTrendPercent.AutoSize = true;
            this.labelSlowTrendPercent.Location = new System.Drawing.Point(277, 98);
            this.labelSlowTrendPercent.Name = "labelSlowTrendPercent";
            this.labelSlowTrendPercent.Size = new System.Drawing.Size(15, 13);
            this.labelSlowTrendPercent.TabIndex = 38;
            this.labelSlowTrendPercent.Text = "%";
            // 
            // textBoxRepeatAnomaly
            // 
            this.textBoxRepeatAnomaly.Enabled = false;
            this.textBoxRepeatAnomaly.Location = new System.Drawing.Point(182, 133);
            this.textBoxRepeatAnomaly.Name = "textBoxRepeatAnomaly";
            this.textBoxRepeatAnomaly.Size = new System.Drawing.Size(100, 20);
            this.textBoxRepeatAnomaly.TabIndex = 37;
            // 
            // textBoxBiLevelChangeDuration
            // 
            this.textBoxBiLevelChangeDuration.Location = new System.Drawing.Point(389, 69);
            this.textBoxBiLevelChangeDuration.Name = "textBoxBiLevelChangeDuration";
            this.textBoxBiLevelChangeDuration.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxBiLevelChangeDuration.Size = new System.Drawing.Size(68, 20);
            this.textBoxBiLevelChangeDuration.TabIndex = 36;
            this.textBoxBiLevelChangeDuration.Text = "60";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(306, 71);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(73, 13);
            this.label14.TabIndex = 35;
            this.label14.Text = "Duration (sec)";
            // 
            // textBoxSlowTrendDuration
            // 
            this.textBoxSlowTrendDuration.Location = new System.Drawing.Point(389, 94);
            this.textBoxSlowTrendDuration.Name = "textBoxSlowTrendDuration";
            this.textBoxSlowTrendDuration.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxSlowTrendDuration.Size = new System.Drawing.Size(68, 20);
            this.textBoxSlowTrendDuration.TabIndex = 32;
            this.textBoxSlowTrendDuration.Text = "60";
            // 
            // labelSlowTrendDuration
            // 
            this.labelSlowTrendDuration.AutoSize = true;
            this.labelSlowTrendDuration.Location = new System.Drawing.Point(306, 96);
            this.labelSlowTrendDuration.Name = "labelSlowTrendDuration";
            this.labelSlowTrendDuration.Size = new System.Drawing.Size(73, 13);
            this.labelSlowTrendDuration.TabIndex = 31;
            this.labelSlowTrendDuration.Text = "Duration (sec)";
            // 
            // checkBoxRepeatAnomaly
            // 
            this.checkBoxRepeatAnomaly.AutoSize = true;
            this.checkBoxRepeatAnomaly.Enabled = false;
            this.checkBoxRepeatAnomaly.Location = new System.Drawing.Point(9, 135);
            this.checkBoxRepeatAnomaly.Name = "checkBoxRepeatAnomaly";
            this.checkBoxRepeatAnomaly.Size = new System.Drawing.Size(170, 17);
            this.checkBoxRepeatAnomaly.TabIndex = 30;
            this.checkBoxRepeatAnomaly.Text = "Repeat anomaly after seconds";
            this.checkBoxRepeatAnomaly.UseVisualStyleBackColor = true;
            this.checkBoxRepeatAnomaly.CheckedChanged += new System.EventHandler(this.CheckBoxRepeatAnomaly_CheckedChanged);
            // 
            // labelSlowTrendPercentage
            // 
            this.labelSlowTrendPercentage.AutoSize = true;
            this.labelSlowTrendPercentage.Location = new System.Drawing.Point(109, 96);
            this.labelSlowTrendPercentage.Name = "labelSlowTrendPercentage";
            this.labelSlowTrendPercentage.Size = new System.Drawing.Size(77, 13);
            this.labelSlowTrendPercentage.TabIndex = 26;
            this.labelSlowTrendPercentage.Text = "% change (+/-)";
            // 
            // textBoxSlowTrendPercent
            // 
            this.textBoxSlowTrendPercent.Location = new System.Drawing.Point(207, 94);
            this.textBoxSlowTrendPercent.Name = "textBoxSlowTrendPercent";
            this.textBoxSlowTrendPercent.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxSlowTrendPercent.Size = new System.Drawing.Size(68, 20);
            this.textBoxSlowTrendPercent.TabIndex = 25;
            this.textBoxSlowTrendPercent.Text = "6";
            // 
            // labelLevelChangeMultiplier
            // 
            this.labelLevelChangeMultiplier.AutoSize = true;
            this.labelLevelChangeMultiplier.Location = new System.Drawing.Point(109, 71);
            this.labelLevelChangeMultiplier.Name = "labelLevelChangeMultiplier";
            this.labelLevelChangeMultiplier.Size = new System.Drawing.Size(71, 13);
            this.labelLevelChangeMultiplier.TabIndex = 24;
            this.labelLevelChangeMultiplier.Text = "Multiplier (+/-)";
            // 
            // textBoxBiLevelChangeMultiplier
            // 
            this.textBoxBiLevelChangeMultiplier.Location = new System.Drawing.Point(207, 69);
            this.textBoxBiLevelChangeMultiplier.Name = "textBoxBiLevelChangeMultiplier";
            this.textBoxBiLevelChangeMultiplier.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxBiLevelChangeMultiplier.Size = new System.Drawing.Size(68, 20);
            this.textBoxBiLevelChangeMultiplier.TabIndex = 23;
            this.textBoxBiLevelChangeMultiplier.Text = "5";
            // 
            // labelSpikeDipMultiplier
            // 
            this.labelSpikeDipMultiplier.AutoSize = true;
            this.labelSpikeDipMultiplier.Location = new System.Drawing.Point(109, 46);
            this.labelSpikeDipMultiplier.Name = "labelSpikeDipMultiplier";
            this.labelSpikeDipMultiplier.Size = new System.Drawing.Size(71, 13);
            this.labelSpikeDipMultiplier.TabIndex = 21;
            this.labelSpikeDipMultiplier.Text = "Multiplier (+/-)";
            // 
            // textBoxSpikeDipMultiplier
            // 
            this.textBoxSpikeDipMultiplier.Location = new System.Drawing.Point(207, 44);
            this.textBoxSpikeDipMultiplier.Name = "textBoxSpikeDipMultiplier";
            this.textBoxSpikeDipMultiplier.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxSpikeDipMultiplier.Size = new System.Drawing.Size(68, 20);
            this.textBoxSpikeDipMultiplier.TabIndex = 18;
            this.textBoxSpikeDipMultiplier.Text = "4";
            // 
            // radioButtonSlowTrend
            // 
            this.radioButtonSlowTrend.AutoSize = true;
            this.radioButtonSlowTrend.Location = new System.Drawing.Point(9, 94);
            this.radioButtonSlowTrend.Name = "radioButtonSlowTrend";
            this.radioButtonSlowTrend.Size = new System.Drawing.Size(75, 17);
            this.radioButtonSlowTrend.TabIndex = 16;
            this.radioButtonSlowTrend.Text = "Slow trend";
            this.radioButtonSlowTrend.UseVisualStyleBackColor = true;
            // 
            // radioButtonBiLevelChange
            // 
            this.radioButtonBiLevelChange.AutoSize = true;
            this.radioButtonBiLevelChange.Location = new System.Drawing.Point(9, 69);
            this.radioButtonBiLevelChange.Name = "radioButtonBiLevelChange";
            this.radioButtonBiLevelChange.Size = new System.Drawing.Size(90, 17);
            this.radioButtonBiLevelChange.TabIndex = 15;
            this.radioButtonBiLevelChange.Text = "Level change";
            this.radioButtonBiLevelChange.UseVisualStyleBackColor = true;
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(9, 21);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(93, 17);
            this.radioButtonNormal.TabIndex = 12;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal events";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            this.radioButtonNormal.CheckedChanged += new System.EventHandler(this.RadioButtonNormal_CheckedChanged);
            // 
            // radioButtonSpikeDip
            // 
            this.radioButtonSpikeDip.AutoSize = true;
            this.radioButtonSpikeDip.Location = new System.Drawing.Point(9, 44);
            this.radioButtonSpikeDip.Name = "radioButtonSpikeDip";
            this.radioButtonSpikeDip.Size = new System.Drawing.Size(73, 17);
            this.radioButtonSpikeDip.TabIndex = 13;
            this.radioButtonSpikeDip.Text = "Spike/Dip";
            this.radioButtonSpikeDip.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(474, 82);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Max temp % variation (+/-)";
            // 
            // textBoxDelay
            // 
            this.textBoxDelay.Location = new System.Drawing.Point(633, 106);
            this.textBoxDelay.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDelay.Name = "textBoxDelay";
            this.textBoxDelay.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxDelay.Size = new System.Drawing.Size(68, 20);
            this.textBoxDelay.TabIndex = 11;
            this.textBoxDelay.Text = "1000";
            // 
            // textBoxTemp
            // 
            this.textBoxTemp.Location = new System.Drawing.Point(633, 54);
            this.textBoxTemp.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxTemp.Name = "textBoxTemp";
            this.textBoxTemp.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxTemp.Size = new System.Drawing.Size(68, 20);
            this.textBoxTemp.TabIndex = 2;
            this.textBoxTemp.Text = "20";
            // 
            // textBoxTempVariation
            // 
            this.textBoxTempVariation.Location = new System.Drawing.Point(633, 80);
            this.textBoxTempVariation.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxTempVariation.Name = "textBoxTempVariation";
            this.textBoxTempVariation.Size = new System.Drawing.Size(68, 20);
            this.textBoxTempVariation.TabIndex = 3;
            this.textBoxTempVariation.Text = "2";
            // 
            // buttonUpdateEventConfig
            // 
            this.buttonUpdateEventConfig.Location = new System.Drawing.Point(602, 171);
            this.buttonUpdateEventConfig.Margin = new System.Windows.Forms.Padding(2);
            this.buttonUpdateEventConfig.Name = "buttonUpdateEventConfig";
            this.buttonUpdateEventConfig.Size = new System.Drawing.Size(127, 25);
            this.buttonUpdateEventConfig.TabIndex = 7;
            this.buttonUpdateEventConfig.Text = "Update Event Config";
            this.buttonUpdateEventConfig.UseVisualStyleBackColor = true;
            this.buttonUpdateEventConfig.Click += new System.EventHandler(this.ButtonUpdateEventConfig_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(474, 109);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(150, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Delay between messages (ms)";
            // 
            // groupBoxIoTHubConfig
            // 
            this.groupBoxIoTHubConfig.Controls.Add(this.panelIoTHub);
            this.groupBoxIoTHubConfig.Controls.Add(this.checkBoxMockMode);
            this.groupBoxIoTHubConfig.Location = new System.Drawing.Point(745, 24);
            this.groupBoxIoTHubConfig.Name = "groupBoxIoTHubConfig";
            this.groupBoxIoTHubConfig.Size = new System.Drawing.Size(429, 209);
            this.groupBoxIoTHubConfig.TabIndex = 10;
            this.groupBoxIoTHubConfig.TabStop = false;
            this.groupBoxIoTHubConfig.Text = "IoT Hub config";
            // 
            // panelIoTHub
            // 
            this.panelIoTHub.Controls.Add(this.label6);
            this.panelIoTHub.Controls.Add(this.buttonUpdateDeviceConfig);
            this.panelIoTHub.Controls.Add(this.textBoxDeviceKey);
            this.panelIoTHub.Controls.Add(this.textBoxHostname);
            this.panelIoTHub.Controls.Add(this.label7);
            this.panelIoTHub.Controls.Add(this.textBoxDeviceID);
            this.panelIoTHub.Controls.Add(this.label5);
            this.panelIoTHub.Location = new System.Drawing.Point(6, 72);
            this.panelIoTHub.Name = "panelIoTHub";
            this.panelIoTHub.Size = new System.Drawing.Size(417, 128);
            this.panelIoTHub.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(2, 67);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Device Key";
            // 
            // buttonUpdateDeviceConfig
            // 
            this.buttonUpdateDeviceConfig.Location = new System.Drawing.Point(280, 99);
            this.buttonUpdateDeviceConfig.Margin = new System.Windows.Forms.Padding(2);
            this.buttonUpdateDeviceConfig.Name = "buttonUpdateDeviceConfig";
            this.buttonUpdateDeviceConfig.Size = new System.Drawing.Size(128, 25);
            this.buttonUpdateDeviceConfig.TabIndex = 4;
            this.buttonUpdateDeviceConfig.Text = "Update IoT Hub Config";
            this.buttonUpdateDeviceConfig.UseVisualStyleBackColor = true;
            this.buttonUpdateDeviceConfig.Click += new System.EventHandler(this.ButtonUpdateDeviceConfig_Click);
            // 
            // textBoxDeviceKey
            // 
            this.textBoxDeviceKey.Location = new System.Drawing.Point(108, 67);
            this.textBoxDeviceKey.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDeviceKey.Name = "textBoxDeviceKey";
            this.textBoxDeviceKey.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxDeviceKey.Size = new System.Drawing.Size(300, 20);
            this.textBoxDeviceKey.TabIndex = 3;
            // 
            // textBoxHostname
            // 
            this.textBoxHostname.Location = new System.Drawing.Point(108, 9);
            this.textBoxHostname.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxHostname.Name = "textBoxHostname";
            this.textBoxHostname.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxHostname.Size = new System.Drawing.Size(300, 20);
            this.textBoxHostname.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(2, 38);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Device ID";
            // 
            // textBoxDeviceID
            // 
            this.textBoxDeviceID.Location = new System.Drawing.Point(108, 38);
            this.textBoxDeviceID.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDeviceID.Name = "textBoxDeviceID";
            this.textBoxDeviceID.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxDeviceID.Size = new System.Drawing.Size(300, 20);
            this.textBoxDeviceID.TabIndex = 8;
            this.textBoxDeviceID.Text = "ADSimulator";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 9);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "IoT Hub Hostname";
            // 
            // checkBoxMockMode
            // 
            this.checkBoxMockMode.AutoSize = true;
            this.checkBoxMockMode.Location = new System.Drawing.Point(6, 39);
            this.checkBoxMockMode.Name = "checkBoxMockMode";
            this.checkBoxMockMode.Size = new System.Drawing.Size(82, 17);
            this.checkBoxMockMode.TabIndex = 9;
            this.checkBoxMockMode.Text = "Mock mode";
            this.checkBoxMockMode.UseVisualStyleBackColor = true;
            this.checkBoxMockMode.CheckedChanged += new System.EventHandler(this.CheckBoxMockMode_CheckedChanged);
            // 
            // chartEventsSent
            // 
            chartArea2.Name = "ChartAreaEvents";
            this.chartEventsSent.ChartAreas.Add(chartArea2);
            this.chartEventsSent.Location = new System.Drawing.Point(11, 332);
            this.chartEventsSent.Name = "chartEventsSent";
            series2.ChartArea = "ChartAreaEvents";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Color = System.Drawing.Color.OrangeRed;
            series2.BorderWidth = 4;
            series2.Name = "EventValue";
            this.chartEventsSent.Series.Add(series2);
            this.chartEventsSent.Size = new System.Drawing.Size(1196, 328);
            this.chartEventsSent.TabIndex = 15;
            this.chartEventsSent.Text = "Events sent from client";
            // 
            // buttonStart
            // 
            this.buttonStart.BackColor = System.Drawing.Color.YellowGreen;
            this.buttonStart.Location = new System.Drawing.Point(1057, 288);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(72, 30);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = false;
            this.buttonStart.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.BackColor = System.Drawing.Color.DarkSalmon;
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(1135, 288);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(72, 30);
            this.buttonStop.TabIndex = 2;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = false;
            this.buttonStop.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // richTextBoxDeviceMessage
            // 
            this.richTextBoxDeviceMessage.Enabled = false;
            this.richTextBoxDeviceMessage.Location = new System.Drawing.Point(11, 665);
            this.richTextBoxDeviceMessage.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxDeviceMessage.Name = "richTextBoxDeviceMessage";
            this.richTextBoxDeviceMessage.Size = new System.Drawing.Size(1196, 52);
            this.richTextBoxDeviceMessage.TabIndex = 3;
            this.richTextBoxDeviceMessage.Text = "";
            // 
            // DeviceSimulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1233, 729);
            this.Controls.Add(this.chartEventsSent);
            this.Controls.Add(this.richTextBoxDeviceMessage);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.Tabs);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DeviceSimulator";
            this.Text = "Device Simulator";
            this.Tabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBoxEventConfig.ResumeLayout(false);
            this.groupBoxEventConfig.PerformLayout();
            this.groupBoxAnomalyConfig.ResumeLayout(false);
            this.groupBoxAnomalyConfig.PerformLayout();
            this.groupBoxIoTHubConfig.ResumeLayout(false);
            this.groupBoxIoTHubConfig.PerformLayout();
            this.panelIoTHub.ResumeLayout(false);
            this.panelIoTHub.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartEventsSent)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxTempVariation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonUpdateEventConfig;
        private System.Windows.Forms.RichTextBox richTextBoxDeviceMessage;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxDelay;
        private System.Windows.Forms.TextBox textBoxTemp;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.RadioButton radioButtonSpikeDip;
        private System.Windows.Forms.GroupBox groupBoxAnomalyConfig;
        private System.Windows.Forms.RadioButton radioButtonSlowTrend;
        private System.Windows.Forms.RadioButton radioButtonBiLevelChange;
        private System.Windows.Forms.TextBox textBoxSpikeDipMultiplier;
        private System.Windows.Forms.Label labelSpikeDipMultiplier;
        private System.Windows.Forms.Label labelLevelChangeMultiplier;
        private System.Windows.Forms.TextBox textBoxBiLevelChangeMultiplier;
        private System.Windows.Forms.Label labelSlowTrendPercentage;
        private System.Windows.Forms.TextBox textBoxSlowTrendPercent;
        private System.Windows.Forms.CheckBox checkBoxRepeatAnomaly;
        private System.Windows.Forms.TextBox textBoxSlowTrendDuration;
        private System.Windows.Forms.Label labelSlowTrendDuration;
        private System.Windows.Forms.TextBox textBoxBiLevelChangeDuration;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBoxRepeatAnomaly;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartEventsSent;
        private System.Windows.Forms.Label labelSlowTrendPercent;
        private System.Windows.Forms.Label labelSpikeDipX;
        private System.Windows.Forms.Label labelLevelChangeX;
        private System.Windows.Forms.TextBox textBoxSpikeCount;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBoxDeviceID;
        private System.Windows.Forms.TextBox textBoxDeviceKey;
        private System.Windows.Forms.TextBox textBoxHostname;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonUpdateDeviceConfig;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxMockMode;
        private System.Windows.Forms.GroupBox groupBoxEventConfig;
        private System.Windows.Forms.GroupBox groupBoxIoTHubConfig;
        private System.Windows.Forms.Panel panelIoTHub;
        private System.Windows.Forms.TextBox textBoxSensorId;
        private System.Windows.Forms.Label labelSensorId;
        private System.Windows.Forms.Label labelTempVariationPercent;
    }
}

