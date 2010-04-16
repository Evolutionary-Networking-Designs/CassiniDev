using System;
using System.Net;
using NUnit.Framework;

namespace Salient.Web.HttpLib.Tests
{
    [TestFixture]
    public class NUnitWebDevFixture2 : WebDevFixture
    {

        protected override int Port
        {
            get { return 12345; }
        }
        protected override string VDir
        {
            get { return "/myapp"; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {

            // the site under test will be found at http://localhost:12345/myapp
            StartServer(@"..\..\..\Salient.Web.HttpLib.TestSite");

        }

        [Test, Description("Simple Get with WebClient")]
        public void Get()
        {
            WebClient client = new WebClient();

            Uri uri = NormalizeUri("default.aspx"); // http://localhost:12345/myapp/default.aspx

            string actual = client.DownloadString(uri);
            Assert.IsTrue(actual.IndexOf("This is Default.aspx") > -1);
        }
    }
}