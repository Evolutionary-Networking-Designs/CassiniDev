// /*!
//  * Project: Salient.Web.HttpLib
//  * http://salient.codeplex.com
//  *
//  * Copyright 2010, Sky Sanders
//  * Dual licensed under the MIT or GPL Version 2 licenses.
//  * http://salient.codeplex.com/license
//  *
//  * Date: April 11 2010 
//  */

#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;

#endregion

namespace Salient.Web.HttpLib
{
    /// <summary>
    /// A general purpose Visual Studio 2008 Development Server (WebDev.WebServer.exe) test fixture
    /// </summary>
    public abstract class WebDevFixture
    {
        private const string WebDevPathx64 =
            @"C:\Program Files (x86)\Common Files\Microsoft Shared\DevServer\9.0\WebDev.WebServer.exe";

        private const string WebDevPathx86 =
            @"C:\Program Files\Common Files\Microsoft Shared\DevServer\9.0\WebDev.WebServer.exe";


        private readonly int _port = 9999;
        private readonly string _vdir = "/";

        protected WebDevFixture()
        {
            // set testing port - default 9999 - can be changed by adding an appSettings 'port'
            // note: appSetting is optional, will just default to 9999
            int tmpPort;
            _port = int.TryParse(ConfigurationManager.AppSettings["port"], out tmpPort) ? tmpPort : _port;

            // set vdir - default '/' - can be changed by adding an appSettings 'vdir'
            // note: appSetting is optional, will just default to '/'

            _vdir = ConfigurationManager.AppSettings["vdir"] ?? "/";

            // do some munging to ensure that the vdir is going to work out for us
            _vdir = string.Format(CultureInfo.InvariantCulture, "/{0}/", _vdir.Trim('/')).Replace("//", "/");
        }

        // these can optionally be overriden in app.config using similarly named appSettings. e.g. 'port' and 'vdir'
        // or overridden by derived class
        protected virtual int Port
        {
            get { return _port; }
        }

        protected virtual string VDir
        {
            get { return _vdir; }
        }

        /// <summary>
        /// Returns an absolute Uri rooted on the running server.
        /// e.g. NormalizeUrl("default.aspx") will return 'http://localhost:9999/default.aspx'
        /// </summary>
        /// <param name="pathAndQuery">path and query relative to app vdir</param>
        /// <returns></returns>
        protected Uri NormalizeUri(string pathAndQuery)
        {
            // do some munging to ensure that the vdir is going to work out for us
            string vdir = string.Format(CultureInfo.InvariantCulture, "/{0}/", VDir.Trim('/')).Replace("//", "/");
            return
                new Uri(string.Format(CultureInfo.InvariantCulture, "http://localhost:{0}{1}{2}", Port, vdir, pathAndQuery));
        }

        /// <summary>
        /// Ensure that an instance of WebDev.WebServer.exe is running and serving the target site. 
        /// </summary>
        /// <param name="sitePath">path of the website to serve, can be absolute or relative</param>
        protected void StartServer(string sitePath)
        {
            sitePath = Path.GetFullPath(sitePath);

            // try x86 first
            string webDevPath = WebDevPathx86;
            if (!File.Exists(webDevPath))
            {
                // then x64
                webDevPath = WebDevPathx64;
                if (!File.Exists(webDevPath))
                {
                    throw new FileNotFoundException("Cannot find WebDev.WebServer.exe");
                }
            }

            string devServerCmdLine = String.Format(CultureInfo.InvariantCulture,
                                                    "/port:{0} /path:\"{1}\" /vpath:\"{2}\"", Port, sitePath,
                                                    VDir);

            // check to see if we already have a server running
            bool running = false;
            foreach (string cmdLine in GetCommandLines("WebDev.WebServer.exe"))
            {
                if (cmdLine.EndsWith(devServerCmdLine, StringComparison.OrdinalIgnoreCase))
                {
                    running = true;
                    break;
                }
            }

            if (!running)
            {
                Process.Start(webDevPath, devServerCmdLine);
            }
        }

        /// <summary>
        /// Using WMI to fetch the command line that started all instances of a process
        /// </summary>
        /// <param name="processName">Image name, e.g. WebDev.WebServer.exe</param>
        /// <returns></returns>
        /// adapted from http://stackoverflow.com/questions/504208/how-to-read-command-line-arguments-of-another-process-in-c/504378%23504378
        /// original code by http://stackoverflow.com/users/61396/xcud
        private static IEnumerable<string> GetCommandLines(string processName)
        {
            List<string> results = new List<string>();

            string wmiQuery = string.Format(CultureInfo.InvariantCulture,
                                            "select CommandLine from Win32_Process where Name='{0}'", processName);

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery))
            {
                using (ManagementObjectCollection retObjectCollection = searcher.Get())
                {
                    foreach (ManagementObject retObject in retObjectCollection)
                    {
                        results.Add((string) retObject["CommandLine"]);
                    }
                }
            }
            return results;
        }
    }
}