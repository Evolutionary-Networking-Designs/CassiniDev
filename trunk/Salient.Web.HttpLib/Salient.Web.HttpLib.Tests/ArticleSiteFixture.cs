using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Salient.Web.HttpLib.TestSite;

using NUnit.Framework;

namespace Salient.Web.HttpLib.Tests
{
    /// <summary>
    /// 
    /// This test fixture contains methods that simulate the type of requests that are to be expected from our client script. 
    /// 
    /// Allows for automated testing of our endpoints from the perspective of client script.
    /// 
    /// You can call this functional testing or smoke testing or maybe even integration testing, just don't call it unit testing.
    /// 
    /// You will also get a brief introduction to the HttpLib classes starting with WebDevFixture.
    /// 
    /// WebDevFixture encapsulates a controlled startup of Visual Studio's development server. To spin up an instance of WebDev, 
    /// simply supply an unused port and the physical path to the site. This can be a relative path enabling portable tests.
    /// 
    /// WebDevFixture provides a convenience method, .NormalizeUri(), that will normalize, or root, a relative path and query string to the root of the current site.
    /// 
    /// For more information see http://www.codeproject.com/Articles/72222/A-general-purpose-Visual-Studio-2008-Development-S.aspx.
    /// 
    /// For more finely tuned control of a testing server you should check out http://cassinidev.codeplex.com/.
    /// 
    /// This example demonstrates use of WebDevFixture with NUnit but any testing framework can be used with ease.
    /// </summary>
    [TestFixture]
    public class ArticleSiteFixture : WebDevFixture
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            StartServer(@"..\..\..\Salient.Web.HttpLib.TestSite");
        }


        // Ajax-enabled WCF service, static .aspx PageMethods and Xml WebServices that have been decorated with [ScriptService] or [ScriptMethod] attributes all demonstrate the same behavior and will hereafter referred to collectively as 'JSON endpoints'.

        // Note: consider yourself warned: In the spirit of the JavaScript client we are going to be simulating, I will be abusing closures, anonymous methods and types with impunity and glee. ;-)
        /// <summary>
        /// 
        /// The default format for a JSON endpoint response JSON  is a 'd: wrapped' object. There are some configuration options and attributes that can modify this behavior, but this default behavior is also the worst case scenario so lets deal with it straight away.
        /// 
        /// An example that we will be dealing with is the simple Result class defined in the demo site.  When returned as JSON it looks like this:
        /// 
        /// <code>{"d":{"__type":"Result:#HttpLibArticleSite","Message":"Value pull OK.","Session":"rmyykw45zbkxxxzdun0juyfr","Value":"foo"}}</code>
        /// 
        /// Deserializing JSON of this format can be problematic but I have provided an extension method for JavaScriptSerializer that will handle this type of JSON. You can read about it in detail here: http://www.codeproject.com/KB/aspnet/Parsing-ClientScript-JSON.aspx
        /// 
        /// The short story is that ClientScriptUtilities provides a JavaScriptSerializer extension method, .CleanAndDeserialize<T>(), that will extract the inner object and clean the "__type" properties before deserializing it. The JSON that is ultimately serialized looks like this:
        /// 
        /// {"Message":"Value pull OK.","Session":"rmyykw45zbkxxxzdun0juyfr","Value":"foo"}
        /// 
        /// CleanAndDeserialize also supports deserialization to anonymous types.
        /// 
        /// NOTE: CleanAndDeserialize does not require ugly MsAjax JSON. It will consume any valid JSON so this is a safe replacement for .Deserialize<T> throughout your code regardless of the JSON format.
        /// </summary>
        [Test]
        public void Deserializing_MS_Ajax_Wrapped_JSON()
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();


            // simulate a typical JSON response
            const string responseText =
                "{\"d\":{\"__type\":\"Result:#HttpLibArticleSite\",\"Message\":\"Value pull OK.\",\"Session\":\"rmyykw45zbkxxxzdun0juyfr\",\"Value\":\"foo\"}}";

            Result result = jsSerializer.CleanAndDeserialize<Result>(responseText);

            Assert.AreEqual("rmyykw45zbkxxxzdun0juyfr", result.Session);

            var resultPrototype = new
                {
                    Message = default(string),
                    Value = default(string)

                };

            var anonymousResultSubset = jsSerializer.CleanAndDeserialize(responseText, resultPrototype);

            Assert.AreEqual("foo", anonymousResultSubset.Value);

        }


        /// <summary>
        /// When an endpoint of any kind throws an exception it is ultimately swallowed by a generic '500 Internal Server Error' WebException. 
        /// Discovering the actual underlying exception can be quite tedious, involving parsing of the WebException.Response.GetResponseStream() and determining whether it is a JsonFaultDetail from a JSON endpoint, an ASP.Net Yellow Screen of death or a number of other formats.
        /// WebRequestException encapsulates all of this behavior and surfaces the salient information, if available. The original WebException is availabe on the InnerException property.
        /// </summary>
        [Test]
        public void Exception_Handling()
        {

            try
            {
                RequestFactory.CreatePostJsonApp(NormalizeUri("AjaxService.svc/ThrowException")).GetResponse();
                Assert.Fail("Expected WebException");
            }
            catch (WebException ex)
            {
                WebRequestException ex2 = WebRequestException.Create(ex);
                Console.WriteLine("Caught {0}: {1}", ex2.ExceptionDetail.ExceptionType, ex2.Message);
            }
        }

        /// <summary>
        /// RequestFactory.CreateRequest is the base method that all of the specialized overloads ultimately call. 
        /// With a basic understanding of this method, all of the specialized overloads will be readily understandable.
        /// 
        /// RequestFactory.CreateRequest builds up an HttpWebRequest, applying the supplied arguments appropriately and returns it.
        /// You are then free to manipulate the request as you choose before executing the request. This pattern is applied over all of the various overloads.
        /// 
        /// Required arguments:
        ///   <code>Uri</code>                 requestUri   - A string or Uri
        /// 
        ///   <code>HttpMethod</code>          method       - The http method of the request. 
        ///                                      Currently supported options are <code>HttpMethod.Post</code> and <code>HttpMethod.Get</code>
        /// 
        ///   <code>ContentType</code>         contentType  - The content-type of the request. 
        ///                                      Currently supported options are <code>ContentType.None</code>, <code>ContentType.ApplicationForm</code>, <code>ContentType.ApplicationJson</code> and <code>ContentType.TextJson</code>
        /// 
        /// Optional arguments:
        ///   <code>object</code>              postData     - Optional. A NameValueCollection or object. Anonymous types are acceptable. The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e. For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body. For a JSON 'Post', it will be JSON serialized and streamed into the request body. For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'. Note: any object that is shaped like the target method's arguments is acceptable. Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form. When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send. 
        /// 
        ///                                      An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code>
        /// 
        ///                                      An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code> or <code>var postData = new { input = new { Message = "message" } };</code>
        ///                                      
        /// 
        ///   <code>CookieCollection</code>    cookies      - Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.
        /// 
        ///   <code>NameValueCollection</code> headers      - Optional. Http headers to be added to the request.
        /// 
        /// <code>StreamExtensions</code> provide two simple convenience extensions for use with the response stream.
        ///   <code>Stream.Text()</code> will extract the stream into an UTF-8 string.
        ///   <code>Stream.Bytes()</code> will extract the stream into a byte array.
        /// </summary>
        [Test]
        public void Using_CreateRequest()
        {

            // Required arguments:
            Uri uri = NormalizeUri("AjaxService.svc/PutSessionVar");
            const HttpMethod method = HttpMethod.Post;
            const ContentType contentType = ContentType.ApplicationJson;
            // Optional arguments:


            // This is the method we will be calling - public Result PutSessionVar(string input)
            // Lets create an anonymous type shaped like the method signature...
            var postData = new { input = "foo" };

            CookieContainer cookies = new CookieContainer();

            // Add a useful header...
            NameValueCollection headers = new NameValueCollection { { "x-foo-header", "bar-value" } };

            // With the arguments defined, create the request
            HttpWebRequest request = RequestFactory.CreateRequest(uri, method, contentType, postData, cookies, headers);

            // If you are not concerned with immediate disposal of the response stream, 
            // you can retrieve the response with a single expression like so
            var responseText = request.GetResponse().GetResponseStream().Text();


            // thats it for the basic usage.





            // we are going to pick up the pace a bit and demonstrate a few usage patterns so lets declare an anonymous 
            // inline factory method to create our requests from this point on.

            Func<HttpWebRequest> createRequest = () => RequestFactory.CreateRequest(uri, method, contentType, postData, cookies, headers);


            using (Stream responseStream = createRequest().GetResponse().GetResponseStream())
            {
                var rtext = responseStream.Text();
            }


            // It is recommended that you wrap requests using the exception handling pattern previously described
            // even if you are just going to re-throw

            try
            {
                using (Stream responseStream = createRequest().GetResponse().GetResponseStream())
                {
                    var rtext = responseStream.Text();
                }
            }
            catch (WebException ex)
            {
                WebRequestException wrex = WebRequestException.Create(ex);
                // Now either try to handle it or re-throw

                throw wrex;
                // we reset the stack trace but a WebRequestException has but one source so this is not an issue.
            }


            // Now if we throw the deserialization into the mix things start coming together.

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

            try
            {
                using (Stream responseStream = createRequest().GetResponse().GetResponseStream())
                {
                    Result result1 = jsSerializer.CleanAndDeserialize<Result>(responseStream.Text());
                    Assert.AreEqual("foo", result1.Value);
                }
            }
            catch (WebException ex)
            {
                WebRequestException wrex = WebRequestException.Create(ex);
                throw wrex;
            }




            // Using an anonymous prototype for the response

            try
            {
                using (Stream responseStream = createRequest().GetResponse().GetResponseStream())
                {
                    var resultPrototype = new
                        {
                            Value = default(string),
                            Session = default(string)
                        };

                    var result2 = jsSerializer.CleanAndDeserialize(responseStream.Text(), resultPrototype);

                    Assert.AreEqual("foo", result2.Value);
                }
            }
            catch (WebException ex)
            {
                WebRequestException wrex = WebRequestException.Create(ex);
                throw wrex;
            }




            // For those who like to live dangerously the whole process can be executed in one statement 
            // Do not try this at home. ;-)

            CookieContainer existingCookies = new CookieContainer();

            var result3 = jsSerializer.CleanAndDeserialize(
                RequestFactory.CreateRequest(NormalizeUri("AjaxService.svc/PutSessionVar"), HttpMethod.Post, ContentType.ApplicationJson,
                                             new { input = "foo" }, existingCookies, new NameValueCollection { { "x-foo-header", "bar-value" } }).
                    GetResponse().GetResponseStream().Text(), new { Value = default(string) });
            Assert.AreEqual("foo", result3.Value);

            // interesting as this treatment may be, I think you would have to be mad to apply it. 

        }



        /// <summary>
        /// Now, with a basic understanding of the base .CreateRequest() method, lets refactor to reduce code bloat and 
        /// centralize our JSON response execution pattern in a few generic methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="handler">If null, created WebRequestException is thrown when GetReponse craps out.</param>
        /// <returns></returns>
        public T GetJsonResponse<T>(HttpWebRequest request, Action<WebRequestException> handler)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            try
            {
                using (Stream responseStream = request.GetResponse().GetResponseStream())
                {

                    return jsSerializer.CleanAndDeserialize<T>(responseStream.Text());
                }
            }
            catch (WebException ex)
            {
                WebRequestException reqEx = WebRequestException.Create(ex);
                if (handler != null)
                {
                    handler(reqEx);
                    return default(T);
                }

                throw reqEx;
            }
        }

        /// <summary>
        /// And for anonymous response types....
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="prototype"></param>
        /// <param name="handler">If null, created WebRequestException is thrown when GetReponse craps out.</param>
        /// <returns></returns>
        public T GetJsonResponse<T>(HttpWebRequest request, T prototype, Action<WebRequestException> handler)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            try
            {
                using (Stream responseStream = request.GetResponse().GetResponseStream())
                {

                    return jsSerializer.CleanAndDeserialize(responseStream.Text(), prototype);
                }
            }
            catch (WebException ex)
            {
                WebRequestException reqEx = WebRequestException.Create(ex);
                if (handler != null)
                {
                    handler(reqEx);
                    return default(T);
                }

                throw reqEx;
            }
        }



        /// <summary>
        /// Test the utility methods.
        /// </summary>
        [Test]
        public void Using_Utility_Methods()
        {
            // We have already seen how to build up a request from parts, so for the sake of 
            // brevity (and being clever) lets define an inline factory method.

            Func<HttpWebRequest> createRequest = () => RequestFactory.CreateRequest(
                                                           NormalizeUri("AjaxService.svc/PutSessionVar"), HttpMethod.Post, ContentType.TextJson, new { input = "foo" }, null, null);

            // As an added bit of abuse, an anonymous exception handler to pass to the generic utility methods

            Action<WebRequestException> commonExceptionHandler = up =>
                {

                    Console.WriteLine("Inside inline shared exception handler");
                    // handle ex if possible, and return, otherwise throw 


                    throw up; // ;-O~~
                };


            // Call a valid method with referenced response type. commonExceptionHandler will not be executed
            Result result = GetJsonResponse<Result>(createRequest(), commonExceptionHandler);

            Assert.AreEqual("foo", result.Value);


            // Call a valid method with an anonymous response type. commonExceptionHandler will not be executed
            var anonResult = GetJsonResponse(createRequest(), new { Value = default(string) }, commonExceptionHandler);

            Assert.AreEqual("foo", anonResult.Value);

            // Test the generic GetJsonResponse with an exception and a handler, . commonExceptionHandler WILL executed
            try
            {
                GetJsonResponse(RequestFactory.CreatePostJsonApp(NormalizeUri("AjaxService.svc/ThrowException")), new { Value = default(string) }, commonExceptionHandler);
                Assert.Fail("Expected WebRequestException");
            }
            catch (WebRequestException)
            {

                Console.WriteLine("Handler works");
            }

            // test the generic GetJsonResponse with an exception and no handler. commonExceptionHandler is null and will NOT be executed
            try
            {
                GetJsonResponse(RequestFactory.CreatePostJsonApp(NormalizeUri("AjaxService.svc/ThrowException")), new { Value = default(string) }, null);
                Assert.Fail("Expected WebRequestException");
            }
            catch (WebRequestException)
            {

                Console.WriteLine("WebRequestException was thrown from GetJsonResponse as no handler was specified");
            }
        }


        /// <summary>
        /// 
        /// Now lets take the time for a brief overview of the available overloads of CreateRequest before we start drilling down into the .CreatePostJsonApp().
        /// 
        /// The overloads and the salient differences
        /// 
        /// RequestFactory.CreateGet();
        ///   Creates a request using http 'Get' and ContentType.None. 
        ///     PostData is Url-encoded and appended to the Uri.
        /// 
        /// RequestFactory.CreateGetJsonApp();
        ///   Creates a request using http 'Get' and ContentType.ApplicationJson. 
        ///     PostData is Url-encoded and appended to the Uri.
        /// 
        /// RequestFactory.CreateGetJsonText();
        ///   Creates a request using http 'Get' and ContentType.TextJson. 
        ///     PostData is Url-encoded and appended to the Uri.
        /// 
        /// 
        /// RequestFactory.CreatePostForm();
        ///   Creates a request using http 'Post' and ContentType.ApplicationForm. 
        ///     PostData is Url-encoded and written to the request body.
        /// 
        /// RequestFactory.CreatePostJsonApp();
        ///   Creates a request using http 'Post' and ContentType.ApplicationJson. 
        ///    PostData is JSON-encoded and written to the request body.
        /// 
        /// RequestFactory.CreatePostJsonText();
        ///   Creates a request using http 'Post' and ContentType.TextJson. 
        ///    PostData is JSON-encoded and written to the request body.
        /// 
        /// All overloads accept a string or Uri for requestUri
        /// 
        /// 
        /// There is a specialized RequestFactory.CreateFilePost() method for uploading files that has it's own
        /// unique but similar API that will be covered at the end of this walkthrough.
        /// 
        /// </summary>
        public void The_Overload_Overview()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreateGet_Overloads()
        {
            Uri requestUri = NormalizeUri("Default.aspx");

            CookieContainer cookies = new CookieContainer();
            NameValueCollection headers = new NameValueCollection();

            // postData could also be an object or anonymous type
            NameValueCollection postData = new NameValueCollection();


            // get overloads
            RequestFactory.CreateGet(requestUri)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGet(requestUri, postData)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGet(requestUri, postData, cookies)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGet(requestUri, postData, cookies, headers)
                .GetResponse().GetResponseStream().Text();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreateGetJsonApp_Overloads()
        {
            Uri requestUri = NormalizeUri("Default.aspx");

            CookieContainer cookies = new CookieContainer();
            NameValueCollection headers = new NameValueCollection();

            // postData could also be an object or anonymous type
            NameValueCollection postData = new NameValueCollection();

            RequestFactory.CreateGetJsonApp(requestUri)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGetJsonApp(requestUri, postData)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGetJsonApp(requestUri, postData, cookies)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGetJsonApp(requestUri, postData, cookies, headers)
                .GetResponse().GetResponseStream().Text();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreateGetJsonText_Overloads()
        {
            Uri requestUri = NormalizeUri("Default.aspx");

            CookieContainer cookies = new CookieContainer();
            NameValueCollection headers = new NameValueCollection();

            // postData could also be an object or anonymous type
            NameValueCollection postData = new NameValueCollection();

            RequestFactory.CreateGetJsonText(requestUri)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGetJsonText(requestUri, postData)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGetJsonText(requestUri, postData, cookies)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreateGetJsonText(requestUri, postData, cookies, headers)
                .GetResponse().GetResponseStream().Text();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreatePostForm_Overloads()
        {
            Uri requestUri = NormalizeUri("Default.aspx");

            CookieContainer cookies = new CookieContainer();
            NameValueCollection headers = new NameValueCollection();

            // postData could also be an object or anonymous type
            NameValueCollection postData = new NameValueCollection();

            RequestFactory.CreatePostForm(requestUri)
                .GetResponse().GetResponseStream().Text(); ;

            RequestFactory.CreatePostForm(requestUri, postData)
                .GetResponse().GetResponseStream().Text(); ;

            RequestFactory.CreatePostForm(requestUri, postData, cookies)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreatePostForm(requestUri, postData, cookies, headers)
                .GetResponse().GetResponseStream().Text();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreatePostJsonApp_Overloads()
        {
            Uri requestUriNoArgs = NormalizeUri("AjaxService.svc/Noop");
            Uri requestUri = NormalizeUri("AjaxService.svc/PutSessionVar");

            CookieContainer cookies = new CookieContainer();
            NameValueCollection headers = new NameValueCollection();

            // postData could also be an object or a NameValueCollection
            object postData = new { input = "foo" };

            RequestFactory.CreatePostJsonApp(requestUriNoArgs)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreatePostJsonApp(requestUri, postData)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreatePostJsonApp(requestUri, postData, cookies)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreatePostJsonApp(requestUri, postData, cookies, headers)
                .GetResponse().GetResponseStream().Text();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CreatePostJsonText_Overloads()
        {
            Uri requestUriNoArgs = NormalizeUri("AjaxService.svc/Noop");
            Uri requestUri = NormalizeUri("AjaxService.svc/PutSessionVar");

            CookieContainer cookies = new CookieContainer();
            NameValueCollection headers = new NameValueCollection();

            // postData could also be an object or a NameValueCollection
            object postData = new { input = "foo" };

            RequestFactory.CreatePostJsonText(requestUriNoArgs)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreatePostJsonText(requestUri, postData)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreatePostJsonApp(requestUri, postData, cookies)
                .GetResponse().GetResponseStream().Text();

            RequestFactory.CreatePostJsonText(requestUri, postData, cookies, headers)
                .GetResponse().GetResponseStream().Text();
        }




        /// <summary>
        /// 
        /// All of the overloads resolve to the base .CreateRequest() methods thus follow the same pattern. The focus of this walkthrough is testing MS JSON endpoints, so we are going to focus on the .CreatePostJsonApp() method.
        /// 
        /// Most other scenarios such as posting to a standard form and consuming a non MS REST service can be serviced in like fashion using the other overloads.
        /// 
        /// So, simulating an XMLHttpRequest that would be used to consume an ms JSON endpoint is accomplished using CreatePostJsonApp().
        /// 
        /// In this example, in order to demonstrate the direct parity between the three types of JSON endpoints we have created 3 endpoints, an Ajax-enabled WCF service, 
        /// A WebServices decorated with [ScriptService] and static .aspx methods decorated with [WebMethod] (PageMethods), each with a similar API.
        /// 
        /// We will call each of these endpoints using identical code from within a loop and witness identical behavior.
        /// 
        /// Note: in previous listings, various generic utility methods and inline factory strategies have been presented. 
        /// You may find these patterns useful but, for the sake of clarity, the rest of this walk through will use expanded syntax.
        /// 
        /// </summary>
        [Test]
        public void Using_RequestFactory_CreatePostJsonApp_With_Json_Endpoints()
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

            // the methods will be calling looks like this:

            // public string PutSessionVar(string input)
            // public Result GetSessionVar()


            // parameters can be defined with anonymous types. 
            var postData = new { input = "foo" };

            // we will simply call API equivalent instances of each endpoint with the same code.
            string[] jsonEndpoints = new[] { "AjaxService.svc", "ScriptService.asmx", "Default.aspx" };

            foreach (string address in jsonEndpoints)
            {
                try
                {
                    using (var response = RequestFactory.CreatePostJsonApp(NormalizeUri(address + "/PutSessionVar"), postData).GetResponse().GetResponseStream())
                    {
                        Result result = jsSerializer.CleanAndDeserialize<Result>(response.Text());
                        Assert.AreEqual("foo", result.Value);
                    }

                    // now lets get the value

                    try // this try/catch is a bit of a spoiler but bear with me
                    {
                        RequestFactory.CreatePostJsonApp(NormalizeUri(address + "/GetSessionVar")).GetResponse().GetResponseStream();
                        Assert.Fail("Expected WebException");
                    }
                    catch (WebException ex)
                    {
                        // as you can see, the value was not found in the session because we don't have a cookie
                        // container to share between requests
                        WebRequestException ex2 = WebRequestException.Create(ex);
                        Console.WriteLine("Caught {0}: {1}", ex2.ExceptionDetail.ExceptionType, ex2.Message);

                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail("Endpoint {0} failed with {1}", address, ex.Message);
                }
            }

            // Which brings us to the overload of .CreatePostJsonApp that accepts a CookieContainer as an argument....

        }


        /// <summary>
        /// 
        /// To maintain session state, FormsAuthentication and other cookies between requests we need to pass a common CookieContainer to each request.
        /// 
        /// Http headers may also be added to the request using another overload but this will not be handled here. Those that need to do such a thing will easily understand the implementation.
        /// </summary>
        [Test]
        public void Using_CookieContainer_With_CreatePostJsonApp()
        {

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

            string[] jsonEndpoints = new[] { "AjaxService.svc", "ScriptService.asmx", "Default.aspx" };
            List<string> failures = new List<string>();
            foreach (string address in jsonEndpoints)
            {
                try
                {

                    // a container to store our cookies
                    CookieContainer cookies = new CookieContainer();

                    // the SessionId to verify cookies/session are working as expected
                    string sessionId;

                    // pass the CookieContainer in to catch any cookies set by the server
                    using (var response = RequestFactory.CreatePostJsonApp(NormalizeUri(address + "/PutSessionVar"), new { input = "foo" }, cookies).GetResponse().GetResponseStream())
                    {
                        Result result = jsSerializer.CleanAndDeserialize<Result>(response.Text());
                        Assert.AreEqual("foo", result.Value);

                        sessionId = result.Session;
                    }

                    // get the var
                    // pass the same CookieContainer in with the next request as it now contains the session cookie 
                    using (var response = RequestFactory.CreatePostJsonApp(NormalizeUri(address + "/GetSessionVar"), null, cookies).GetResponse().GetResponseStream())
                    {
                        Result result = jsSerializer.CleanAndDeserialize<Result>(response.Text());
                        Assert.AreEqual("foo", result.Value);
                        Assert.AreEqual(sessionId, result.Session);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(string.Format("Endpoint {0} failed {1} with {2}", address, "CreatePostJsonApp", ex.Message));
                }
            }

            if (failures.Count > 0)
            {
                Assert.Fail(string.Join("\r\n", failures.ToArray()));
            }
        }


        /// <summary>
        /// The primary CreateFilePost method uploads a Stream to an Http form handler.
        /// 
        /// 
        /// Required arguments:
        ///   string or Uri         requestUri      - Absolute Uri of resource
        ///   NameValueCollection   postData        - A NameValueCollection containing form fields to post with file data
        ///   Stream                fileData        - An open, positioned stream containing the file data.
        ///   string                fileName        - A name to assign to the file data and from which to infer <code>fileContentType</code> if necessary.
        /// 
        /// Optional arguments:
        ///   string                fileContentType - Optional. If omitted, the registry is queried using <code>fileName</code>. If content type is not available from registry, <code>application/octet-stream</code> will be submitted.
        ///   string                fileFieldName   - Optional. A identifier to represent the name of the input element supplying the data. If ommited the value <code>file</code> will be submitted.
        ///   CookieCollection      cookies         - Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.
        ///   NameValueCollection   headers         - Optional. Http headers to be added to the request.
        /// 
        /// </summary>
        [Test]
        public void Using_CreateFilePost_To_Upload_Data_From_Memory()
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

            var uploadResultPrototype = new
                {
                    postData = default(string),
                    fileFieldName = default(string),
                    fileName = default(string),
                    fileContentType = default(string),
                    fileContentLength = default(int),
                };

            Uri requestUri = NormalizeUri("UploadHandler.ashx");
            NameValueCollection postData = new NameValueCollection
                {
                    {"field1", "field1Value"},
                    {"chkBoxGrp1", "a"},
                    {"chkBoxGrp1", "b"}
                };


            const string content = "some text";
            Stream fileData = new MemoryStream(Encoding.UTF8.GetBytes(content));
            const string fileName = "TextFileFromMemory.txt";
            const string fileContentType = "text/plain";
            const string fileFieldName = "fileField";

            // am glossing over the use of cookies and headers as this has already been covered.

            HttpWebRequest request = RequestFactory.CreateFilePost(requestUri, postData, fileData, fileName, fileContentType, fileFieldName, null, null);

            try
            {
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    var response = jsSerializer.CleanAndDeserialize(stream.Text(), uploadResultPrototype);
                    Assert.AreEqual(9, response.fileContentLength);
                    Assert.AreEqual("text/plain", response.fileContentType);
                    Assert.AreEqual("fileField", response.fileFieldName);
                    Assert.AreEqual("TextFileFromMemory.txt", response.fileName);
                    Assert.AreEqual("field1=field1Value\r\nchkBoxGrp1=a,b\r\n", response.postData);


                }
            }
            catch (WebException ex)
            {
                throw WebRequestException.Create(ex);
            }


            // just send data and filename

            fileData.Position = 0;
            HttpWebRequest request2 = RequestFactory.CreateFilePost(requestUri, fileData, fileName);

            try
            {
                using (Stream stream = request2.GetResponse().GetResponseStream())
                {
                    var response = jsSerializer.CleanAndDeserialize(stream.Text(), uploadResultPrototype);
                    Assert.AreEqual(9, response.fileContentLength);
                    Assert.AreEqual("text/plain", response.fileContentType);
                    Assert.AreEqual("file", response.fileFieldName);
                    Assert.AreEqual("TextFileFromMemory.txt", response.fileName);
                }
            }
            catch (WebException ex)
            {
                throw WebRequestException.Create(ex);
            }

            
        }

        /// <summary>
        /// The secondary base CreateFilePost method accepts a physical file path, open the file stream and calls the primary CreateFilePost method.
        /// 
        /// 
        /// Required arguments:
        ///   string or Uri         requestUri      - Absolute Uri of resource
        ///   NameValueCollection   postData        - A NameValueCollection containing form fields to post with file data
        ///   string                fileName        - The physical path of the file to upload
        /// Optional arguments:
        ///   string                fileContentType - Optional. If omitted, the registry is queried using <code>fileName</code>. If content type is not available from registry, <code>application/octet-stream</code> will be submitted.
        ///   string                fileFieldName   - Optional. A identifier to represent the name of the input element supplying the data. If ommited the value <code>file</code> will be submitted.
        ///   CookieCollection      cookies         - Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.
        ///   NameValueCollection   headers         - Optional. Http headers to be added to the request.
        /// 
        /// </summary>
        [Test]
        public void Using_CreateFilePost_To_Upload_File_From_Disk()
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var uploadResultPrototype = new
                {
                    postData = default(string),
                    fileFieldName = default(string),
                    fileName = default(string),
                    fileContentType = default(string),
                    fileContentLength = default(int),
                };


            Uri requestUri = NormalizeUri("UploadHandler.ashx");
            NameValueCollection postData = new NameValueCollection
                {
                    {"field1", "field1Value"},
                    {"chkBoxGrp1", "a"},
                    {"chkBoxGrp1", "b"}
                };

            string fileName = Path.GetFullPath(@"TestFiles\TextFileFromDisk.txt");
            const string fileContentType = "text/plain";
            const string fileFieldName = "fileField";


            var request = RequestFactory.CreateFilePost(requestUri, postData, fileName, fileContentType, fileFieldName, null, null);


            try
            {
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    var response = jsSerializer.CleanAndDeserialize(stream.Text(), uploadResultPrototype);
                    Assert.AreEqual(12, response.fileContentLength); // content length is text length + BOM
                    Assert.AreEqual("text/plain", response.fileContentType);
                    Assert.AreEqual("fileField", response.fileFieldName);
                    Assert.AreEqual("TextFileFromDisk.txt", response.fileName);
                    Assert.AreEqual("field1=field1Value\r\nchkBoxGrp1=a,b\r\n", response.postData);


                }
            }
            catch (WebException ex)
            {
                throw WebRequestException.Create(ex);
            }


            // just send data and filename


            HttpWebRequest request2 = RequestFactory.CreateFilePost(requestUri, fileName);

            try
            {
                using (Stream stream = request2.GetResponse().GetResponseStream())
                {
                    var response = jsSerializer.CleanAndDeserialize(stream.Text(), uploadResultPrototype);
                    Assert.AreEqual(12, response.fileContentLength);// content length is text length + BOM
                    Assert.AreEqual("text/plain", response.fileContentType);
                    Assert.AreEqual("file", response.fileFieldName);
                    Assert.AreEqual("TextFileFromDisk.txt", response.fileName);
                }
            }
            catch (WebException ex)
            {
                throw WebRequestException.Create(ex);
            }

        }
    }
}