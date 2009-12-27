using System;
using System.IO;
using System.Net;

namespace Cassini.CommandLine
{
    
    /// <summary>
    /// An assembler for args that references IPUtility and validates argument values.
    /// </summary>
    public static class CassiniArgsParser
    {
        public static CassiniArgs Parse(string[] args)
        {
            
            var pargs = new CassiniArgs();
            Parser.ParseArgumentsWithUsage(args, pargs);
            ValidateAppPath(pargs);
            ValidateVRoot(pargs);
            ValidateIPAdress(pargs);
            ValidatePort(pargs);
            return pargs;
        }
        private static void ValidatePort(CassiniArgs pargs)
        {
            if (pargs.Port == 0)
            {
                // get an available port
                pargs.Port = IPUtility.GetAvailablePort(pargs.PortRangeStart , pargs.PortRangeEnd, pargs.IPAddress, pargs.ReusePort);
                pargs.IsPortDynamic = true;
            }
            else
            {
                // ensure port is open
                var port = IPUtility.GetAvailablePort(pargs.Port, pargs.Port, pargs.IPAddress, pargs.ReusePort);
                if (port != pargs.Port)
                {
                    throw new ArgumentException(string.Format("Port {0} on {1} is already in use.", pargs.Port, pargs.IPAddress),"Port");
                }
            }
        }

        private static void ValidateIPAdress(CassiniArgs args)
        {
            switch (args.IPAddressString.ToLower())
            {
                case "any":
                    args.IPAddress = IPAddress.Any;
                    break;
                case "loopback":
                    args.IPAddress = IPAddress.Loopback;
                    break;
                default:
                    args.IPAddress = IPAddress.Parse(args.IPAddressString);
                    break;
            }

            //ValidateHostName(args);

        }
        private static void ValidateAppPath(CassiniArgs args)
        {
            if (string.IsNullOrEmpty(args.AppPath))
            {
                throw new ArgumentException("", "appPath");
            }
            args.AppPath = Path.GetFullPath(args.AppPath);

            if (!Directory.Exists(args.AppPath))
            {
                throw new DirectoryNotFoundException(string.Format("", args.AppPath));
            }

            args.AppPath += args.AppPath.EndsWith("\\")?"":"\\";

        }
        private static void ValidateVRoot(CassiniArgs args)
        {
            if (!args.VirtualPath.StartsWith("/"))
            {
                args.VirtualPath = "/" + args.VirtualPath;
            }
        }
        //private static void ValidateHostName(CassiniArgs args)
        //{
        //    if (string.IsNullOrEmpty(args.HostName))
        //    {
        //        // if the ip is loopback, we should use 127.0.0.1. to enable fiddler to capture traffic
        //        if (IPAddress.IsLoopback(args.IPAddress))
        //        {
        //            args.HostName = "127.0.0.1.";
        //        }
        //        else
        //        {
        //            args.HostName = args.IPAddress.ToString();
        //        }
        //    }
        //}
    }
}