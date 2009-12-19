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
using System.Net;
using Cassini;
using NUnit.Framework;

#endregion

namespace Salient.WebTest
{
    [TestFixture]
    public class MyCassiniBootStrap : CassiniBootStrap
    {
        private string serverRootUrl;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Server server = StartServer();

            // use for expanding relative urls
            serverRootUrl = server.RootUrl;
        }


        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            StopServer();
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// This event handler is called only when Cassini has been loaded into it's own AppDomain
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnUnhandledCassiniException(object sender, UnhandledExceptionEventArgs e)
        {
            base.OnUnhandledCassiniException(sender, e);
        }

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        // Release managed resources 
                    }
                    // Release native unmanaged resources 


                    _disposed = true;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        [Test]
        [ExpectedException(typeof (WebException),
            ExpectedMessage = "The remote server returned an error: (500) Internal Server Error.")]
        public void GenerateException_PageOnLoad()
        {
            GetHttpContent(NormalizeUrl("errorPage.aspx"));
        }

        [Test]
        public void GetStaticContent()
        {
            string ping = GetHttpContent(NormalizeUrl("ping.txt"));

            Assert.True(ping.StartsWith("pong"));
        }
    }
}