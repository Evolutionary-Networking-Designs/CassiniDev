using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using CassiniDev;
namespace CassiniDev.Lib.Net35.Tests
{

    [TestFixture]
    public class ChromeTest : CassiniDevBrowserTest
    {
        [TestFixtureSetUp]
        public void Start()
        {
            string content = new ContentLocator("testContent").LocateContent();
            StartServer(content);
        }
        [TestFixtureTearDown]
        public void Stop()
        {
            StopServer();
        }
        [Test]
        public void Test()
        {

            var url = NormalizeUrl("foofile.foo");
            var request = HttpWebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Assert.IsNotNull(response.Headers["x-foo-bar-header"]);
            Assert.AreEqual("value", response.Headers["x-foo-bar-header"]);
            var x = Encoding.UTF8.GetString(response.GetResponseStream().StreamToBytes());
            Assert.AreEqual("all yur base are minez", x);


        }

        [Test]
        public void Test2()
        {
            var uri = NormalizeUrl("qunit-callback.htm");

            RequestEventArgs result = RunTest(uri, WebBrowser.Chrome);
            var body = Encoding.UTF8.GetString(result.RequestLog.Body);
            Console.WriteLine(body);
        }
    }
    [TestFixture]
    public class Class1 : CassiniDevServer
    {

        [TestFixtureSetUp]
        public void Start()
        {
            string content = new ContentLocator("testContent").LocateContent();
            StartServer(content);



        }


        [TestFixtureTearDown]
        public void Stop()
        {

            StopServer();
        }
        [Test]
        public void Test()
        {


            // this is where you would build out something like service mock logic

            EventHandler<RequestInfoArgs> mockingHandler = (i, e) =>
                        {
                            e.Continue = false;
                            e.Response = "all yur base are minez";
                            e.ExtraHeaders = "x-foo-bar-header: value";
                        };
            
            

            var url = NormalizeUrl("foofile.foo");
            var request = HttpWebRequest.Create(url);

            Server.ProcessRequest += mockingHandler;

            WebResponse response = request.GetResponse();

            Server.ProcessRequest -= mockingHandler;


            Assert.IsNotNull(response.Headers["x-foo-bar-header"]);
            Assert.AreEqual("value", response.Headers["x-foo-bar-header"]);

            var x = Encoding.UTF8.GetString(response.GetResponseStream().StreamToBytes());
            Assert.AreEqual("all yur base are minez", x);


        }
        [Test]
        public void Test2()
        {

            var url = NormalizeUrl("foofile.foo");
            var request = HttpWebRequest.Create(url);
            WebResponse response = request.GetResponse();
            var x = Encoding.UTF8.GetString(response.GetResponseStream().StreamToBytes());
            Assert.AreEqual("﻿this is a foo file", x);


        }
    }
}
