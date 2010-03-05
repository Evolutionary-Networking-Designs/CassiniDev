using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using CassiniDev.FixtureExamples.NUnit.TestWCFServiceReference;
using CassiniDev.Testing;
using mshtml;
using NUnit.Framework;

namespace CassiniDev.FixtureExamples.NUnit
{
    /// <summary>
    /// All tests in one file spinning up one server.
    /// </summary>
    [TestFixture]
    public class AllTestsExternalIP : Fixture
    {
        //private Uri NormalizeUri(string relativePath)
        //{
        //    return new Uri(String.Format("http://{0}:{1}/{2}", _ip, _port,relativePath));
        //}
        /// <summary>
        /// Will fail on any but locahost/loopback - No protocol binding matches the given address.... 
        /// </summary>
        [Test, ExpectedException(typeof(WebException))]
        public void PostJsonGetJsonWithHttpRequestHelperWcfHelloWorld()
        {
            Uri uri = NormalizeUri("TestAjaxService.svc/HelloWorld");
            string json = HttpRequestHelper.AjaxTxt(uri, null, null);
            Assert.AreEqual("{\"d\":\"Hello World\"}", json);
        }

        /// <summary>
        /// Will fail on any but locahost/loopback - No protocol binding matches the given address.... 
        /// </summary>
        [Test, ExpectedException(typeof(WebException))]
        public void PostJsonGetJsonWithHttpRequestHelperWcfHelloWorldWithArgsInOut()
        {
            Uri uri = NormalizeUri("TestAjaxService.svc/HelloWorldWithArgsInOut");
            Dictionary<string, object> postData = new Dictionary<string, object>();

            // if you have a reference to the type expected you can serialize an instance
            // or just simply create an anonymous type that is shaped like the type expected...
            postData.Add("args", new { Message = "HI!" });

            string json = HttpRequestHelper.AjaxTxt(uri, postData, null);

            Assert.AreEqual("{\"d\":{\"__type\":\"HelloWorldArgs:#CassiniDev.FixtureExamples.TestWeb\",\"Message\":\"you said: HI!\"}}", json);
        }

        /// <summary>
        /// Will fail
        /// </summary>
        [Test, ExpectedException(typeof(CommunicationObjectFaultedException))]
        public void WCFWithGeneratedClientHelloWorld()
        {
            Uri uri = NormalizeUri("TestWCFService.svc");
            using (TestWCFServiceClient client = new TestWCFServiceClient(
                new WSHttpBinding(), new EndpointAddress(uri)))
            {
                client.Open();
                var result = client.HelloWorld();
                Assert.AreEqual("Hello World", result);
            }
        }

        /// <summary>
        /// Will fail
        /// </summary>
        [Test, ExpectedException(typeof(CommunicationObjectFaultedException))]
        public void WCFWithGeneratedClientHelloWorldWithArgsInOut()
        {
            Uri uri = NormalizeUri("TestWCFService.svc");
            using (TestWCFServiceClient client = new TestWCFServiceClient(
                new WSHttpBinding(), new EndpointAddress(uri)))
            {
                client.Open();
                TestWCFServiceReference.HelloWorldArgs args =
                    new CassiniDev.FixtureExamples.NUnit.TestWCFServiceReference.HelloWorldArgs();
                args.Message = "HEY";
                HelloWorldArgs result = client.HelloWorldWithArgsInOut(args);
                Assert.AreEqual("you said: HEY", result.Message);
            }
        }


        private IPAddress _ip;
        private int _port;
        /// <summary>
        /// all WCF tests will fail on any but locahost/loopback - No protocol binding matches the given address.... 
        /// User xml web service for JSON until issue is resolved
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Console.WriteLine("Setup your IP address in TestFixtureSetUp or these tests will fail.");
            var ip = IPAddress.Parse("192.168.0.103");
            StartServer(@"..\..\..\CassiniDev.FixtureExamples.TestWeb", ip, GetPort(8080, 9000, ip), "/", null, false, 10000, 15000);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            //StopServer();
        }

