namespace TISensorToEH
{
    partial class MainForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.txtSBNamespace = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtEHName = new System.Windows.Forms.TextBox();
            this.txtAccessPolicyName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAccessPolicyKey = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.RichTextBox();
            this.txtSensorName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.rbStart = new System.Windows.Forms.RadioButton();
            this.rbStop = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Highlight;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.button1.Location = new System.Drawing.Point(73, 539);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(357, 47);
            this.button1.TabIndex = 0;
            this.button1.Text = "Send Data to Eventhub";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtSBNamespace
            // 
            this.txtSBNamespace.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSBNamespace.Location = new System.Drawing.Point(67, 65);
            this.txtSBNamespace.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtSBNamespace.Name = "txtSBNamespace";
            this.txtSBNamespace.Size = new System.Drawing.Size(684, 28);
            this.txtSBNamespace.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(61, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "Servicebus namespace";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(61, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(147, 24);
            this.label2.TabIndex = 4;
            this.label2.Text = "Eventhub Name";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // txtEHName
            // 
            this.txtEHName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEHName.Location = new System.Drawing.Point(65, 159);
            this.txtEHName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtEHName.Name = "txtEHName";
            this.txtEHName.Size = new System.Drawing.Size(684, 28);
            this.txtEHName.TabIndex = 2;
            // 
            // txtAccessPolicyName
            // 
            this.txtAccessPolicyName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAccessPolicyName.Location = new System.Drawing.Point(69, 254);
            this.txtAccessPolicyName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtAccessPolicyName.Name = "txtAccessPolicyName";
            this.txtAccessPolicyName.Size = new System.Drawing.Size(684, 28);
            this.txtAccessPolicyName.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(67, 214);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(249, 24);
            this.label3.TabIndex = 6;
            this.label3.Text = "Shared Access Policy Name";
            // 
            // txtAccessPolicyKey
            // 
            this.txtAccessPolicyKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAccessPolicyKey.Location = new System.Drawing.Point(72, 358);
            this.txtAccessPolicyKey.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtAccessPolicyKey.Name = "txtAccessPolicyKey";
            this.txtAccessPolicyKey.Size = new System.Drawing.Size(684, 28);
            this.txtAccessPolicyKey.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(69, 318);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(230, 24);
            this.label4.TabIndex = 8;
            this.label4.Text = "Shared Access Policy Key";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.ForeColor = System.Drawing.SystemColors.Highlight;
            this.txtStatus.Location = new System.Drawing.Point(781, 27);
            this.txtStatus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(207, 549);
            this.txtStatus.TabIndex = 9;
            this.txtStatus.Text = " ";
            // 
            // txtSensorName
            // 
            this.txtSensorName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSensorName.Location = new System.Drawing.Point(76, 458);
            this.txtSensorName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtSensorName.Name = "txtSensorName";
            this.txtSensorName.Size = new System.Drawing.Size(684, 28);
            this.txtSensorName.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(73, 418);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(126, 24);
            this.label5.TabIndex = 11;
            this.label5.Text = "Sensor Name";
            // 
            // rbStart
            // 
            this.rbStart.AutoSize = true;
            this.rbStart.Location = new System.Drawing.Point(467, 510);
            this.rbStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbStart.Name = "rbStart";
            this.rbStart.Size = new System.Drawing.Size(59, 21);
            this.rbStart.TabIndex = 12;
            this.rbStart.Text = "Start";
            this.rbStart.UseVisualStyleBackColor = true;
            this.rbStart.Visible = false;
            this.rbStart.CheckedChanged += new System.EventHandler(this.rbStart_CheckedChanged);
            // 
            // rbStop
            // 
            this.rbStop.AutoSize = true;
            this.rbStop.Checked = true;
            this.rbStop.Location = new System.Drawing.Point(467, 539);
            this.rbStop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbStop.Name = "rbStop";
            this.rbStop.Size = new System.Drawing.Size(58, 21);
            this.rbStop.TabIndex = 13;
            this.rbStop.TabStop = true;
            this.rbStop.Text = "Stop";
            this.rbStop.UseVisualStyleBackColor = true;
            this.rbStop.Visible = false;
            this.rbStop.CheckedChanged += new System.EventHandler(this.rbStop_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 608);
            this.Controls.Add(this.rbStop);
            this.Controls.Add(this.rbStart);
            this.Controls.Add(this.txtSensorName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.txtAccessPolicyKey);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtAccessPolicyName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtEHName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSBNamespace);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "Azure Stream Analytics Demo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtSBNamespace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtEHName;
        private System.Windows.Forms.TextBox txtAccessPolicyName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAccessPolicyKey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox txtStatus;
        private System.Windows.Forms.TextBox txtSensorName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton rbStart;
        private System.Windows.Forms.RadioButton rbStop;
    }
}

