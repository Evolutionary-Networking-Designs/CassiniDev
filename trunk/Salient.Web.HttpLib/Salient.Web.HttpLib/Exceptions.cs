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
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

#endregion

namespace Salient.Web.HttpLib
{

    public class WebRequestException : Exception
    {
        private WebRequestException(WebException innerException)
            : base(innerException.Message, innerException)
        {
        }

        public WebExceptionDetail ExceptionDetail { get; private set; }

        public new string Message { get; private set; }

        public WebExceptionStatus Status
        {
            get { return InnerException.Status; }
        }

        public new WebException InnerException
        {
            get { return (WebException)base.InnerException; }
        }

        public static WebRequestException Create(WebException innerException)
        {
            WebRequestException result = new WebRequestException(innerException);
            // now decide what kind of error it is
            result.ExceptionDetail = WebExceptionDetail.Create(innerException);
            result.Message = result.ExceptionDetail.Message;
            return result;
        }



    }

    public sealed class WebExceptionDetail
    {
        public string ExceptionType { get; set; }

        public string HelpLink { get; set; }

        public WebExceptionDetail InnerException { get; set; }

        public string Message { get; set; }

        public string ResponseText { get; set; }

        public string StackTrace { get; set; }

        public string Type { get; set; }

        #region Factory Methods

        private static readonly string[] JsonFaultDetailTokens = new[]
            {"\"Message\"", "\"StackTrace\"", "\"ExceptionType\""};

