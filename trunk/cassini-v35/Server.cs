/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License (Ms-PL). A copy of the license can be found in the license.htm file
 * included in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Cassini
{
    /// <summary>
    /// 
    /// </summary>
    /// <changes>
    /// 12/20/09 - sky: changed visibility from internal to public
    /// 12/21/09 - sky: * added IDisposable implementation
    ///                 * set socketoptions to reuse time_wait sockets enabling if flag is set- 
    ///                 rapid buildup and teardown of server instances for testing purposes
    ///                 * added constructors 
    ///                 * changed CreateSocketBindAndListen to instance method to allow using instance flag
    /// 
    /// 12/23/09 - sky: * implemented more robust command line argument parser
    ///                 * requires fx 3.5
    /// 
    /// 12/24/09 - sky: * Implemented arbitrary IP address
    ///                 * removed v6 loopback support temporarily
    ///                 *
    ///                 *
    /// 
    /// TODO: 
    ///   * implement programmatic addition of host entry for this ip:port and hostname for lifetime of server
    ///     quickest hack is to write to host file but requires permissions.
    ///   
    /// 
    /// </changes>
    public class Server : MarshalByRefObject, IDisposable
    {
        readonly string _virtualPath = "/";
        readonly string _physicalPath = Environment.CurrentDirectory;
        readonly bool _reuseConnection = false;
        readonly IPAddress _ipAddress = IPAddress.Loopback;
        readonly int _port = 0;
        private readonly string _hostname;
        bool _shutdownInProgress;
        Socket _socket;
        Host _host;
 
        public Server(int port, string virtualPath, string physicalPath, bool reuseConnection, IPAddress ip, string hostName)
        {
            _port = port;
            _virtualPath = virtualPath;
            _physicalPath = physicalPath.EndsWith("\\", StringComparison.Ordinal) ? physicalPath : physicalPath + "\\";
            _reuseConnection = reuseConnection;
            _ipAddress = ip;
            _hostname = hostName;
        }
        public Server(int port, string virtualPath, string physicalPath, bool reuseConnection)
            : this(port, virtualPath, physicalPath, reuseConnection, IPAddress.Loopback, null)
        {

        }

        public Server(int port, string virtualPath, string physicalPath)
            : this(port, virtualPath, physicalPath, false)
        {

        }

        public override object InitializeLifetimeService()
        {
            // never expire the license
            return null;
        }

        
        public string HostName
        {
            get
            {
                return _hostname;
            }
        }
        public IPAddress IPAddress
        {
            get
            {
                return _ipAddress;
            }
        }
        public string VirtualPath
        {
            get
            {
                return _virtualPath;
            }
        }

        public string PhysicalPath
        {
            get
            {
                return _physicalPath;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// 12/21/09 sky: * changed localhost to 127.0.0.1 and added '.' to localhost to enable fiddler to pick up traffic
        ///               see http://www.west-wind.com/weblog/posts/596348.aspx
        /// 12/24/09 sky: * added support for arbitrary IP
        /// </summary>
        public string RootUrl
        {
            get
            {
                string hostname = _hostname ?? _ipAddress.ToString();

                // allow fiddler to capture traffic.
                // TODO: programmatically add host entry 

                if (hostname == IPAddress.Loopback.ToString())
                {
                    hostname += ".";
                }

                if (_port != 80)
                {
                    return "http://" + hostname + ":" + _port + _virtualPath;
                }
                else
                {
                    return "http://" + hostname + "." + _virtualPath;
                }
            }
        }



        /// <summary>
        /// Socket listening
        /// 12/24/09 sky: * added support for arbitrary IP
        /// </summary>
        /// <param name="family"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private Socket CreateSocketBindAndListen(AddressFamily family, IPAddress address, int port)
        {
            var socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
            //SKY: in a testing scenario we may be creating and disposing many servers in rapid
            // succession. Default socket behaviour will send the closed socket into the TIME_WAIT
            // state, a sort of cooldown period, making it unavaible. By setting ReuseAddress we are telling winsock to accept the
            // binding event if the socket is in TIME_WAIT. 
            
            try
            {
                if (_reuseConnection)
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }

                socket.Bind(new IPEndPoint(address, port));
                socket.Listen((int)SocketOptionName.MaxConnections);
                return socket;
                
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, "port", ex);                
            }
        }

        /// <summary>
        /// 12/24/09 sky: * added support for arbitrary IP
        ///               * removed try/catch, implicit rollover is just a bad idea
        /// </summary>
        public void Start()
        {

            _socket = CreateSocketBindAndListen(AddressFamily.InterNetwork, _ipAddress, _port);

            ThreadPool.QueueUserWorkItem(delegate
            {
                while (!_shutdownInProgress)
                {
                    try
                    {
                        Socket acceptedSocket = _socket.Accept();

                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            if (!_shutdownInProgress)
                            {
                                var conn = new Connection(this, acceptedSocket);

                                // wait for at least some input
                                if (conn.WaitForRequestBytes() == 0)
                                {
                                    conn.WriteErrorAndClose(400, "missing request");
                                    return;
                                }

                                // find or create host
                                Host host = GetHost();
                                if (host == null)
                                {
                                    conn.WriteErrorAndClose(500, "could not find or create host");
                                    return;
                                }

                                // process request in worker app domain
                                host.ProcessRequest(conn);
                            }
                        });
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            _shutdownInProgress = true;

            try
            {
                if (_socket != null)
                {
                    _socket.Close();
                }
            }
            catch
            {
            }
            finally
            {
                _socket = null;
                Thread.Sleep(10);
            }

            try
            {
                if (_host != null)
                {
                    _host.Shutdown();
                }

                while (_host != null)
                {
                    Thread.Sleep(100);
                }
            }
            catch
            {
            }
            finally
            {
                _host = null;
            }
        }

        /// <summary>
        /// called at the end of request processing to disconnect the remoting
        /// proxy for Connection object and allow GC to pick it up
        /// </summary>
        /// <param name="conn"></param>
        public void OnRequestEnd(Connection conn)
        {
            RemotingServices.Disconnect(conn);
        }

        public void HostStopped()
        {
            _host = null;
        }

        Host GetHost()
        {
            if (_shutdownInProgress)
                return null;

            Host host = _host;

            if (host == null)
            {
                lock (this)
                {
                    host = _host;
                    if (host == null)
                    {
                        host = (Host)CreateWorkerAppDomainWithHost(_virtualPath, _physicalPath, typeof(Host));
                        host.Configure(this, _port, _virtualPath, _physicalPath);
                        _host = host;
                    }
                }
            }

            return host;
        }

        static object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType)
        {
            // this creates worker app domain in a way that host doesn't need to be in GAC or bin
            // using BuildManagerHost via private reflection
            string uniqueAppString = string.Concat(virtualPath, physicalPath).ToLowerInvariant();
            string appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);

            // create BuildManagerHost in the worker app domain
            var appManager = ApplicationManager.GetApplicationManager();
            var buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
            var buildManagerHost = appManager.CreateObject(appId, buildManagerHostType, virtualPath, physicalPath, false);

            // call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
            buildManagerHostType.InvokeMember(
                "RegisterAssembly",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null,
                buildManagerHost,
                new object[2] { hostType.Assembly.FullName, hostType.Assembly.Location });

            // create Host in the worker app domain
            return appManager.CreateObject(appId, hostType, virtualPath, physicalPath, false);
        }


        // implement disposable to help eliminate zombie ports
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                }
            }
            _disposed = true;
        }

        ~Server()
        {
            Dispose(false);
        }
    }
}
