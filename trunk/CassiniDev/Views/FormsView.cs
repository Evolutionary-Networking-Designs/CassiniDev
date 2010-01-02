// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace CassiniDev
{
    public partial class FormsView : Form, IView
    {
        private IPresenter _presenter;
        private RunState _runState;

        /// <summary>
        /// DI Constructor
        /// </summary>
        /// <param name="presenter"></param>
        public FormsView(IPresenter presenter)
        {
            InitializeComponent();
            _presenter = presenter;
        }

  
        #region IView Members

        public RunState RunState
        {
            get { return _runState; }
            set
            {
                _runState = value;
                switch (value)
                {
                    case RunState.Idle:
                        ButtonStart.Enabled = true;
                        ButtonStop.Enabled = false;
                        EnableForm();
                        break;
                    case RunState.Running:
                        ButtonStart.Enabled = false;
                        ButtonStop.Enabled = true;
                        DisableForm();
                        break;
                }
            }
        }


        public IPMode IPMode
        {
            get
            {
                if (RadioButtonIPAny.Checked)
                {
                    return IPMode.Any;
                }
                if (RadioButtonIPLoopBack.Checked)
                {
                    return IPMode.Loopback;
                }
                return IPMode.Specific;
            }
            set
            {
                switch (value)
                {
                    case IPMode.Loopback:
                        RadioButtonIPLoopBack.Checked = true;
                        break;
                    case IPMode.Any:
                        RadioButtonIPAny.Checked = true;
                        break;
                    case IPMode.Specific:
                        RadioButtonIPSpecific.Checked = true;
                        break;
                }
            }
        }


        public string RootUrl
        {
            get
            {
                return LinkLabelRootUrl.Text;
            }
            set { LinkLabelRootUrl.Text = value; }
        }

        public PortMode PortMode
        {
            get { return RadioButtonPortSpecific.Checked ? PortMode.Specific : PortMode.FirstAvailable; }
            set
            {
                switch (value)
                {
                    case PortMode.FirstAvailable:
                        RadioButtonPortFind.Checked = true;
                        break;
                    case PortMode.Specific:
                        RadioButtonPortSpecific.Checked = true;
                        break;
                }
            }
        }
        
        public string ApplicationPath
        {
            get { return TextBoxAppPath.Text; }
            set { TextBoxAppPath.Text = value; }
        }

        public string VirtualPath
        {
            get { return TextBoxVPath.Text; }
            set { TextBoxVPath.Text = value; }
        }

        public string HostName
        {
            get { return TextBoxHostName.Text; }
            set { TextBoxHostName.Text = value; }
        }

        public bool AddHost
        {
            get { return CheckBoxAddHostEntry.Checked; }
            set { CheckBoxAddHostEntry.Checked = value; }
        }

        public string IPAddress
        {
            get { return TextBoxIPSpecific.Text; }
            set { TextBoxIPSpecific.Text = value; }
        }

        public bool IPv6
        {
            get { return CheckBoxIPV6.Checked; }
            set { CheckBoxIPV6.Checked = value; }
        }

        public ushort Port
        {
            get
            {
                ushort result;
                ushort.TryParse(TextBoxPort.Text, out result);
                return result;
            }
            set { TextBoxPort.Text = value.ToString(); }
        }

        public ushort PortRangeStart
        {
            get
            {
                ushort result;
                ushort.TryParse(TextBoxPortRangeStart.Text, out result);
                return result;
            }
            set { TextBoxPortRangeStart.Text = value.ToString(); }
        }

        public ushort PortRangeEnd
        {
            get
            {
                ushort result;
                ushort.TryParse(TextBoxPortRangeEnd.Text, out result);
                return result;
            }
            set { TextBoxPortRangeEnd.Text = value.ToString(); }
        }

        public void ClearError()
        {
            errorProvider1.Clear();
        }

        public void SetError(ErrorField field, string value)
        {
            switch (field)
            {
                case ErrorField.ApplicationPath:
                    errorProvider1.SetError(TextBoxAppPath, value);
                    break;
                case ErrorField.VirtualPath:
                    errorProvider1.SetError(TextBoxVPath, value);
                    break;
                case ErrorField.HostName:
                    errorProvider1.SetError(TextBoxHostName, value);
                    break;
                case ErrorField.IsAddHost:
                    errorProvider1.SetError(CheckBoxAddHostEntry, value);
                    break;
                case ErrorField.IPAddress:
                    errorProvider1.SetError(TextBoxIPSpecific, value);
                    break;
                case ErrorField.IPAddressAny:
                    errorProvider1.SetError(RadioButtonIPAny, value);
                    break;
                case ErrorField.IPAddressLoopBack:
                    errorProvider1.SetError(RadioButtonIPLoopBack, value);
                    break;
                case ErrorField.Port:
                    errorProvider1.SetError(TextBoxPort, value);
                    break;
                case ErrorField.PortRange:
                    errorProvider1.SetError(TextBoxPortRangeStart, value);
                    errorProvider1.SetError(TextBoxPortRangeEnd, value);
                    break;
                case ErrorField.PortRangeStart:
                    errorProvider1.SetError(TextBoxPortRangeStart, value);
                    break;
                case ErrorField.PortRangeEnd:
                    errorProvider1.SetError(TextBoxPortRangeEnd, value);
                    break;
                case ErrorField.None:
                    MessageBox.Show(value, "Error");
                    break;
            }
        }

        public void Start()
        {
            CommandLineArguments args = new CommandLineArguments
                                        {
                                            AddHost = AddHost,
                                            ApplicationPath = ApplicationPath,
                                            HostName = HostName,
                                            IPAddress = IPAddress,
                                            IPMode = IPMode,
                                            IPv6 = IPv6,
                                            Port = Port,
                                            PortMode = PortMode,
                                            PortRangeEnd = PortRangeEnd,
                                            PortRangeStart = PortRangeStart,
                                            VirtualPath = VirtualPath
                                        };

            _presenter.Start(args);
        }

        public void Stop()
        {
            _presenter.Stop(AddHost);
        }

        private void EnableForm()
        {
            TextBoxAppPath.Enabled = true;
            ButtonBrowsePhysicalPath.Enabled = true;
            TextBoxVPath.Enabled = true;
            TextBoxHostName.Enabled = true;
            GroupBoxIPAddress.Enabled = true;
            GroupBoxPort.Enabled = true;
            LabelHostName.Enabled = true;
            LabelPhysicalPath.Enabled = true;
            LabelVPath.Enabled = true;

            CheckBoxAddHostEntry.Enabled = !String.IsNullOrEmpty(HostName);
        }


        private void DisableForm()
        {
            TextBoxAppPath.Enabled = false;
            ButtonBrowsePhysicalPath.Enabled = false;
            TextBoxVPath.Enabled = false;
            TextBoxHostName.Enabled = false;
            GroupBoxIPAddress.Enabled = false;
            GroupBoxPort.Enabled = false;
            CheckBoxAddHostEntry.Enabled = false;
            LabelHostName.Enabled = false;
            LabelPhysicalPath.Enabled = false;
            LabelVPath.Enabled = false;
        }

        #endregion

        #region Form Events

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            Stop();
        }


        private void CheckBoxAddHostEntry_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsAdmin())
            {
                if (CheckBoxAddHostEntry.Checked)
                {
                    AddShieldToButton(ButtonStart);
                }
                else
                {
                    RemoveShieldFromButton(ButtonStart);
                }
            }
        }

        private void ButtonBrowsePhysicalPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                ApplicationPath = fbd.SelectedPath;
            }
        }

        private void LinkLabelRootUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo();
                proc.StartInfo.FileName = "iexplore";
                proc.StartInfo.Arguments = string.Format("\"{0}\"", LinkLabelRootUrl.Text);
                proc.Start();
            }
        }


        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RunState == RunState.Running)
            {
                Stop();
            }
        }

        private void RadioButtonIPLoopBack_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxIPSpecific.Enabled = false;
            CheckBoxIPV6.Enabled = true;
        }

        private void RadioButtonIPAny_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxIPSpecific.Enabled = false;
            CheckBoxIPV6.Enabled = true;
        }

        private void RadioButtonIPSpecific_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxIPSpecific.Enabled = true;
            CheckBoxIPV6.Enabled = false;
            CheckBoxIPV6.Checked = false;
        }

        private void RadioButtonPortSpecific_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxPort.Enabled = true;
            TextBoxPortRangeEnd.Enabled = false;
            TextBoxPortRangeStart.Enabled = false;
        }

        private void RadioButtonPortFind_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxPort.Enabled = false;
            TextBoxPortRangeEnd.Enabled = true;
            TextBoxPortRangeStart.Enabled = true;
        }

        private void TextBoxHostName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(HostName))
            {
                CheckBoxAddHostEntry.Enabled = false;
                CheckBoxAddHostEntry.Checked = false;
            }
            else
            {
                CheckBoxAddHostEntry.Enabled = true;
            }
        }

        #endregion

        #region UAC

        private const int BcmFirst = 0x1600; //Normal button
        private const int BcmSetshield = (BcmFirst + 0x000C); //Elevated button

        [DllImport("user32")]
        private static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);


        private static void AddShieldToButton(Button b)
        {
            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BcmSetshield, 0, 0xFFFFFFFF);
        }

        private static void RemoveShieldFromButton(Button b)
        {
            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BcmSetshield, 0, 0x0);
        }

        private static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion
    }
}