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
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Cassini.CommandLine;

namespace CassiniDev
{
    /// <summary>
    /// Command line arguments
    /// </summary>
    public class CommandLineArguments
    {
        [Argument(ArgumentType.AtMostOnce, ShortName = "ah", DefaultValue = false,
            HelpText = "If true add entry to Windows hosts file. Requires write permissions to hosts file.")]
        public
            bool AddHost;

        [Argument(ArgumentType.AtMostOnce, ShortName = "a", HelpText = "Physical location of content.")]
        public string
            ApplicationPath;

        [Argument(ArgumentType.AtMostOnce, ShortName = "h",
            HelpText = "Host name used for app root url. Optional unless AddHost is true.")]
        public string HostName;

        [Argument(ArgumentType.AtMostOnce, ShortName = "i",
            HelpText = "IP address to listen to. Ignored if IPMode != Specific")]
        public string IPAddress;

        [Argument(ArgumentType.AtMostOnce, ShortName = "im", DefaultValue = IPMode.Loopback, HelpText = "")]
        public
            IPMode IPMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "v6", DefaultValue = false,
            HelpText = "If IPMode 'Any' or 'LoopBack' are specified use the V6 address")]
        public bool IPv6;

        [Argument(ArgumentType.AtMostOnce, ShortName = "p",
            HelpText = "Port to listen to. Ignored if PortMode=FirstAvailable.")]
        public int Port;

        [Argument(ArgumentType.AtMostOnce, ShortName = "pm", DefaultValue = PortMode.FirstAvailable, HelpText = "")]
        public PortMode PortMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "pre", DefaultValue = (int)9000,
            HelpText = "End of port range. Ignored if PortMode != FirstAvailable")]
        public int PortRangeEnd = 9000;

        [Argument(ArgumentType.AtMostOnce, ShortName = "prs", DefaultValue = (int)8080,
            HelpText = "Start of port range. Ignored if PortMode != FirstAvailable")]
        public int PortRangeStart =
            8080;

        [DefaultArgument(ArgumentType.AtMostOnce, DefaultValue = RunMode.Server, HelpText = "[Server|Hostsfile]")]
        public RunMode RunMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "t", DefaultValue = 0,
            HelpText = "Length of time, in ms, to wait for a request before stopping the server. 0 = no timeout.")]
        public int TimeOut;

        [Argument(ArgumentType.AtMostOnce, ShortName = "v", DefaultValue = "/", HelpText = "Optional. default value '/'"
            )]
        public string VirtualPath = "/";

        [Argument(ArgumentType.AtMostOnce, ShortName = "w", DefaultValue = 0,
            HelpText =
                "Length of time, in ms, to wait for a specific port before throwing an exception or exiting. 0 = don't wait."
            )]
        public int WaitForPort;


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (RunMode != RunMode.Server)
            {
                sb.AppendFormat("{0}", RunMode);
            }
            if (!string.IsNullOrEmpty(ApplicationPath))
            {
                sb.AppendFormat(" /a:{0}", ApplicationPath);
            }
            sb.AppendFormat(" /v:{0}", VirtualPath);

            if (!string.IsNullOrEmpty(HostName))
            {
                sb.AppendFormat(" /h:{0}", HostName);
            }
            if (AddHost)
            {
                sb.Append(" /ah+");
            }

            if (IPMode != IPMode.Loopback)
            {
                sb.AppendFormat(" /im:{0}", IPMode);
            }

            if (!string.IsNullOrEmpty(IPAddress))
            {
                sb.AppendFormat(" /i:{0}", IPAddress);
            }

            if (IPv6)
            {
                sb.Append(" /v6+");
            }

            if (PortMode != PortMode.FirstAvailable)
            {
                sb.AppendFormat(" /pm:{0}", PortMode);
            }

            if (Port != 0)
            {
                sb.AppendFormat(" /p:{0}", Port);
            }

            if (PortRangeStart != 8080)
            {
                sb.AppendFormat(" /prs:{0}", PortRangeStart);
            }
            if (PortRangeEnd != 9000)
            {
                sb.AppendFormat(" /pre:{0}", PortRangeEnd);
            }
            if (TimeOut > 0)
            {
                sb.AppendFormat(" /t:{0}", TimeOut);
            }
            if (WaitForPort > 0)
            {
                sb.AppendFormat(" /w:{0}", WaitForPort);
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(ApplicationPath))
            {
                throw new CassiniException("ApplicationPath is null.", ErrorField.ApplicationPath);
            }

            if (!Directory.Exists(ApplicationPath))
            {
                throw new CassiniException("ApplicationPath does not exist.", ErrorField.ApplicationPath);
            }

            ApplicationPath = ApplicationPath.Trim('\"').TrimEnd('\\');

            if (!string.IsNullOrEmpty(VirtualPath))
            {
                VirtualPath = VirtualPath.Trim('\"');
                VirtualPath = VirtualPath.Trim('/');
                VirtualPath = "/" + VirtualPath;

            }
            else
            {
                VirtualPath = "/";
            }

            

            

            if (!VirtualPath.StartsWith("/"))
            {
                VirtualPath = "/" + VirtualPath;
            }
            
            
            if (AddHost && string.IsNullOrEmpty(HostName))
            {
                throw new CassiniException("Invalid Hostname", ErrorField.HostName);
            }

            IPAddress ip = ParseIP(IPMode, IPv6, IPAddress);

            switch (PortMode)
            {
                case PortMode.FirstAvailable:

                    if (PortRangeStart < 1)
                    {
                        throw new CassiniException("Invalid port.", ErrorField.PortRangeStart);
                    }

                    if (PortRangeEnd < 1)
                    {
                        throw new CassiniException("Invalid port.", ErrorField.PortRangeEnd);
                    }

                    if (PortRangeStart > PortRangeEnd)
                    {
                        throw new CassiniException("Port range end must be equal or greater than port range start.",
                                                   ErrorField.PortRange);
                    }
                    if (CassiniNetworkUtils.GetAvailablePort(PortRangeStart, PortRangeEnd, ip, true) == 0)
                    {
                        throw new CassiniException("No available port found.", ErrorField.PortRange);
                    }
                    break;
                case PortMode.Specific:
                    // start waiting....
                    //TODO: design this hack away.... why am I waiting in a validation method?
                    int now = Environment.TickCount;

                    // wait until either 1) the specified port is available or 2) the specified amount of time has passed
                    while (Environment.TickCount < now + WaitForPort &&
                           CassiniNetworkUtils.GetAvailablePort(Port, Port, ip, true) != Port)
                    {
                        Thread.Sleep(100);
                    }

                    // is the port available?
                    if (CassiniNetworkUtils.GetAvailablePort(Port, Port, ip, true) != Port)
                    {
                        throw new CassiniException("Port is in use.", ErrorField.Port);
                    }
                    break;
                default:
                    throw new CassiniException("Invalid PortMode", ErrorField.None);
            }
        }


        /// <summary>
        /// Converts CommandLineArgument values to an IP address if possible.
        /// Throws Exception if not.
        /// </summary>
        /// <param name="ipmode"></param>
        /// <param name="v6"></param>
        /// <param name="ipString"></param>
        /// <returns></returns>
        /// <exception cref="CassiniException">If IPMode is invalid</exception>
        /// <exception cref="CassiniException">If IPMode is 'Specific' and ipString is invalid</exception>
        public static IPAddress ParseIP(IPMode ipmode, bool v6, string ipString)
        {
            IPAddress ip;
            switch (ipmode)
            {
                case IPMode.Loopback:

                    ip = v6 ? System.Net.IPAddress.IPv6Loopback : System.Net.IPAddress.Loopback;
                    break;
                case IPMode.Any:
                    ip = v6 ? System.Net.IPAddress.IPv6Any : System.Net.IPAddress.Any;
                    break;
                case IPMode.Specific:

                    if (!System.Net.IPAddress.TryParse(ipString, out ip))
                    {
                        throw new CassiniException("Invalid IP Address", ErrorField.IPAddress);
                    }
                    break;
                default:
                    throw new CassiniException("Invalid IPMode", ErrorField.None);
            }
            return ip;
        }


    }
}