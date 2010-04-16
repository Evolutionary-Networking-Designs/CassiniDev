// /*!
//  * Project: Salient.Web.HttpLib
//  * http://salient.codeplex.com
//  *
//  * Copyright 2010, Sky Sanders
//  * Dual licensed under the MIT or GPL Version 2 licenses.
//  * http://salient.codeplex.com/license
//  *
//  * Date: April 11 2010 
//  */

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using CassiniDev.FixtureExamples.NUnit.TestWCFServiceReference;
using CassiniDev.Testing;
using NUnit.Framework;
using Salient.Web.HttpLib;

#endregion

namespace CassiniDev.FixtureExamples.NUnit
{
    /// <summary>
    /// All tests in one file spinning up one server.
    /// </summary>
    [TestFixture]
    public class NewAllTestsLocalHost : Fixture
    {
        /// <summary>
        /// WCF AJAX JSON tests will fail on any but locahost/loopback - No protocol binding matches the given address.... 
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            StartServer(@"..\..\..\CassiniDev.FixtureExamples.TestWeb", IPAddress.Loopback,
                        GetPort(8080, 9000, IPAddress.Loopback), "/", null, false, 10000, 15000);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            StopServer();
        }


        [Test]
        public void GetWebFormWithHttpRequestHelper()
        {
            Uri requestUri = NormalizeUri("default.aspx");

            string responseText = RequestFactory.CreateGet(requestUri).GetResponse().GetResponseStream().Text();
            Assert.IsTrue(responseText.IndexOf("This is the default document of the TestWebApp") > 0);
        }


        /// <summary>
        /// Demonstrate the use of cookies across one or more http requests.
        /// All request methods on HttpRequestHelper can accept a CookieContainer...
        /// e.g. can use cookies with WebForms, Html, WCF and XML services....
        /// </summary>
        [Test]
        public void GetWebFormWithHttpRequestHelperWithCookies()
        {
            // pass the same instance of CookieContainer to each request
            // cookies (session id, membership cache etc) are read from and written to
            CookieContainer cookies = new CookieContainer();

            // page that sets a cookie on the response
            Uri requestUri = NormalizeUri("TestWebFormCookies1.aspx");

            // pass the cookie container with the requestUri
            RequestFactory.CreateGet(requestUri, null, cookies).GetResponse();

            // get the cookies for this path from the container
            CookieCollection mycookies = cookies.GetCookies(requestUri);

            // find our cookie and test it
            Cookie cookie = mycookies["Cooookie"];
            Assert.IsNotNull(cookie);
            Assert.AreEqual("TestWebFormCookies1", cookie.Value);

            // page that reads cookie from request and ensures is present and
            // has expected value then writes it back out to the response with
            // a different value.
            requestUri = NormalizeUri("TestWebFormCookies2.aspx");

            // just pass the same CookieContainer
            RequestFactory.CreateGet(requestUri, null, cookies).GetResponse();

            //....
            mycookies = cookies.GetCookies(requestUri);
            cookie = mycookies["Cooookie"];
            Assert.IsNotNull(cookie);

            // bingo! 
            Assert.AreEqual("TestWebFormCookies2", cookie.Value);

            // now you can maintain state across requests
        }


        /// <summary>
        /// Just to demonstrate manual basic fetching - recommend using HttpRequestHelper
        /// </summary>
        [Test]
        public void GetWebFormWithHttpWebRequest()
        {
            Uri requestUri = NormalizeUri("default.aspx");
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(requestUri);
            string responseText;
            using (WebResponse response = req.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                responseText = reader.ReadToEnd();
                responseStream.Close();
                response.Close();
            }
            Assert.IsTrue(responseText.IndexOf("This is the default document of the TestWebApp") > 0);
        }

