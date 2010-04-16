﻿using System;
using System.Net;
using NUnit.Framework;

namespace Salient.Web.HttpLib.Tests
{
    [TestFixture]
    public class NUnitWebDevFixture : WebDevFixture
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // default WebDevFixture port = 9999
            // default WebDevFixture vdir = '/'
            // thus the site under test will be found at http://localhost:9999/

            // the path to the website under test can be absolute or relative

            StartServer(@"..\..\..\Salient.Web.HttpLib.TestSite");

        }

        [Test, Description("Simple Get with WebClient")]
        public void Get()
        {
            WebClient client = new WebClient();
            Uri uri = NormalizeUri("default.aspx"); // http://localhost:9999/default.aspx
            
            string actual = client.DownloadString(uri);
            Assert.IsTrue(actual.IndexOf("This is Default.aspx") > -1);
        }
    }
}
