using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Cassini.CommandLine;
using Salient.Net;
using System.Collections;

namespace Cassini
{
    /// <summary>
    /// TODO: change add host entry to uac button if not running as administrator
    /// </summary>
    public partial class MainForm2 : Form
    {
        private Server _server;

        public MainForm2(string[] args)
        {
            InitializeComponent();

            if (args != null && args.Length > 0)
            {
                var pargs = new CassiniArgs();
                Parser.ParseArguments(args, pargs);
                BuildFormFromArgs(pargs);
            }

        }

        private void RemoveHostEntry()
        {
            if (!string.IsNullOrEmpty(_server.HostName) && string.Compare(_server.HostName, "localhost", true) != 0 && CheckBoxAddHostEntry.Checked)
            {
                // remove host entry, if we added one
                using (var hostFile = new HostsFile())
                {
                    hostFile.Open();
                    var ip = _server.IPAddress.Equals(IPAddress.Any) ? IPAddress.Loopback : _server.IPAddress;
                    var item = hostFile[_server.HostName];
                    if(item.IPAddress.Equals(ip) && item.Comment=="cassini")
                    {
                        hostFile.Remove(_server.HostName);
                    }
                    hostFile.Save();
                }

            }
        }


        private void ButtonStop_Click(object sender, EventArgs e)
        {
            RemoveHostEntry();
            _server.Stop();
            _server.Dispose();
            LinkLabelRootUrl.Text = "";
            ButtonStart.Enabled = true;
            ButtonStop.Enabled = false;
            GroupBoxMain.Enabled = true;
        }



        private void ButtonStart_Click(object sender, EventArgs e)
        {

            var pargs = BuildArgsFromForm();
            if (pargs != null)
            {
                try
                {
                    BuildFormFromArgs(pargs);

                    _server = new Server(pargs.Port, pargs.VirtualPath, pargs.AppPath, pargs.ReusePort, pargs.IPAddress, pargs.HostName);

                    _server.Start();

                    if (!string.IsNullOrEmpty(pargs.HostName) && string.Compare(pargs.HostName, "localhost", true) != 0 && CheckBoxAddHostEntry.Checked)
                    {
                        // add host entry
                        using (var hostFile = new HostsFile())
                        {
                            var ip = pargs.IPAddress.Equals(IPAddress.Any) ? IPAddress.Loopback : pargs.IPAddress;
                            hostFile.Open();
                            hostFile.Add(ip, pargs.HostName, "cassini", EntryAddMode.LeaveExisting);
                            hostFile.Save();
                        }
                    }

                    LinkLabelRootUrl.Text = _server.RootUrl;

                    // disable form
                    GroupBoxMain.Enabled = false;
                    ButtonStart.Enabled = false;
                    ButtonStop.Enabled = true;
                }
                catch (ArgumentException ex)
                {
                    GiveErrorFeedback(ex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        #region Form Events

        private void ButtonBrowsePhysicalPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    TextBoxAppPath.Text = fbd.SelectedPath;
                }
            }
        }


        private void RadioButtonPortSpecific_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxPortRangeStart.Enabled = false;
            TextBoxPortRangeEnd.Enabled = false;
            TextBoxPort.Enabled = true;
        }

        private void RadioButtonPortFind_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxPort.Enabled = false;
            TextBoxPortRangeStart.Enabled = true;
            TextBoxPortRangeStart.Enabled = true;
        }

