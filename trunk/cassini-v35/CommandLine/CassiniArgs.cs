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

namespace Cassini.CommandLine
{
    /// <summary>
    /// appath allows relative paths
    /// </summary>
    public class CassiniArgs
    {
        [Argument(ArgumentType.AtMostOnce,
            HelpText = "The physical location of the root directory. Default is the current path.", ShortName = "a",
            LongName = "apppath")] 
        public string AppPath;

        [Argument(ArgumentType.AtMostOnce,
            HelpText = "The hostname of the site. Informational only. Used to create Server.RootUrl. If ommited DNS will be used to resolve hostname.",
            ShortName = "h", LongName = "hostname")] 
        public string HostName;

        [Argument(ArgumentType.AtMostOnce,
            HelpText = "IP address of adapter to listen on. Possible values: any, loopback, IPv4 string",
            DefaultValue = "loopback", ShortName = "i", LongName = "ipaddress")] 
        public string IPAddressString="loopback";

        public IPAddress IPAddress;
        

        [Argument(ArgumentType.AtMostOnce,
            HelpText = "The port to listen on. If ommitted the first available port > 8080 will be used unless port range is specified.",
            DefaultValue = (ushort)0, ShortName = "p", LongName = "port")]
        public ushort Port;

        public bool IsPortDynamic;

        [Argument(ArgumentType.AtMostOnce, HelpText = "Find first available port between portrangestart and this value."
            , DefaultValue = (ushort)65534, ShortName = "e", LongName = "portrangeend")]
        public ushort PortRangeEnd;

        [Argument(ArgumentType.AtMostOnce, HelpText = "Find first available port between this value and portrangeend.",
            DefaultValue = (ushort)8080, ShortName = "s", LongName = "portrangestart")]
        public ushort PortRangeStart;

        [Argument(ArgumentType.AtMostOnce, HelpText = "Allow reuse of port in TIME_WAIT state.", DefaultValue = false,
            ShortName = "r", LongName = "reuse")] 
        public bool ReusePort;

        [Argument(ArgumentType.AtMostOnce, HelpText = "The virtual root of the application.", DefaultValue = "/",
            ShortName = "v",LongName="vpath")] 
        public string VirtualPath;


 
    }
}