        private static readonly Regex RxYellowScreenOfDeath =
            new Regex(@"<!--\s*(\n\[(?<type>\w+)\]:\s*(?<message>.*$))+(?<stack>(\n\s*at.*$)+)",
                      RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        public static WebExceptionDetail Create(WebException innerException)
        {
            // the first thing we want to do is get a seekable copy of the ResponseStream
            // so that we can examine it. This is necessary as the original response stream
            // is not seekable and will be consumed when we try to parse it. In case we find
            // nothing of interest we need to replace the exhausted stream on the inner exception.
            // get a seekable copy of the response stream
            MemoryStream exceptionStream = new MemoryStream(innerException.Response.GetResponseStream().Bytes());

            // the response will always be text 
            string responseText = exceptionStream.Text();

            exceptionStream.Position = 0;
            const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField;
            FieldInfo streamField = typeof (HttpWebResponse).GetField("m_ConnectStream", bf);
            streamField.SetValue(innerException.Response, exceptionStream);

            WebExceptionDetail exceptionDetail =
                CreateJsonDetail(innerException, responseText) ??
                CreateAspNetDetail(innerException, responseText) ??
                // TODO: create parsers for error pages
                /*
                CreateIis7Detail(innerException, responseText) ??
                CreateStandardDetail(innerException, responseText) ??
                */
                CreateUnknownDetail(innerException, responseText);

            return exceptionDetail;
        }

        //

        private static WebExceptionDetail CreateAspNetDetail(WebException innerException, string text)
        {
            WebExceptionDetail ex = null;
            if (IsAspNetError(text))
            {
                Match match = RxYellowScreenOfDeath.Match(text);
                if (match.Success)
                {
                    string stackTrace = match.Groups["stack"].Value;
                    int lastIndex = match.Groups["message"].Captures.Count - 1;
                    string message = match.Groups["message"].Captures[lastIndex].Value.Trim();
                    string exceptionType = match.Groups["type"].Captures[lastIndex].Value.Trim();

                    ex = new WebExceptionDetail
                        {ExceptionType = exceptionType, StackTrace = stackTrace, Message = message};

                    // now nest the inner exceptions, if any
                    WebExceptionDetail currentEx = ex;
                    for (int i = lastIndex - 1; i > -1; i--)
                    {
                        string innerMessage = match.Groups["message"].Captures[i].Value.Trim();
                        string innerExceptionType = match.Groups["type"].Captures[i].Value.Trim();

                        WebExceptionDetail inner = new WebExceptionDetail
                            {ExceptionType = innerExceptionType, Message = innerMessage};
                        currentEx.InnerException = inner;
                        currentEx = inner;
                    }
                }
            }
            // var json = new JavaScriptSerializer().Serialize(ex);
            return ex;
        }


      

        private static WebExceptionDetail CreateUnknownDetail(WebException innerException, string text)
        {
            return new WebExceptionDetail
                {
                    Message = innerException.Message,
                    ExceptionType = innerException.GetType().FullName,
                    ResponseText = text
                };
        }

        private static bool IsAspNetError(string text)
        {
            return RxYellowScreenOfDeath.IsMatch(text);
        }


       
        /// <summary>
        /// Identifies a JSON exception string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static bool IsJsonFault(string json)
        {
            foreach (string token in JsonFaultDetailTokens)
            {
                if (json.IndexOf(token) < 0)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// When a JSON enpoint throws any exception it presents to our HttpWebRequest as a generic WebException (500 Server Error). This does not present much information to work with in order to appropriately handle the exception.
        /// 
        /// To discover the actual exception that was thrown we need to catch the WebException and extract the content that was served from the WebException.Response.GetResponseStream().
        /// 
        /// This is always going to be text, but it can be of serveral different formats. It can be a simple status message, an ASP.Net Yellow Screen of Death, a blue IIS generic error page or, in the case of JSON endpoints, a JSON string representing the actual exception.
        /// 
        /// Each of these types of web errors leaves us to do much work in order to properly identify the exception. 
        /// 
        /// RequestFactory.CreateRequest(), which is the base method call by all of our helpers, handles all of this by trapping any WebException throw when the request is executed and re-throwing a more detailed exception.
        /// 
        /// While we will cover calling other types of endpoints and handling the possible exceptions later in this walkthrough, the primary focus at this point is JSON endpoints.
        /// 
        /// In this type of scenario, the JSON endpoint that thows the exception thinks it is being called by JavaScript so the exception information it provides is an 'untyped' JSON string like so:
        /// 
        ///  {
        ///      "ExceptionDetail": {
        ///          "HelpLink": null,
        ///          "InnerException": null,
        ///          "Message": "You fooed the bar",
        ///          "StackTrace": "(... lots of text ...)",
        ///          "Type": "System.InvalidOperationException"
        ///      },
        ///      "ExceptionType": "System.InvalidOperationException",
        ///      "Message": "You fooed the bar",
        ///      "StackTrace": "(... lots of text ...)",
        ///  }
        /// 
        /// 
        /// This JSON represents a serialized System.ServiceModel.Description.WebScriptEnablingBehavior+JsonFaultDetail. This is an internal nested type making it quite impractical to use the original type to deserialize the JSON.
        /// 
        /// This problem is easily solved by defining a public class of the same shape. As an added bonus we will derive our JsonFaultDetail from System.Exception so that it may be re-thrown.
        /// 
        ///  The next task is to determine if the JSON response from one of our helper methods is a JsonFault. We may come up with a more elegant way to do this later but for now we will simply scanning the string for expected properties in a static method on our public JsonFaultDetail class.
        /// 
        ///  With the requirements of a public destination type and the ability to identify a JsonDetailFault we can now add a static helper method, .Check(JSON), to JsonFaultDetail that will check the response stream for a JsonFaultDetail. If one is found, it is deserialized and thrown.
        /// 



        /// Exceptions generated in the WebRequest.CreateRequest() are most typically going to be a generic WebException '500 Internal Server Error'. Not much to chew on there. 
        /// But if, in the calling method, we catch the WebException, we can get the ResponseStream from the Exceptions's  .Response property. 
        /// 
        ///  In the current scenario, the service that threw the error thinks it is being called by JavaScript so the exception information it provides is an 'untyped' JSON string like so:
        /// 
        ///  {
        ///      "ExceptionDetail": {
        ///          "HelpLink": null,
        ///          "InnerException": null,
        ///          "Message": "You fooed the bar",
        ///          "StackTrace": "(... lots of text ...)",
        ///          "Type": "System.InvalidOperationException"
        ///      },
        ///      "ExceptionType": "System.InvalidOperationException",
        ///      "Message": "You fooed the bar",
        ///      "StackTrace": "(... lots of text ...)",
        ///  }
        /// 
        /// This JSON represents an internal nested class, System.ServiceModel.Description.WebScriptEnablingBehavior+JsonFaultDetail, making it quite impractical to use the original type as a template for deserialization. 
        /// This problem is easily solved by defining a public class of the same shape. As an added bonus we will derive our JsonFaultDetail from System.Exception so that it may be re-thrown.
        /// 
        ///  The next task is to determine if the JSON response from one of our helper methods is a JsonFault. We may come up with a more elegant way to do this later but for now I am simply scanning the string for all of the property names in a static method on our public JsonFaultDetail class.
        ///  e.g.
        ///  bool isJsonFault = JsonFaultDetail.IsJsonFault(responseText);
        /// 
        ///  With these prerequisites handled we can now add a static helper method, .Check(json), to JsonFaultDetail that will check the response stream for a JsonFaultDetail and throw if found.
        /// 
        /// </summary>
        /// <param name="innerException"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static WebExceptionDetail CreateJsonDetail(WebException innerException, string text)
        {
            WebExceptionDetail ex = null;
            if (IsJsonFault(text))
            {
                ex = new JavaScriptSerializer().Deserialize<WebExceptionDetail>(text);
            }
            return ex;
        }

        //private static bool IsIis7Error(string text)
        //{
        //    throw new NotImplementedException();
        //}

        //private static WebExceptionDetail CreateIis7Detail(WebException innerException, string text)
        //{
        //    WebExceptionDetail ex = null;
        //    if (IsIis7Error(text))
        //    {
        //        throw new NotImplementedException();
        //    }
        //    return ex;
        //}


        ///// <summary>
        ///// Check for generic 000 error detaul
        ///// </summary>
        ///// <param name="text"></param>
        ///// <returns></returns>
        //private static bool IsStandardError(string text)
        //{
        //    throw new NotImplementedException();
        //}


        //private static WebExceptionDetail CreateStandardDetail(WebException innerException, string text)
        //{
        //    WebExceptionDetail ex = null;
        //    if (IsStandardError(text))
        //    {
        //        throw new NotImplementedException();
        //    }
        //    return ex;
        //}
        #endregion
    }
}