        private void RadioButtonIPLoopBack_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxIPSpecific.Enabled = false;
        }

        private void RadioButtonIPAny_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxIPSpecific.Enabled = false;
        }

        private void RadioButtonIPSpecific_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxIPSpecific.Enabled = true;
        }
        #endregion


        private CassiniArgs BuildArgsFromForm()
        {
            var args = new List<string>();

            args.Add(string.Format("/apppath:{0}", TextBoxAppPath.Text));

            string ipAddress;
            if (RadioButtonIPLoopBack.Checked)
            {
                ipAddress = "loopback";
            }
            else if (RadioButtonIPAny.Checked)
            {
                ipAddress = "any";
            }
            else
            {
                ipAddress = TextBoxIPSpecific.Text;
            }

            if (!string.IsNullOrEmpty(ipAddress))
            {
                args.Add(string.Format("/ipaddress:{0}", ipAddress));
            }



            if (TextBoxPort.Enabled)
            {
                if (!string.IsNullOrEmpty(TextBoxPort.Text))
                {
                    args.Add(string.Format("/port:{0}", TextBoxPort.Text));
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(TextBoxPortRangeEnd.Text))
                {
                    args.Add(string.Format("/portrangeend:{0}", TextBoxPortRangeEnd.Text));
                }

                if (!string.IsNullOrEmpty(TextBoxPortRangeStart.Text))
                {
                    args.Add(string.Format("/portrangestart:{0}", TextBoxPortRangeStart.Text));
                }
            }


            args.Add(string.Format("/reuse:{0}", CheckBoxReusePort.Checked));

            if (!string.IsNullOrEmpty(TextBoxVPath.Text))
            {
                args.Add(string.Format("/vpath:{0}", TextBoxVPath.Text));
            }

            if (!string.IsNullOrEmpty(TextBoxHostName.Text))
            {
                args.Add(string.Format("/hostname:{0}", TextBoxHostName.Text));
            }



            _errorProvider.Clear();
            CassiniArgs pargs = null;
            try
            {

                pargs = CassiniArgsParser.Parse(args.ToArray());

            }
            catch (ArgumentException ex)
            {
                GiveErrorFeedback(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            return pargs;
        }

        private void BuildFormFromArgs(CassiniArgs pargs)
        {
            TextBoxAppPath.Text = pargs.AppPath;
            TextBoxVPath.Text = pargs.VirtualPath;
            TextBoxHostName.Text = pargs.HostName;

            if (pargs.IsPortDynamic)
            {
                RadioButtonPortFind.Checked = true;
                TextBoxPort.Text = string.Empty;
            }
            else
            {
                RadioButtonPortSpecific.Checked = true;
                TextBoxPort.Text = pargs.Port.ToString();
            }

            TextBoxPort.Text = pargs.Port.ToString();
            TextBoxPortRangeStart.Text = pargs.PortRangeStart.ToString();
            TextBoxPortRangeEnd.Text = pargs.PortRangeEnd.ToString();

            CheckBoxReusePort.Checked = pargs.ReusePort;



            switch (pargs.IPAddressString.ToLower())
            {
                case "any":
                    RadioButtonIPAny.Checked = true;
                    TextBoxIPSpecific.Text = string.Empty;
                    break;
                case "loopback":
                    RadioButtonIPLoopBack.Checked = true;
                    TextBoxIPSpecific.Text = string.Empty;
                    break;
                default:
                    RadioButtonIPSpecific.Checked = true;
                    TextBoxIPSpecific.Text = pargs.IPAddress.ToString();
                    break;
            }


        }

        private void LinkLabelRootUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo();
                proc.StartInfo.FileName = "iexplore";
                proc.StartInfo.Arguments = string.Format("\"{0}\"", _server.RootUrl);
                proc.Start();
            }
        }
        private void GiveErrorFeedback(ArgumentException ex)
        {
            switch (ex.ParamName.ToLower())
            {
                case "port":
                    _errorProvider.SetError(TextBoxPort, ex.Message);
                    break;
                case "apppath":
                    _errorProvider.SetError(TextBoxAppPath, ex.Message);
                    break;
                case "hostname":
                    _errorProvider.SetError(TextBoxHostName, ex.Message);
                    break;
                default:
                    MessageBox.Show(string.Format("{0}", ex.Message));
                    break;
            }
        }

        private void TextBoxHostName_TextChanged(object sender, EventArgs e)
        {
            // protect against modifying localhost record
            if (!string.IsNullOrEmpty(TextBoxHostName.Text))
            {
                if (string.Compare(TextBoxHostName.Text, "localhost", true) == 0)
                {
                    CheckBoxAddHostEntry.Enabled = false;
                    CheckBoxAddHostEntry.Checked = false;
                }
                else
                {
                    CheckBoxAddHostEntry.Enabled = true;
                }
            }
            else
            {
                CheckBoxAddHostEntry.Enabled = false;
                CheckBoxAddHostEntry.Checked = false;
            }
        }

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
            if (_server != null)
            {
                RemoveHostEntry();
            }

            base.Dispose(disposing);
        }
        ~MainForm2()
        {
            Dispose(false);
        }

        

    }
}
