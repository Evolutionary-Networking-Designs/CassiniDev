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
using System.Threading;
using CassiniDev;

namespace Cassini
{
    /// <summary>
    /// 
    /// Please put all but the most trivial changes and all additions to Server in partial files to
    /// reduce the code churn and pain of merging new releases of Cassini. If a method is to be significantly modified,
    /// comment it out, explain the modification/move in the header and put the modified version in this file.
    /// 
    /// 12/29/09 sky: Implemented IDisposable to help eliminate zombie ports
    /// 12/29/09 sky: Added instance properties for HostName and IPAddress and constructor to support them
    /// 12/29/09 sky: Extracted and implemented IServer interface to facilitate stubbing for tests
    /// 
    /// </summary>
    public partial class Server : IServer
    {
        private bool _disposed;
        private string _hostName;
        private IPAddress _ipAddress;

        public Server(ServerArguments args)
            : this(args.Port, args.VirtualPath, args.ApplicationPath)
        {
            _ipAddress = args.IPAddress;
            _hostName = args.Hostname;
        }

        #region IServer Members

        public string HostName
        {
            get { return _hostName; }
        }

        public IPAddress IPAddress
        {
            get { return _ipAddress; }
        }

        #endregion

        #region IDisposable

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
                    // just add a little slack for the socket transition to TIME_WAIT
                    Thread.Sleep(10);
                }
            }
            _disposed = true;
        }

        ~Server()
        {
            Dispose(false);
        }

        #endregion
    }
}