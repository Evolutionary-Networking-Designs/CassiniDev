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
using System.Reflection;
using System.Threading;
using Cassini;
using NUnit.Framework;
using WatiN.Core;

#endregion

namespace Salient.WebTest
{
    [TestFixture]
    public class Class1
    {
        private AppDomain domain;
        private Server server;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            AppDomainSetup domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase = @"D:\Cassini-v35-Developer-3502\trunk\Cassini-v35-lib\bin\Debug\";
            domain = AppDomain.CreateDomain("MyDomain", null, domaininfo);


            server = (Server)
                     domain.CreateInstanceAndUnwrap(typeof (Server).Assembly.FullName, typeof (Server).FullName, false,
                                                    BindingFlags.Instance |
                                                    BindingFlags.Public, null,
                                                    new object[] {8088, "/", Environment.CurrentDirectory}, null, null,
                                                    null);

            server.Start();

            //Thread t = new Thread(() =>
            //                          {

            //                              server.Start();   

            //                          });
            //t.Start();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Thread.Sleep(100);
            server.Stop();
            server = null;
            AppDomain.Unload(domain);
        }


        [Test,
         ExpectedException(typeof (WebException),
             ExpectedMessage = "The remote server returned an error: (500) Internal Server Error.")]
        public void EnsureCassiniExceptionsAreIsolated()
        {
            string ping;

            HttpWebRequest req = (HttpWebRequest) WebRequest.Create("http://localhost:8088/errorPage.aspx");

            using (Stream rstream = req.GetResponse().GetResponseStream())
            {
                ping = new StreamReader(rstream).ReadToEnd();
            }
        }


        /// <summary>
        /// http://codeblog.theg2.net/2009/12/watin-20-and-gallio-31-in-x64.html
        /// </summary>
        [Test]
        public void EnsureCassiniExceptionsAreIsolatedWithWatiN()
        {
            //WatiN  not working with IE8 or ff 3.5.6 on win7x64

            // am getting results with ff 3.5 - don't let it upgrade itself

            // Open a new Internet Explorer window and
            // goto the google website.


            string url = "http://localhost:8088/errorPageButton.aspx";
            //IE ie = new IE(url);
            //ie.Button(Find.ById("Button1")).Click();

            FireFox ff = new FireFox(url);
            ff.Button(Find.ById("Button1")).Click();

            //// Find the search text field and type Watin in it.
            //ie.TextField(Find.ByName("q")).TypeText("WatiN");

            //// Click the Google search button.
            //ie.Button(Find.ByValue("Google Search")).Click();

            // Uncomment the following line if you want to close
            // Internet Explorer and the console window immediately.
            //ie.Close();
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