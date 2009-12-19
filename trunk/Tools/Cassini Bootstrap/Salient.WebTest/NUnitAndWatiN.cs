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
using Cassini;
using NUnit.Framework;
using WatiN.Core;

#endregion

namespace Salient.WebTest
{
    [TestFixture]
    public class NUnitAndWatiN : CassiniBootStrap
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Server server = StartServer(@"E:\Projects\cassinidev\trunk\Tools\Cassini Bootstrap\TestWebApp", 8070, "/TestWebApp");

            // use for expanding relative urls
            _serverRootUrl = server.RootUrl;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            StopServer();
        }

        private string _serverRootUrl;


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

        /// <summary>
        /// http://codeblog.theg2.net/2009/12/watin-20-and-gallio-31-in-x64.html
        /// </summary>
        [Test]
        public void EnsureCassiniExceptionsAreIsolatedWithWatiN()
        {
            //ALSO: need to add configuration file to force NUnit into STA
            // due to WatiN's automation

            //WatiN  not working with IE8 or ff 3.5.6 on win7x64

            // am getting results with ff 3.5 - don't let it upgrade itself

            // Open a new Internet Explorer window and
            // goto the google website.


            //SKY

            string url = NormalizeUrl("ErrorOnClick.aspx");
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
        public void GetStaticContent()
        {
            string ping = GetHttpContent(NormalizeUrl("ping.txt"));

            Assert.True(ping.StartsWith("pong"));
        }
    }
}