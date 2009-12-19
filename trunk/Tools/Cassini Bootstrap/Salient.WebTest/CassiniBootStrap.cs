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

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Cassini;

#endregion

namespace Salient.WebTest
{
    /// <summary>
    /// <para>Provides a configurable bootstrap for a local instance of Cassini 3.5.</para>
    /// <para>Typically used for integration, acceptance and smoke testing of asp.net/ajax/wcf.</para>
    /// <para>Specifying a tighter coupling of the Cassini instance can allow simultaneous testing 
    /// and analysis from both client and server perspectives.</para>
    /// </summary>
    public class CassiniBootStrap : IDisposable
    {
        #region Helper Methods

        public static JavaScriptSerializer JavaScriptSerializer = new JavaScriptSerializer();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public virtual string NormalizeUrl(string relativeUrl)
        {
            //quick and dirty
            string serverRootUrl = _server.RootUrl;
            if(!serverRootUrl.EndsWith("/"))
            {
                serverRootUrl  += "/";
            }
            return serverRootUrl + relativeUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public virtual string GetHttpContent(string url)
        {
            string response;

            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);

            using (Stream rstream = req.GetResponse().GetResponseStream())
            {
                response = new StreamReader(rstream).ReadToEnd();
            }

            return response;
        }

        public string CallJson(string requestUrl)
        {
            return CallJson(requestUrl, null);
        }

        /// <summary>
        /// Performs a webscript (JSON) post to a service
        /// Does simple serialization of arguments. Complex objects
        /// with circular references will not pass. This includes Entities.
        /// I believe it can handle anonymous types. TODO:test anonymous types
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string CallJson(string requestUrl, Dictionary<string, object> args)
        {
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(requestUrl);

            // set the headers
            // TODO: provide built in useragent options
            req.Method = "POST";
            req.Headers.Add("Pragma", "no-cache");
            req.Headers.Add("Cache-Control", "no-cache");
            req.ContentType = "application/json; charset=UTF-8";

            // handle the body of the post
            //var bodyTest = JsonConvert.SerializeObject(args);
            if (args != null)
            {
                string bodyTest = JavaScriptSerializer.Serialize(args);
                byte[] body = Encoding.UTF8.GetBytes(bodyTest);
                req.ContentLength = body.Length;

                // add it to the request stream
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(body, 0, body.Length);
                }
            }

            string responseText;

            // send the request and get the response
            using (WebResponse response = req.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                responseText = reader.ReadToEnd();
                responseStream.Close();
                response.Close();
            }

            return responseText;
        }

        #endregion

        #region backers

        private bool _disposed;
        private Server _server;
        private AppDomain _serverAppDomain;

        #endregion

        #region Cassini

        /// <summary>
        /// NOT IMPLEMENTED: Call on unhandled exceptions in the Cassini AppDomain
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnUnhandledCassiniException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        /// <summary>
        /// <para>Starts an instance of Cassini Web Server 3.5 with the specified parameters.</para>
        /// <para>Default values:  physicalPath:Environment.CurrentDirectory, portNumber:1968,
        ///  virtualPath:'/', useOwnAppDomain:true</para>
        /// </summary>
        /// <returns>A running instance of Cassini.Server</returns>
        internal Server StartServer()
        {
            return StartServer(Environment.CurrentDirectory);
        }

        /// <summary>
        /// <para>Starts an instance of Cassini Web Server 3.5 with the specified parameters.</para>
        /// <para>Default values:  physicalPath:Environment.CurrentDirectory, portNumber:1968,
        ///  virtualPath:'/', useOwnAppDomain:true</para>
        /// </summary>
        /// <param name="physicalPath">The physical path of the content to be served. Default: the current directory.</param>
        /// <returns>A running instance of Cassini.Server</returns>
        internal Server StartServer(string physicalPath)
        {
            return StartServer(physicalPath, 1968);
        }

        /// <summary>
        /// <para>Starts an instance of Cassini Web Server 3.5 with the specified parameters.</para>
        /// <para>Default values:  physicalPath:Environment.CurrentDirectory, portNumber:1968,
        ///  virtualPath:'/', useOwnAppDomain:true</para>
        /// </summary>
        /// <param name="physicalPath">The physical path of the content to be served. Default: the current directory.</param>
        /// <param name="portNumber">The local port assigned to this server. Default: 1968</param>
        /// <returns>A running instance of Cassini.Server</returns>
        internal Server StartServer(string physicalPath, int portNumber)
        {
            return StartServer(physicalPath, portNumber, "/");
        }


        /// <summary>
        /// <para>Starts an instance of Cassini Web Server 3.5 with the specified parameters.</para>
        /// <para>Default values:  physicalPath:Environment.CurrentDirectory, portNumber:1968,
        ///  virtualPath:'/', useOwnAppDomain:true</para>
        /// </summary>
        /// <param name="physicalPath">The physical path of the content to be served. Default: the current directory.</param>
        /// <param name="portNumber">The local port assigned to this server. Default: 1968</param>
        /// <param name="virtualPath"><para>The virtual root of the server. Default: '/'</para></param>
        /// <returns>A running instance of Cassini.Server</returns>
        internal Server StartServer(string physicalPath, int portNumber, string virtualPath)//, bool useOwnAppDomain)
        {
            StopServer();


            //if (useOwnAppDomain)
            //{
            //    AppDomainSetup domaininfo = new AppDomainSetup();
            //    domaininfo.ApplicationBase = Environment.CurrentDirectory;
            //    // @"D:\Cassini-v35-Developer-3502\trunk\Cassini-v35-lib\bin\Debug\";
            //    _serverAppDomain = AppDomain.CreateDomain("HostDomain", null, domaininfo);

            //    //TODO: actually take the time to understand the details of this and why it isn't working.
            //    _serverAppDomain.UnhandledException += new UnhandledExceptionEventHandler(_serverAppDomain_UnhandledException);


            //    string serverAssemblyName = typeof (Server).Assembly.FullName;
            //    string serverTypeName = typeof (Server).FullName;
            //    object[] serverConstructorArgs = new object[] {portNumber, virtualPath, physicalPath};

            //    _server = (Server)
            //              _serverAppDomain.CreateInstanceAndUnwrap(serverAssemblyName, serverTypeName, false,
            //                                                       BindingFlags.Instance |
            //                                                       BindingFlags.Public, null, serverConstructorArgs,
            //                                                       null,
            //                                                       null, null);
                
            //}
            //else
            //{
            //    _server = new Server(portNumber, virtualPath, physicalPath);
            //}
            _server = new Server(portNumber, virtualPath, physicalPath);
            _server.Start();

            return _server;
        }

        private void _serverAppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            OnUnhandledCassiniException(sender, e);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void StopServer()
        {
            StopServer(100);
        }

        /// <summary>
        /// <para>Stops the server, if running and unloads the AppDomain if one  has been created.</para>
        /// <para>Neglecting to stop the server may result in zombie ports that will break automation.</para>
        /// 
        /// </summary>
        /// <param name="delay">delay before stopping the server. Allows for requests to complete. Default:100ms</param>
        protected void StopServer(int delay)
        {
            Thread.Sleep(delay);
            if (_server != null)
            {
                try
                {
                    _server.Stop();
                    _server = null;
                }
                catch
                {
                }
            }
            if (_serverAppDomain != null)
            {
                try
                {
                    AppDomain.Unload(_serverAppDomain);
                    _serverAppDomain = null;
                }
                catch
                {
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_server != null)
                    {
                        StopServer();
                    }
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// Failsafe disposal
        /// </summary>
        ~CassiniBootStrap()
        {
            Dispose(false);
        }

        #endregion
    }
}