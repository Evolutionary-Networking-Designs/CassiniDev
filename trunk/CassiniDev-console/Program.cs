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
using System.Net;
using Cassini;
using Cassini.CommandLine;


namespace CassiniDev
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] cmdLine)
        {
            CommandLineArguments args = new CommandLineArguments();
 

            if (!Parser.ParseArgumentsWithUsage(cmdLine, args))
            {
                Environment.Exit(-1);
            }
            else
            {
                switch (args.RunMode)
                {
                    case RunMode.Server:
                        try
                        {
                            args.Validate();

                            IPAddress ip = CommandLineArguments.ParseIP(args.IPMode, args.IPv6, args.IPAddress);
                            int port = args.PortMode == PortMode.FirstAvailable ?
                                CassiniNetworkUtils.GetAvailablePort(args.PortRangeStart, args.PortRangeEnd, ip, true) : 
                                args.Port;

                            using (var server =
                                new Server(port, args.VirtualPath, args.ApplicationPath,
                                    ip, args.HostName, args.TimeOut))
                            {
                                server.Start();
                                Console.WriteLine("started: {0}\r\nPress Enter key to exit....", server.RootUrl);
                                Console.ReadLine();
                                server.Stop();
                            }
                        }
                        catch (CassiniException ex)
                        {
                            Console.WriteLine("error:{0} {1}",
                                              ex.Field == ErrorField.None
                                                  ? ex.GetType().Name
                                                  : ex.Field.ToString(), ex.Message);
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("error:{0}", ex2.Message);
                            Console.WriteLine(Parser.ArgumentsUsage(typeof (CommandLineArguments)));
                        }
                        break;
                    case RunMode.Hostsfile:
                        SetHostsFile(args);
                        break;
                }
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