        /// <summary>
        /// Will fail on any but locahost/loopback - No protocol binding matches the given address.... 
        /// </summary>
        [Test]
        public void PostJsonGetJsonWithHttpRequestHelperWcfHelloWorld()
        {
            Uri uri = NormalizeUri("TestAjaxService.svc/HelloWorld");
            string json = RequestFactory.CreatePostJsonText(uri).GetResponse().GetResponseStream().Text();
            Assert.AreEqual("{\"d\":\"Hello World\"}", json);
        }

        /// <summary>
        /// Will fail on any but locahost/loopback - No protocol binding matches the given address.... 
        /// </summary>
        [Test]
        public void PostJsonGetJsonWithHttpRequestHelperWcfHelloWorldWithArgsInOut()
        {
            Uri uri = NormalizeUri("TestAjaxService.svc/HelloWorldWithArgsInOut");
            Dictionary<string, object> postData = new Dictionary<string, object>();

            // if you have a reference to the type expected you can serialize an instance
            // or just simply create an anonymous type that is shaped like the type expected...
            postData.Add("args", new {Message = "HI!"});

            string json = RequestFactory.CreatePostJsonText(uri, postData).GetResponse().GetResponseStream().Text();

            Assert.AreEqual(
                "{\"d\":{\"__type\":\"HelloWorldArgs:#CassiniDev.FixtureExamples.TestWeb\",\"Message\":\"you said: HI!\"}}",
                json);
        }

        /// <summary>
        /// Passes sometimes ? timeout?
        /// </summary>
        [Test]
        public void WCFWithGeneratedClientHelloWorld()
        {
            Uri uri = NormalizeUri("TestWCFService.svc");
            using (TestWCFServiceClient client = new TestWCFServiceClient(
                new WSHttpBinding(), new EndpointAddress(uri)))
            {
                client.Open();
                string result = client.HelloWorld();
                Assert.AreEqual("Hello World", result);
            }
        }

        /// <summary>
        /// Passes sometimes ? timeout?
        /// </summary>
        [Test]
        public void WCFWithGeneratedClientHelloWorldWithArgsInOut()
        {
            Uri uri = NormalizeUri("TestWCFService.svc");
            using (TestWCFServiceClient client = new TestWCFServiceClient(
                new WSHttpBinding(), new EndpointAddress(uri)))
            {
                client.Open();
                HelloWorldArgs args = new HelloWorldArgs {Message = "HEY"};
                HelloWorldArgs result = client.HelloWorldWithArgsInOut(args);
                Assert.AreEqual("you said: HEY", result.Message);
            }
        }

        [Test]
        public void XmlWebServicePostFormGetXmlHelloWorld()
        {
            Uri uri = NormalizeUri("TestWebService.asmx/HelloWorld");
            string xml = RequestFactory.CreatePostForm(uri).GetResponse().GetResponseStream().Text();

            Assert.IsTrue(xml.IndexOf("<string xmlns=\"http://tempuri.org/\">Hello World</string>") > -1);
        }

        // no corresponding call with arguments for Form/Xml
        // cannot post complex objects as form data, only intrinsic scalar types.


        [Test]
        public void XmlWebServicePostJsonGetJsonHelloWorld()
        {
            Uri uri = NormalizeUri("TestWebService.asmx/HelloWorld");
            string json = RequestFactory.CreatePostJsonApp(uri).GetResponse().GetResponseStream().Text();
            Assert.AreEqual("{\"d\":\"Hello World\"}", json);
        }

        [Test]
        public void XmlWebServicePostJsonGetJsonHelloWorldWithArgsInOut()
        {
            Uri uri = NormalizeUri("TestWebService.asmx/HelloWorldWithArgsInOut");
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("args", new {Message = "HI!"});
            string json = RequestFactory.CreatePostJsonApp(uri, postData).GetResponse().GetResponseStream().Text();
            Assert.AreEqual(
                "{\"d\":{\"__type\":\"CassiniDev.FixtureExamples.TestWeb.HelloWorldArgs\",\"Message\":\"you said: HI!\"}}",
                json);
        }
    }
}