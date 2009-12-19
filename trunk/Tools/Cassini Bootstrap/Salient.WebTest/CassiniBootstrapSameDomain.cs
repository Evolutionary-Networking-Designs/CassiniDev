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
using System.IO;
using System.Net;
using Cassini;
using NUnit.Framework;

#endregion

namespace Salient.WebTest
{
    [TestFixture]
    public class CassiniBootstrapSameDomain
    {
        private Server server;


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            server = new Server(8088, "/", Environment.CurrentDirectory);
            server.Start();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            server.Stop();
            server = null;
        }

        [Test]
        public void EnsureCassiniExceptionsAreIsolated()
        {
            string ping;

            HttpWebRequest req = (HttpWebRequest) WebRequest.Create("http://localhost:8088/errorPage.aspx");

            using (Stream rstream = req.GetResponse().GetResponseStream())
            {
                ping = new StreamReader(rstream).ReadToEnd();
            }
        }

        [Test]
        public void EnsureCassiniIsRunning()
        {
            string ping;

            HttpWebRequest req = (HttpWebRequest) WebRequest.Create("http://localhost:8088/ping.txt");

            using (Stream rstream = req.GetResponse().GetResponseStream())
            {
                ping = new StreamReader(rstream).ReadToEnd();
            }

            Assert.True(ping.StartsWith("pong"));
        }
    }
}