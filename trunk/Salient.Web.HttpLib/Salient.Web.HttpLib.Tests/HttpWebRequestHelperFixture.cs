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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;
using Salient.Web.HttpLib.TestSite;

#endregion

namespace Salient.Web.HttpLib.Tests
{
    /// <summary>
    /// Exercises RequestFactory by simulating various types of XmlHttpRequest requests. 
    /// 
    /// Expected testing behavior is to be considered a functional test of WebDevFixture.
    /// </summary>
    [TestFixture]
    public class RequestFactoryFixture : WebDevFixture
    {
        private string _nameValue;
        private DateTime _dateValue;
        private Uri _postHandlerUri;
        private Uri _defaultUri;
        private static readonly JavaScriptSerializer Jsr = new JavaScriptSerializer();



        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // set some default values for test params
            _nameValue = "NameValue";
            // keep in mind that all dates serialized to JSON are UTC
            _dateValue = new DateTime(1999, 1, 1).ToUniversalTime();
            _postHandlerUri = NormalizeUri("PostHandler.ashx");
            _defaultUri = NormalizeUri("default.aspx");


            // default WebDevFixture port = 9999
            // default WebDevFixture vdir = '/'
            // thus the site under test will be found at http://localhost:9999/

            // the path to the website under test can be absolute or relative

            StartServer(@"..\..\..\Salient.Web.HttpLib.TestSite");
        }

        [Test, Description("")]
        public void DownloadFile()
        {
            string actual;

            using (var response = RequestFactory.CreateRequest(_defaultUri, HttpMethod.Get, ContentType.None, null, null, null).GetResponse().GetResponseStream())
            {
                actual = Encoding.UTF8.GetString(response.Bytes());
            }
            Assert.IsTrue(actual.IndexOf("This is Default.aspx") > -1);
        }

