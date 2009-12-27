namespace Cassini
{
    partial class MainForm2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.TextBoxAppPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LabelVPath = new System.Windows.Forms.Label();
            this.TextBoxVPath = new System.Windows.Forms.TextBox();
            this.LabelHostName = new System.Windows.Forms.Label();
            this.TextBoxHostName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RadioButtonIPSpecific = new System.Windows.Forms.RadioButton();
            this.RadioButtonIPAny = new System.Windows.Forms.RadioButton();
            this.RadioButtonIPLoopBack = new System.Windows.Forms.RadioButton();
            this.TextBoxIPSpecific = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CheckBoxReusePort = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TextBoxPortRangeEnd = new System.Windows.Forms.TextBox();
            this.TextBoxPortRangeStart = new System.Windows.Forms.TextBox();
            this.RadioButtonPortFind = new System.Windows.Forms.RadioButton();
            this.TextBoxPort = new System.Windows.Forms.TextBox();
            this.RadioButtonPortSpecific = new System.Windows.Forms.RadioButton();
            this.ButtonBrowsePhysicalPath = new System.Windows.Forms.Button();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.ButtonStart = new System.Windows.Forms.Button();
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.CheckBoxAddHostEntry = new System.Windows.Forms.CheckBox();
            this.GroupBoxMain = new System.Windows.Forms.GroupBox();
            this.LinkLabelRootUrl = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this.GroupBoxMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // TextBoxAppPath
            // 
            this.TextBoxAppPath.Location = new System.Drawing.Point(6, 32);
            this.TextBoxAppPath.Name = "TextBoxAppPath";
            this.TextBoxAppPath.Size = new System.Drawing.Size(262, 20);
            this.TextBoxAppPath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Physical Path";
            // 
            // LabelVPath
            // 
            this.LabelVPath.AutoSize = true;
            this.LabelVPath.Location = new System.Drawing.Point(6, 54);
            this.LabelVPath.Name = "LabelVPath";
            this.LabelVPath.Size = new System.Drawing.Size(61, 13);
            this.LabelVPath.TabIndex = 3;
            this.LabelVPath.Text = "Virtual Path";
            // 
            // TextBoxVPath
            // 
            this.TextBoxVPath.Location = new System.Drawing.Point(6, 71);
            this.TextBoxVPath.Name = "TextBoxVPath";
            this.TextBoxVPath.Size = new System.Drawing.Size(295, 20);
            this.TextBoxVPath.TabIndex = 2;
            this.TextBoxVPath.Text = "/";
            // 
            // LabelHostName
            // 
            this.LabelHostName.AutoSize = true;
            this.LabelHostName.Location = new System.Drawing.Point(6, 94);
            this.LabelHostName.Name = "LabelHostName";
            this.LabelHostName.Size = new System.Drawing.Size(106, 13);
            this.LabelHostName.TabIndex = 5;
            this.LabelHostName.Text = "Host Name (optional)";
            // 
            // TextBoxHostName
            // 
            this.TextBoxHostName.Location = new System.Drawing.Point(6, 110);
            this.TextBoxHostName.Name = "TextBoxHostName";
            this.TextBoxHostName.Size = new System.Drawing.Size(192, 20);
            this.TextBoxHostName.TabIndex = 3;
            this.TextBoxHostName.TextChanged += new System.EventHandler(this.TextBoxHostName_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RadioButtonIPSpecific);
            this.groupBox1.Controls.Add(this.RadioButtonIPAny);
            this.groupBox1.Controls.Add(this.RadioButtonIPLoopBack);
            this.groupBox1.Controls.Add(this.TextBoxIPSpecific);
            this.groupBox1.Location = new System.Drawing.Point(6, 136);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(137, 103);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "IP Address";
            // 
            // RadioButtonIPSpecific
            // 
            this.RadioButtonIPSpecific.AutoSize = true;
            this.RadioButtonIPSpecific.Location = new System.Drawing.Point(18, 54);
            this.RadioButtonIPSpecific.Name = "RadioButtonIPSpecific";
            this.RadioButtonIPSpecific.Size = new System.Drawing.Size(63, 17);
            this.RadioButtonIPSpecific.TabIndex = 5;
            this.RadioButtonIPSpecific.Text = "Specific";
            this.RadioButtonIPSpecific.UseVisualStyleBackColor = true;
            this.RadioButtonIPSpecific.CheckedChanged += new System.EventHandler(this.RadioButtonIPSpecific_CheckedChanged);
            // 
            // RadioButtonIPAny
            // 
            this.RadioButtonIPAny.AutoSize = true;
            this.RadioButtonIPAny.Location = new System.Drawing.Point(18, 37);
            this.RadioButtonIPAny.Name = "RadioButtonIPAny";
            this.RadioButtonIPAny.Size = new System.Drawing.Size(43, 17);
            this.RadioButtonIPAny.TabIndex = 5;
            this.RadioButtonIPAny.Text = "Any";
            this.RadioButtonIPAny.UseVisualStyleBackColor = true;
            this.RadioButtonIPAny.CheckedChanged += new System.EventHandler(this.RadioButtonIPAny_CheckedChanged);
            // 
            // RadioButtonIPLoopBack
            // 
            this.RadioButtonIPLoopBack.AutoSize = true;
            this.RadioButtonIPLoopBack.Checked = true;
            this.RadioButtonIPLoopBack.Location = new System.Drawing.Point(18, 20);
            this.RadioButtonIPLoopBack.Name = "RadioButtonIPLoopBack";
            this.RadioButtonIPLoopBack.Size = new System.Drawing.Size(73, 17);
            this.RadioButtonIPLoopBack.TabIndex = 5;
            this.RadioButtonIPLoopBack.TabStop = true;
            this.RadioButtonIPLoopBack.Text = "Loopback";
            this.RadioButtonIPLoopBack.UseVisualStyleBackColor = true;
            this.RadioButtonIPLoopBack.CheckedChanged += new System.EventHandler(this.RadioButtonIPLoopBack_CheckedChanged);
            // 
            // TextBoxIPSpecific
            // 
            this.TextBoxIPSpecific.Enabled = false;
            this.TextBoxIPSpecific.Location = new System.Drawing.Point(35, 71);
            this.TextBoxIPSpecific.Name = "TextBoxIPSpecific";
            this.TextBoxIPSpecific.Size = new System.Drawing.Size(92, 20);
            this.TextBoxIPSpecific.TabIndex = 7;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CheckBoxReusePort);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.TextBoxPortRangeEnd);
            this.groupBox2.Controls.Add(this.TextBoxPortRangeStart);
            this.groupBox2.Controls.Add(this.RadioButtonPortFind);
            this.groupBox2.Controls.Add(this.TextBoxPort);
            this.groupBox2.Controls.Add(this.RadioButtonPortSpecific);
            this.groupBox2.Location = new System.Drawing.Point(149, 136);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(158, 103);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Port";
            // 
            // CheckBoxReusePort
            // 
            this.CheckBoxReusePort.AutoSize = true;
            this.CheckBoxReusePort.Location = new System.Drawing.Point(15, 73);
            this.CheckBoxReusePort.Name = "CheckBoxReusePort";
            this.CheckBoxReusePort.Size = new System.Drawing.Size(97, 17);
            this.CheckBoxReusePort.TabIndex = 17;
            this.CheckBoxReusePort.Text = "Reuse idle port";
            this.CheckBoxReusePort.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(74, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "and";
            // 
            // TextBoxPortRangeEnd
            // 
            this.TextBoxPortRangeEnd.Location = new System.Drawing.Point(105, 53);
            this.TextBoxPortRangeEnd.Name = "TextBoxPortRangeEnd";
            this.TextBoxPortRangeEnd.Size = new System.Drawing.Size(37, 20);
            this.TextBoxPortRangeEnd.TabIndex = 15;
            this.TextBoxPortRangeEnd.Text = "65534";
            // 
            // TextBoxPortRangeStart
            // 
            this.TextBoxPortRangeStart.Location = new System.Drawing.Point(32, 53);
            this.TextBoxPortRangeStart.Name = "TextBoxPortRangeStart";
            this.TextBoxPortRangeStart.Size = new System.Drawing.Size(36, 20);
            this.TextBoxPortRangeStart.TabIndex = 14;
            this.TextBoxPortRangeStart.Text = "8080";
            // 
            // RadioButtonPortFind
            // 
            this.RadioButtonPortFind.AutoSize = true;
            this.RadioButtonPortFind.Checked = true;
            this.RadioButtonPortFind.Location = new System.Drawing.Point(15, 36);
            this.RadioButtonPortFind.Name = "RadioButtonPortFind";
            this.RadioButtonPortFind.Size = new System.Drawing.Size(137, 17);
            this.RadioButtonPortFind.TabIndex = 6;
            this.RadioButtonPortFind.Text = "First Available between:";
            this.RadioButtonPortFind.UseVisualStyleBackColor = true;
            this.RadioButtonPortFind.CheckedChanged += new System.EventHandler(this.RadioButtonPortFind_CheckedChanged);
            // 
            // TextBoxPort
            // 
            this.TextBoxPort.Enabled = false;
            this.TextBoxPort.Location = new System.Drawing.Point(84, 17);
            this.TextBoxPort.Name = "TextBoxPort";
            this.TextBoxPort.Size = new System.Drawing.Size(43, 20);
            this.TextBoxPort.TabIndex = 7;
            this.TextBoxPort.Text = "8080";
            // 
            // RadioButtonPortSpecific
            // 
            this.RadioButtonPortSpecific.AutoSize = true;
            this.RadioButtonPortSpecific.Location = new System.Drawing.Point(15, 19);
            this.RadioButtonPortSpecific.Name = "RadioButtonPortSpecific";
            this.RadioButtonPortSpecific.Size = new System.Drawing.Size(63, 17);
            this.RadioButtonPortSpecific.TabIndex = 6;
            this.RadioButtonPortSpecific.TabStop = true;
            this.RadioButtonPortSpecific.Text = "Specific";
            this.RadioButtonPortSpecific.UseVisualStyleBackColor = true;
            this.RadioButtonPortSpecific.CheckedChanged += new System.EventHandler(this.RadioButtonPortSpecific_CheckedChanged);
            // 
            // ButtonBrowsePhysicalPath
            // 
            this.ButtonBrowsePhysicalPath.Location = new System.Drawing.Point(274, 32);
            this.ButtonBrowsePhysicalPath.Name = "ButtonBrowsePhysicalPath";
            this.ButtonBrowsePhysicalPath.Size = new System.Drawing.Size(27, 23);
            this.ButtonBrowsePhysicalPath.TabIndex = 1;
            this.ButtonBrowsePhysicalPath.Text = "...";
            this.ButtonBrowsePhysicalPath.UseVisualStyleBackColor = true;
            this.ButtonBrowsePhysicalPath.Click += new System.EventHandler(this.ButtonBrowsePhysicalPath_Click);
            // 
            // ButtonStop
            // 
            this.ButtonStop.Enabled = false;
            this.ButtonStop.Location = new System.Drawing.Point(250, 280);
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.Size = new System.Drawing.Size(75, 23);
            this.ButtonStop.TabIndex = 12;
            this.ButtonStop.Text = "Stop";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(169, 280);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 13;
            this.ButtonStart.Text = "Start";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // _errorProvider
            // 
            this._errorProvider.ContainerControl = this;
            // 
            // CheckBoxAddHostEntry
            // 
            this.CheckBoxAddHostEntry.AutoSize = true;
            this.CheckBoxAddHostEntry.Location = new System.Drawing.Point(204, 110);
            this.CheckBoxAddHostEntry.Name = "CheckBoxAddHostEntry";
            this.CheckBoxAddHostEntry.Size = new System.Drawing.Size(97, 17);
            this.CheckBoxAddHostEntry.TabIndex = 4;
            this.CheckBoxAddHostEntry.Text = "Add Host Entry";
            this.toolTip1.SetToolTip(this.CheckBoxAddHostEntry, "If specified an entry is added to the host file. This entry is removed when serve" +
                    "r is stopped.");
            this.CheckBoxAddHostEntry.UseVisualStyleBackColor = true;
            // 
            // GroupBoxMain
            // 
            this.GroupBoxMain.Controls.Add(this.label1);
            this.GroupBoxMain.Controls.Add(this.CheckBoxAddHostEntry);
            this.GroupBoxMain.Controls.Add(this.TextBoxAppPath);
            this.GroupBoxMain.Controls.Add(this.TextBoxVPath);
            this.GroupBoxMain.Controls.Add(this.LabelVPath);
            this.GroupBoxMain.Controls.Add(this.TextBoxHostName);
            this.GroupBoxMain.Controls.Add(this.ButtonBrowsePhysicalPath);
            this.GroupBoxMain.Controls.Add(this.LabelHostName);
            this.GroupBoxMain.Controls.Add(this.groupBox2);
            this.GroupBoxMain.Controls.Add(this.groupBox1);
            this.GroupBoxMain.Location = new System.Drawing.Point(12, 25);
            this.GroupBoxMain.Name = "GroupBoxMain";
            this.GroupBoxMain.Size = new System.Drawing.Size(313, 249);
            this.GroupBoxMain.TabIndex = 19;
            this.GroupBoxMain.TabStop = false;
            // 
            // LinkLabelRootUrl
            // 
            this.LinkLabelRootUrl.AutoSize = true;
            this.LinkLabelRootUrl.Location = new System.Drawing.Point(12, 9);
            this.LinkLabelRootUrl.Name = "LinkLabelRootUrl";
            this.LinkLabelRootUrl.Size = new System.Drawing.Size(0, 13);
            this.LinkLabelRootUrl.TabIndex = 20;
            this.LinkLabelRootUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelRootUrl_LinkClicked);
            // 
            // MainForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 317);
            this.Controls.Add(this.LinkLabelRootUrl);
            this.Controls.Add(this.GroupBoxMain);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.ButtonStop);
            this.Name = "MainForm2";
            this.Text = "Cassini Developer Edition";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this.GroupBoxMain.ResumeLayout(false);
            this.GroupBoxMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxAppPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label LabelVPath;
        private System.Windows.Forms.TextBox TextBoxVPath;
        private System.Windows.Forms.Label LabelHostName;
        private System.Windows.Forms.TextBox TextBoxHostName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RadioButtonIPLoopBack;
        private System.Windows.Forms.TextBox TextBoxIPSpecific;
        private System.Windows.Forms.RadioButton RadioButtonIPSpecific;
        private System.Windows.Forms.RadioButton RadioButtonIPAny;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox CheckBoxReusePort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TextBoxPortRangeEnd;
        private System.Windows.Forms.TextBox TextBoxPortRangeStart;
        private System.Windows.Forms.RadioButton RadioButtonPortFind;
        private System.Windows.Forms.TextBox TextBoxPort;
        private System.Windows.Forms.RadioButton RadioButtonPortSpecific;
        private System.Windows.Forms.Button ButtonBrowsePhysicalPath;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.ErrorProvider _errorProvider;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox CheckBoxAddHostEntry;
        private System.Windows.Forms.GroupBox GroupBoxMain;
        private System.Windows.Forms.LinkLabel LinkLabelRootUrl;
    }
}