        /// <summary>
        /// Inline javascript execution verified.
        /// 
        /// event driven script results not verified... assertion is expecting no result
        /// please break this test if you can. lol.
        /// 
        /// If reliable JS execution is required you will need to use a UX testing framework like
        /// Watin.
        /// </summary>
        [Test]
        public void GetHtmlWithHttpRequestHelperAndParseWithMSHTMLAndVerifyJavascriptExecution()
        {
            Uri jsUri = NormalizeUri("TestJavascript.htm");

            string responseText = HttpRequestHelper.Get(jsUri, null, null);
            using (HttpRequestHelper browser = new HttpRequestHelper())
            {
                // parse the html in internet explorer
                IHTMLDocument2 doc = browser.ParseHtml(responseText);

                // get the resultant html
                string content = doc.body.outerHTML;

                Assert.IsTrue(content.IndexOf("HEY IM IN A SCRIPT") > 0, "inline script document.write not executed");
                Assert.IsTrue(content.IndexOf("this is some text from javascript") == -1,
                              "if failed then event driven script document.appendChile executed!");
            }
        }

        [Test]
        public void GetWebFormWithHttpRequestHelper()
        {
            Uri requestUri = NormalizeUri("default.aspx");
            string responseText = HttpRequestHelper.Get(requestUri, null, null);
            Assert.IsTrue(responseText.IndexOf("This is the default document of the TestWebApp") > 0);
        }

        [Test]
        public void GetWebFormWithHttpRequestHelperAndParseWithMSHTML()
        {
            Uri requestUri = NormalizeUri("default.aspx");

            string responseText = HttpRequestHelper.Get(requestUri, null, null);
            Assert.IsTrue(responseText.IndexOf("This is the default document of the TestWebApp") > 0);
            using (HttpRequestHelper browser = new HttpRequestHelper())
            {
                // parse the html in internet explorer
                IHTMLDocument2 doc = browser.ParseHtml(responseText);
                // get the resultant html
                string content = doc.body.outerHTML;
                Assert.IsTrue(content.IndexOf("This is the default document of the TestWebApp") > 0);
                IHTMLElement form = browser.FindElementByID("form1");
                Assert.IsNotNull(form);
            }
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
            HttpRequestHelper.Get(requestUri, null, cookies);

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
            HttpRequestHelper.Get(requestUri, null, cookies);

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
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(requestUri);
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

   

        [Test]
        public void XmlWebServicePostFormGetXmlHelloWorld()
        {
            Uri uri = NormalizeUri("TestWebService.asmx/HelloWorld");
            string xml = HttpRequestHelper.Post(uri, null, null);
            //
            Assert.IsTrue(xml.IndexOf("<string xmlns=\"http://tempuri.org/\">Hello World</string>") > -1);
        }

        // no corresponding call with arguments for Form/Xml
        // cannot post complex objects as form data, only intrinsic scalar types.


        [Test]
        public void XmlWebServicePostJsonGetJsonHelloWorld()
        {
            Uri uri = NormalizeUri("TestWebService.asmx/HelloWorld");
            string json = HttpRequestHelper.AjaxApp(uri, null, null);
            Assert.AreEqual("{\"d\":\"Hello World\"}", json);
        }

        [Test]
        public void XmlWebServicePostJsonGetJsonHelloWorldWithArgsInOut()
        {
            Uri uri = NormalizeUri("TestWebService.asmx/HelloWorldWithArgsInOut");
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("args", new { Message = "HI!" });
            
            
            string json = HttpRequestHelper.AjaxApp(uri, postData, null);
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<AjaxWrapper<HelloWorldArgs>>(json);
            Assert.AreEqual("you said: HI!", obj.d.Message);
        }

   
    }
    public class AjaxWrapper<T>
    {
        public T d;   
    }
}