        [Test, Description("")]
        public void Get()
        {
            string responseText;
            using (Stream response = RequestFactory.CreateRequest(_defaultUri, HttpMethod.Get, ContentType.None, null, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("This is Default.aspx") > -1);
        }

        [Test, Description("")]
        public void GetOverLoad()
        {
            string responseText;
            using (Stream response = RequestFactory.CreateGet(_defaultUri).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("This is Default.aspx") > -1);
        }

        [Test, Description("")]
        public void GetOverLoadWithPostData()
        {
            string responseText;
            using (Stream response = RequestFactory.CreateGet(_defaultUri, new { param1 = "param1Value" }).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("This is Default.aspx") > -1);
            Assert.IsTrue(responseText.IndexOf("?param1=param1Value") > -1);
        }


        [Test, Description("")]
        public void GetWithAnonymousObjectQuery()
        {

            var postData = new { param1 = "param1Value", param2 = "param2Value" };

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(_defaultUri, HttpMethod.Get, ContentType.None, postData, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("?param1=param1Value&param2=param2Value") > -1);
        }

        [Test, Description("")]
        public void GetWithDictionaryQuery()
        {


            Dictionary<string, string> postData = new Dictionary<string, string> { { "param1", "param1Value" }, { "param2", "param2Value" } };

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(_defaultUri, HttpMethod.Get, ContentType.None, postData, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("?param1=param1Value&param2=param2Value") > -1);
        }

        [Test, Description("")]
        public void GetWithNameValueCollectionQuery()
        {


            NameValueCollection postData = new NameValueCollection { { "param1", "param1Value" }, { "param2", "param2Value" } };

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(_defaultUri, HttpMethod.Get, ContentType.None, postData, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("?param1=param1Value&param2=param2Value") > -1);
        }

        [Test, Description("")]
        public void PageMethodPostWithAnonymousTypeParam()
        {
            Uri uri = NormalizeUri("Default.aspx/PageMethod");

            var postData = new { name = _nameValue, date = _dateValue };

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(uri, HttpMethod.Post, ContentType.ApplicationJson, postData, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            TestClass result = Jsr.CleanAndDeserialize<TestClass>(responseText);
            Assert.AreEqual(_nameValue, result.Name);
            Assert.AreEqual(_dateValue, result.Date);
        }

        [Test, Description("")]
        public void PostAjaxWcfServiceObjectJson()
        {
            var postData = new { testClass = new TestClass { Name = _nameValue, Date = _dateValue } };


            Uri uri = NormalizeUri("AjaxWcfService.svc/EchoObject");

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(uri, HttpMethod.Post, ContentType.ApplicationJson, postData, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            TestClass result = Jsr.CleanAndDeserialize<TestClass>(responseText);
            Assert.AreEqual(_nameValue, result.Name);
            Assert.AreEqual(_dateValue, result.Date);
        }

        [Test, Description("")]
        public void CreatePostJsonAppAjaxWcfService()
        {

            Uri uri = NormalizeUri("AjaxWcfService.svc/EchoParams");

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(uri, HttpMethod.Post, ContentType.ApplicationJson, GetPostData(), null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            TestClass result = Jsr.CleanAndDeserialize<TestClass>(responseText);
            Assert.AreEqual(_nameValue, result.Name);
            Assert.AreEqual(_dateValue, result.Date);
        }


        [Test, Description("")]
        public void PostForm()
        {

            NameValueCollection postData = new NameValueCollection
                {
                    {"op", "postForm"},
                    {"field1", "field1Value1"},
                    {"field1", "field1Value2"},
                    {"field1", "field1Value3"},
                    {"field2", "field2Value"}
                };

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(_postHandlerUri, HttpMethod.Post, ContentType.ApplicationForm, postData, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.AreEqual(
                "{\"Status\":\"OK\",\"Field1\":\"field1Value1,field1Value2,field1Value3\",\"Field2\":\"field2Value\"}",
                responseText);
        }

        [Test, Description("")]
        public void PostWebServiceObjectJson()
        {
            var postData = new { testClass = new TestClass { Name = _nameValue, Date = _dateValue } };

            Uri uri = NormalizeUri("WebService1.asmx/EchoObject");

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(uri, HttpMethod.Post, ContentType.ApplicationJson, postData, null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            TestClass result = Jsr.CleanAndDeserialize<TestClass>(responseText);
            Assert.AreEqual(_nameValue, result.Name);
            Assert.AreEqual(_dateValue, result.Date);
        }


        [Test, Description("")]
        public void PostWebServiceParamsJson()
        {

            Uri uri = NormalizeUri("WebService1.asmx/EchoParams");

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(uri, HttpMethod.Post, ContentType.ApplicationJson, GetPostData(), null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            TestClass result = Jsr.CleanAndDeserialize<TestClass>(responseText);
            Assert.AreEqual(_nameValue, result.Name);
            Assert.AreEqual(_dateValue, result.Date);
        }


        [Test, Description("")]
        public void PostWebServiceParamsXml()
        {

            Uri uri = NormalizeUri("WebService1.asmx/EchoParams");

            string responseText;
            using (Stream response = RequestFactory.CreateRequest(uri, HttpMethod.Post, ContentType.ApplicationForm, GetPostData(), null, null).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("TestClass") > -1);
        }




        [Test, Description("Sanity Check - Simple Get with WebClient")]
        public void StockGet()
        {
            WebClient client = new WebClient();

            string actual = client.DownloadString(_defaultUri);
            Assert.IsTrue(actual.IndexOf("This is Default.aspx") > -1);
        }


        #region PostData, Cookies and Headers
        /// <summary>
        /// Here we are using all optional arguments to ensure implementation.
        /// While I am not yet testing all overloads, this should exercise and test
        /// postData,cookies and headers sufficiently.
        /// </summary>
        [Test, Description("")]
        public void GetOverLoadWithPostDataCookiesAndHeaders()
        {
            CookieContainer cookies = new CookieContainer();
            cookies.Add(_defaultUri, new Cookie("testCookie", "testCookie"));
            NameValueCollection headers = new NameValueCollection { { "x-test-header", "x-test-header-value" } };
            string responseText;
            using (Stream response = RequestFactory.CreateGet(_defaultUri, new { param1 = "param1Value" }, cookies, headers).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            Assert.IsTrue(responseText.IndexOf("This is Default.aspx") > -1);
            Assert.IsTrue(responseText.IndexOf("?param1=param1Value") > -1);
            Assert.IsTrue(responseText.IndexOf("x-test-header-value") > -1);
            CookieCollection tmpCookies = cookies.GetCookies(_defaultUri);
            Assert.IsNotNull(tmpCookies["ASP.NET_SessionId"]);
            Assert.IsNotNull(tmpCookies["testCookie"]);
        }

        /// <summary>
        /// Here we are using all optional arguments to ensure implementation
        /// </summary>
        [Test, Description("")]
        public void PostFileAllParams()
        {

            const string headerName = "x-test-header";
            const string postDataFieldName = "field";
            // these values are intentionally arbitrary
            const string postDataFieldValue = "value";
            const string fileName = "foobar";
            const string contentType = "text/foo";
            const string fileFieldName = "fileField";
            const string headerValue = "x-test-header-value";
            const string cookie = "testCookie";

            string filePath = Path.GetFullPath("TestFiles/TextFile1.txt");

            CookieContainer cookies = new CookieContainer();
            cookies.Add(_postHandlerUri, new Cookie(cookie, cookie));

            NameValueCollection headers = new NameValueCollection { { headerName, headerValue } };
            NameValueCollection postData = new NameValueCollection { { postDataFieldName, postDataFieldValue } };

            // we can use this anonymous type as a template for
            // deserializing json text
            var responseTemplate =
                new
                {
                    FileName = "",
                    ContentType = "",
                    ContentLength = 0,
                    Text = string.Empty,
                    Header = string.Empty,
                    Field = string.Empty,
                    FileField = string.Empty
                };


            string responseText;
            using (Stream response = RequestFactory.CreateFilePost(_postHandlerUri, postData, new MemoryStream(File.ReadAllBytes(filePath)), fileName, contentType, fileFieldName, cookies, headers).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }

            var obj = JsonConvert.DeserializeAnonymousType(responseText, responseTemplate);

            Assert.AreEqual(fileName, obj.FileName);
            Assert.AreEqual(contentType, obj.ContentType);
            Assert.AreEqual(16, obj.ContentLength);
            Assert.AreEqual(headerValue, obj.Header);
            Assert.AreEqual(postDataFieldValue, obj.Field);
            Assert.AreEqual(fileFieldName, obj.FileField);
            Assert.AreEqual(File.ReadAllText(filePath), obj.Text);

            CookieCollection tmpCookies = cookies.GetCookies(_postHandlerUri);
            Assert.IsNotNull(tmpCookies["fromServer"]);
            Assert.IsNotNull(tmpCookies[cookie]);
            Assert.IsNotNull(tmpCookies["ASP.NET_SessionId"]);
        }




        [Test, Description("")]
        public void PostOverloadPostDataAndCookiesAndHeaders()
        {
            const string headerName = "x-test-header";
            const string headerValue = "x-test-header-value";
            const string cookie = "testCookie";
            CookieContainer cookies = new CookieContainer();
            cookies.Add(_postHandlerUri, new Cookie(cookie, cookie));
            NameValueCollection headers = new NameValueCollection { { headerName, headerValue } };

            NameValueCollection postData = new NameValueCollection
                {
                    {"op", "postForm"},
                    {"field1", "field1Value1"},
                    {"field1", "field1Value2"},
                    {"field1", "field1Value3"},
                    {"field2", "field2Value"}
                };

            string responseText;
            using (Stream response = RequestFactory.CreatePostForm(_postHandlerUri, postData, cookies, headers).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }

            Assert.AreEqual(
                "{\"Status\":\"OK\",\"Field1\":\"field1Value1,field1Value2,field1Value3\",\"Field2\":\"field2Value\"}",
                responseText);


            CookieCollection tmpCookies = cookies.GetCookies(_postHandlerUri);
            Assert.IsNotNull(tmpCookies["fromServer"]);
            Assert.IsNotNull(tmpCookies[cookie]);
            Assert.IsNotNull(tmpCookies["ASP.NET_SessionId"]);

        }

        [Test, Description("")]
        public void PostOverloadPostData()
        {
            NameValueCollection postData = new NameValueCollection
                {
                    {"op", "postForm"},
                    {"field1", "field1Value1"},
                    {"field1", "field1Value2"},
                    {"field1", "field1Value3"},
                    {"field2", "field2Value"}
                };

            string responseText;
            using (Stream response = RequestFactory.CreatePostForm(_postHandlerUri, postData).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }

            Assert.AreEqual(
                "{\"Status\":\"OK\",\"Field1\":\"field1Value1,field1Value2,field1Value3\",\"Field2\":\"field2Value\"}",
                responseText);




        }


        [Test, Description("")]
        public void CreatePostJsonAppOverloadWithCookiesAndHeaders()
        {
            const string headerName = "x-test-header";
            const string headerValue = "x-test-header-value";
            const string cookie = "testCookie";
            CookieContainer cookies = new CookieContainer();
            cookies.Add(_postHandlerUri, new Cookie(cookie, cookie));
            NameValueCollection headers = new NameValueCollection { { headerName, headerValue } };

            Uri uri = NormalizeUri("AjaxWcfService.svc/EchoParams");

            string responseText;
            using (Stream response = RequestFactory.CreatePostJsonApp(uri, GetPostData(), cookies, headers).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            TestClass result = Jsr.CleanAndDeserialize<TestClass>(responseText);
            Assert.AreEqual(_nameValue, result.Name);
            Assert.AreEqual(_dateValue, result.Date);
            Assert.AreEqual(headerValue, result.Header);

            CookieCollection tmpCookies = cookies.GetCookies(_postHandlerUri);
            Assert.IsNotNull(tmpCookies[cookie]);
            Assert.IsNotNull(tmpCookies["ASP.NET_SessionId"]);
        }

        [Test, Description("")]
        public void CreatePostJsonApp()
        {

            Uri uri = NormalizeUri("AjaxWcfService.svc/EchoParams");

            string responseText;
            using (Stream response = RequestFactory.CreatePostJsonApp(uri, GetPostData()).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            TestClass result = Jsr.CleanAndDeserialize<TestClass>(responseText);
            Assert.AreEqual(_nameValue, result.Name);
            Assert.AreEqual(_dateValue, result.Date);
        }

        #endregion


        #region Upload
        [Test, Description("")]
        public void PostFileFromDisk()
        {



            string filePath = Path.GetFullPath("TestFiles/TextFile1.txt");

            string contentType;
            RequestFactory.TryGetContentType(filePath, out contentType);

            string responseText;
            using (Stream response = RequestFactory.CreateFilePost(_postHandlerUri, filePath).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            var obj = JsonConvert.DeserializeAnonymousType(responseText,
                                                           new { FileName = string.Empty, ContentType = "", ContentLength = 0, Text = string.Empty });
            Assert.AreEqual("TextFile1.txt", obj.FileName);
            Assert.AreEqual("text/plain", obj.ContentType);
            Assert.AreEqual("TextFile1.txt", obj.Text);
        }

        [Test, Description("")]
        public void PostFileFromMemory()
        {


            string filePath = Path.GetFullPath("TestFiles/TextFile1.txt");
            string fileName = Path.GetFileName(filePath);
            string responseText;
            using (Stream response = RequestFactory.CreateFilePost(_postHandlerUri, new MemoryStream(File.ReadAllBytes(filePath)), fileName).GetResponse().GetResponseStream())
            {
                responseText = response.Text();
            }
            var obj = JsonConvert.DeserializeAnonymousType(responseText,
                                                           new { FileName = "", ContentType = "", ContentLength = 0, Text = string.Empty });

            Assert.AreEqual(fileName, obj.FileName);
            Assert.AreEqual("text/plain", obj.ContentType);
            Assert.AreEqual("TextFile1.txt", obj.Text);
        }


        #endregion



        [Test]
        public void ParseYellowScreenOfDeath()
        {
            try
            {
                RequestFactory.CreatePostForm(_postHandlerUri, new NameValueCollection { { "op", "throwException" } }).GetResponse().GetResponseStream();
                Assert.Fail("Expected WebRequestException");
            }
            catch (WebException ex)
            {
                WebRequestException ex2 = WebRequestException.Create(ex);
                Console.WriteLine("Caught {0}: {1}", ex2.ExceptionDetail.ExceptionType, ex2.Message);
            }
        }

        [Test]
        public void ParseJsonFault()
        {

            Uri uri = NormalizeUri("AjaxWcfService.svc/ThrowException");

            try
            {
                RequestFactory.CreatePostJsonApp(uri).GetResponse().GetResponseStream();
                Assert.Fail("Expected JsonFaultDetail");
            }
            catch (WebException ex)
            {
                WebRequestException ex2 = WebRequestException.Create(ex);
                Console.WriteLine("Caught {0}: {1}", ex2.ExceptionDetail.ExceptionType, ex2.Message);
            }
        }

        [Test]
        public void CleanAndDeserializeToAnonymousType()
        {
            // To demonstrate deserializing to an anonymous type consider this:

            // This is an example of a TestClass response
            // {"d":{"__type":"TestClass:#Salient.Web.HttpLib.TestSite","Date":"\/Date(1271275580882)\/","Header":"","IntVal":99,"Name":"sky"}}

            const string responseText =
                "{\"d\":{\"__type\":\"TestClass:#Salient.Web.HttpLib.TestSite\",\"Date\":\"\\/Date(1271275580882)\\/\",\"Header\":\"\",\"IntVal\":99,\"Name\":\"sky\"}}";

            // imagine for a moment that we do not have a reference to Salient.Web.HttpLib.TestSite from which
            // to deserialize this JSON into an instance of TestClass.

            // this is what an anonymous prototype of TestClass looks like

            var testClassPrototype = new
                {
                    Name = default(string),
                    Header = default(string),
                    Date = default(DateTime),
                    IntVal = default(int)
                };

            // just pass this prototype to deserialize to get a strongly typed instance 
            var jsob = new JavaScriptSerializer().CleanAndDeserialize(responseText, testClassPrototype);

            Assert.AreEqual("sky", jsob.Name);
            Assert.AreEqual(99, jsob.IntVal);

            // now also imagine that we are interested in only part of a JSON response,
            var prototypeOfInterestingData = new
                {
                    Name = default(string)

                };

            var partialJsob = new JavaScriptSerializer().CleanAndDeserialize(responseText, prototypeOfInterestingData);

            Assert.AreEqual("sky", partialJsob.Name);

            // one thing to keep in mind is that anonymous types are read-only.

        }



        private object GetPostData()
        {
            return new { name = _nameValue, date = _dateValue };
        }

    }
}