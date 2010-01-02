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
using System.Net;
using Cassini.CommandLine;

namespace CassiniDev
{
    /// <summary>
    /// Server Constructor arguments
    /// </summary>
    public class ServerArguments
    {
        public ushort Port { get; set; }
        public string VirtualPath { get; set; }
        public string ApplicationPath { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
    }


    /// <summary>
    /// Command line arguments
    /// </summary>
    public class CommandLineArguments  
    {
        [DefaultArgument(ArgumentType.AtMostOnce, DefaultValue = RunMode.Server, HelpText="[Server|Hostsfile]")]
        public RunMode RunMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "a")]
        public string ApplicationPath;

        [Argument(ArgumentType.AtMostOnce, ShortName = "v", DefaultValue = "/")]
        public string VirtualPath="/";

        [Argument(ArgumentType.AtMostOnce, ShortName = "h")]
        public string HostName;

        [Argument(ArgumentType.AtMostOnce, ShortName = "ah", DefaultValue = false)]
        public bool AddHost;

        [Argument(ArgumentType.AtMostOnce, ShortName = "im", DefaultValue = IPMode.Loopback)]
        public IPMode IPMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "i", HelpText = "IP address or constants 'any','loopback',", DefaultValue = "loopback")]
        public string IPAddress = "loopback";

        [Argument(ArgumentType.AtMostOnce, ShortName = "v6", DefaultValue = false)]
        public bool IPv6;

        [Argument(ArgumentType.AtMostOnce, ShortName = "pm", DefaultValue = PortMode.FirstAvailable)]
        public PortMode PortMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "p")]
        public ushort Port;

        [Argument(ArgumentType.AtMostOnce, ShortName = "pre", DefaultValue = (ushort)9000)]
        public ushort PortRangeEnd=9000;

        [Argument(ArgumentType.AtMostOnce, ShortName = "prs", DefaultValue = (ushort)8080)]
        public ushort PortRangeStart=8080;
        
        

   
    }
}