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
using System.Windows.Forms;
using Cassini.CommandLine;
using CassiniDev.Views;

namespace CassiniDev
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            CommandLineArguments sargs = new CommandLineArguments();

            if (!Parser.ParseArguments(args, sargs))
            {
                string usage = Parser.ArgumentsUsage(typeof(CommandLineArguments), 120);
                MessageBox.Show(usage);
                Environment.Exit(-1);
                return;
            }
            switch (sargs.RunMode)
            {
                case RunMode.Server:
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    using (FormsView view = new FormsView())
                    {
                        view.Start(sargs);
                        Application.Run(view);
                    }
                    break;
                case RunMode.Hostsfile:
                    SetHostsFile(sargs);
                    break;
            }
        }

        private static void SetHostsFile(CommandLineArguments sargs)
        {
            try
            {
                if (sargs.AddHost)
                {
                    HostsFile.AddHostEntry(sargs.IPAddress, sargs.HostName);
                }
                else
                {
                    HostsFile.RemoveHostEntry(sargs.IPAddress, sargs.HostName);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Environment.Exit(-1);
            }
            catch
            {
                Environment.Exit(-2);
            }
        }
    }
}