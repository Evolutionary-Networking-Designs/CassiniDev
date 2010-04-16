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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.Win32;

#endregion

namespace Salient.Web.HttpLib
{
    /// <summary>
    /// A group of static methods that leverate HttpWebRequest.
    /// Useful for automating client requests for the testing of Http handlers.
    /// </summary>
    public class RequestFactory
    {
        #region Http Request

        /// <summary>
        /// Executes an HttpWebRequest.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="method">
        /// <para>The http method of the request.</para>
        /// <para>Currently supported options are <code>HttpMethod.Post</code> and <code>HttpMethod.Get</code></para>
        /// </param>
        /// <param name="contentType">
        /// <para>The content-type of the request.</para>
        /// <para>Currently supported options are <code>ContentType.None</code>, <code>ContentType.ApplicationForm</code>, <code>ContentType.ApplicationJson</code> and <code>ContentType.TextJson</code></para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateRequest(Uri requestUri, HttpMethod method, ContentType contentType,
                                                   object postData, CookieContainer cookies, NameValueCollection headers)
        {
            if (method == HttpMethod.Get && postData != null)
            {
                requestUri = AppendQueryString(requestUri, ToQueryString(postData));
            }

            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(requestUri);


            req.Method = method.ToString().ToUpperInvariant();
            req.ContentType = contentType.AsString();
            req.CookieContainer = cookies;

            if (headers != null)
            {
                // set the headers
                foreach (string key in headers.AllKeys)
                {
                    string[] values = headers.GetValues(key);
                    if (values != null)
                        foreach (string value in values)
                        {
                            req.Headers.Add(key, value);
                        }
                }
            }

            if (postData != null)
            {
                switch (method)
                {
                    case HttpMethod.Get:
                        // noop - we already added postdata to query string
                        break;

                    case HttpMethod.Post:
                        byte[] data = new byte[] {};

                        switch (contentType)
                        {
                            case ContentType.ApplicationJson:
                            case ContentType.TextJson:
                                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                                data = Encoding.UTF8.GetBytes(jsSerializer.Serialize(postData));
                                break;
                            case ContentType.None:
                            case ContentType.ApplicationForm:
                                data = Encoding.UTF8.GetBytes(ToQueryString(postData));
                                break;
                        }

                        req.ContentLength = data.Length;

                        using (Stream reqStream = req.GetRequestStream())
                        {
                            reqStream.Write(data, 0, data.Length);
                        }
                        break;
                }
            }

            return req;
        }

        #region Convenience Overloads of CreateRequest

        #region Get

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(string requestUri)
        {
            return CreateGet(requestUri);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(string requestUri, object postData)
        {
            return CreateGet(requestUri, postData);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(string requestUri, object postData, CookieContainer cookies)
        {
            return CreateGet(requestUri, postData, cookies, null);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(string requestUri, object postData, CookieContainer cookies,
                                               NameValueCollection headers)
        {
            return CreateGet(new Uri(requestUri), postData, cookies, headers);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(Uri requestUri)
        {
            return CreateGet(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(Uri requestUri, object postData)
        {
            return CreateGet(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(Uri requestUri, object postData, CookieContainer cookies)
        {
            return CreateGet(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with empty content-type.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGet(Uri requestUri, object postData, CookieContainer cookies,
                                               NameValueCollection headers)
        {
            return CreateRequest(requestUri, HttpMethod.Get, ContentType.None, postData, cookies, headers);
        }

        #endregion

        #region Json Get

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(string requestUri)
        {
            return CreateGetJsonApp(requestUri);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(string requestUri, object postData)
        {
            return CreateGetJsonApp(requestUri, postData);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(string requestUri, object postData, CookieContainer cookies)
        {
            return CreateGetJsonApp(requestUri, postData, cookies, null);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(string requestUri, object postData, CookieContainer cookies,
                                                      NameValueCollection headers)
        {
            return CreateGetJsonApp(new Uri(requestUri), postData, cookies, headers);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(Uri requestUri)
        {
            return CreateGetJsonApp(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(Uri requestUri, object postData)
        {
            return CreateGetJsonApp(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(Uri requestUri, object postData, CookieContainer cookies)
        {
            return CreateGetJsonApp(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'application/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonApp(Uri requestUri, object postData, CookieContainer cookies,
                                                      NameValueCollection headers)
        {
            return CreateRequest(requestUri, HttpMethod.Get, ContentType.ApplicationJson, postData, cookies, headers);
        }

        #endregion

        #region Json Text Get

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(string requestUri)
        {
            return CreateGetJsonText(requestUri);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(string requestUri, object postData)
        {
            return CreateGetJsonText(requestUri, postData);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(string requestUri, object postData, CookieContainer cookies)
        {
            return CreateGetJsonText(requestUri, postData, cookies, null);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(string requestUri, object postData, CookieContainer cookies,
                                                       NameValueCollection headers)
        {
            return CreateGetJsonText(new Uri(requestUri), postData, cookies, headers);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(Uri requestUri)
        {
            return CreateGetJsonText(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(Uri requestUri, object postData)
        {
            return CreateGetJsonText(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(Uri requestUri, object postData, CookieContainer cookies)
        {
            return CreateGetJsonText(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest GET with content-type 'text/json; charset=UTF-8'.
        /// <paramref name="postData"/> is serialized as a query string and appended to the Uri.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateGetJsonText(Uri requestUri, object postData, CookieContainer cookies,
                                                       NameValueCollection headers)
        {
            return CreateRequest(requestUri, HttpMethod.Get, ContentType.TextJson, postData, cookies, headers);
        }

        #endregion

        #region Form Post

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(Uri requestUri)
        {
            return CreatePostForm(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a query string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(Uri requestUri, object postData)
        {
            return CreatePostForm(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a query string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(Uri requestUri, object postData, CookieContainer cookies)
        {
            return CreatePostForm(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a query string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(Uri requestUri, object postData, CookieContainer cookies,
                                                    NameValueCollection headers)
        {
            return CreateRequest(requestUri, HttpMethod.Post, ContentType.ApplicationForm, postData, cookies, headers);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(string requestUri)
        {
            return CreatePostForm(requestUri, null, null, null);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a query string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(string requestUri, object postData)
        {
            return CreatePostForm(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a query string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(string requestUri, object postData, CookieContainer cookies)
        {
            return CreatePostForm(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/x-www-form-urlencoded; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a query string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostForm(string requestUri, object postData, CookieContainer cookies,
                                                    NameValueCollection headers)
        {
            return CreatePostForm(new Uri(requestUri), postData, cookies, headers);
        }

        #endregion

        #region Json Post

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(Uri requestUri)
        {
            return CreatePostJsonApp(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(Uri requestUri, object postData)
        {
            return CreatePostJsonApp(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(Uri requestUri, object postData, CookieContainer cookies)
        {
            return CreatePostJsonApp(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(Uri requestUri, object postData, CookieContainer cookies,
                                                       NameValueCollection headers)
        {
            return CreateRequest(requestUri, HttpMethod.Post, ContentType.ApplicationJson, postData, cookies, headers);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(string requestUri)
        {
            return CreatePostJsonApp(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(string requestUri, object postData)
        {
            return CreatePostJsonApp(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(string requestUri, object postData, CookieContainer cookies)
        {
            return CreatePostJsonApp(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "application/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonApp(string requestUri, object postData, CookieContainer cookies,
                                                       NameValueCollection headers)
        {
            return CreatePostJsonApp(new Uri(requestUri), postData, cookies, headers);
        }

        #endregion

        #region Json Text Post

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(string requestUri)
        {
            return CreatePostJsonText(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(string requestUri, object postData)
        {
            return CreatePostJsonText(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(string requestUri, object postData, CookieContainer cookies)
        {
            return CreatePostJsonText(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(string requestUri, object postData, CookieContainer cookies,
                                                        NameValueCollection headers)
        {
            return CreatePostJsonText(new Uri(requestUri), postData, cookies, headers);
        }


        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(Uri requestUri)
        {
            return CreatePostJsonText(requestUri, null, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(Uri requestUri, object postData)
        {
            return CreatePostJsonText(requestUri, postData, null, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(Uri requestUri, object postData, CookieContainer cookies)
        {
            return CreatePostJsonText(requestUri, postData, cookies, null);
        }

        /// <summary>
        /// Builds and returns an HttpWebRequest POST with content-type "text/json; charset=UTF-8".
        /// <paramref name="postData"/> is serialized as a JSON string and posted as request body.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">
        /// <para>Optional, A NameValueCollection or object. Anonymous types are acceptable.</para>
        /// <para>The object will be serialized appropriately for the request method and content-type before being applied to the request. i.e.</para>
        /// <para>For form 'Post' requests, the postData object will be serialized into a Url encoded key-value string an streamed into the request body.</para>
        /// <para>For a JSON 'Post', it will be JSON serialized and streamed into the request body.</para>
        /// <para>For a 'Get' request it will be Url encoded into a query string and intelligently appended to the Uri. If the Uri is bare, the query string will be appended with a '?'. If the Uri already has a query string the new query will be appended with a '&'.</para>
        /// <para>Note: any object that is shaped like the target method's arguments is acceptable.</para>
        /// <para>Conveniently this includes anonymous types. For a 'Get' or form 'Post' a NameValue collection may be appropriate as it is able to accept multiple values for a single key to fully simulate the possible shape of a form.</para>
        /// <para>When creating anonymous types as input parameters, you are not required to prototype the target type exactly. You MUST prototype non-nullable properties, including value types and structs, but you may omit any nullable properties, including Nullable&lt;T> and reference types, that you do not need to send.</para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(string input)</code> would be <code>var postData = new { input = "foo" };</code></para>
        /// <para>An anonymous type suitable as postData for the method <code>public Result PutSessionVar(Result input)</code> would be <code>var postData = new { input = new Result() };</code> or <code>var postData = new { input = new { Message = "message", Session = "session", Value = "value" } };</code>  or <code>var postData = new { input = new { Message = "message" } };</code></para>
        /// </param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreatePostJsonText(Uri requestUri, object postData, CookieContainer cookies,
                                                        NameValueCollection headers)
        {
            return CreateRequest(requestUri, HttpMethod.Post, ContentType.TextJson, postData, cookies, headers);
        }

        #endregion

        #endregion

        #endregion

        #region File Post

        /// Reference: 
        /// http://tools.ietf.org/html/rfc1867
        /// http://tools.ietf.org/html/rfc2388
        /// http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.2
        /// <summary>
        /// Uploads a stream using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
        /// <param name="fileData">An open, positioned stream containing the file data</param>
        /// <param name="fileName">A name to assign to the file data and from which to infer <code>fileContentType</code> if necessary</param>
        /// <param name="fileContentType">Optional. If omitted, registry is queried using <paramref name="fileName"/>. 
        /// If content type is not available from registry, application/octet-stream will be submitted.</param>
        /// <param name="fileFieldName">Optional, a form field name to assign to the uploaded file data. 
        /// If ommited the value 'file' will be submitted.</param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(Uri requestUri, NameValueCollection postData, Stream fileData,
                                                    string fileName, string fileContentType, string fileFieldName,
                                                    CookieContainer cookies, NameValueCollection headers)
        {

            if(requestUri==null)
            {
                throw new ArgumentNullException("requestUri");

            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");

            }

            if (fileData == null)
            {
                throw new ArgumentNullException("fileData");

            }
            HttpWebRequest webrequest = (HttpWebRequest) WebRequest.Create(requestUri);

            // determine content type if not supplied
            string ctype;
            fileContentType = string.IsNullOrEmpty(fileContentType)
                                  ? TryGetContentType(fileName, out ctype) ? ctype : "application/octet-stream"
                                  : fileContentType;

            fileFieldName = string.IsNullOrEmpty(fileFieldName) ? "file" : fileFieldName;

            if (headers != null)
            {
                // set the headers
                foreach (string key in headers.AllKeys)
                {
                    string[] values = headers.GetValues(key);
                    if (values != null)
                        foreach (string value in values)
                        {
                            webrequest.Headers.Add(key, value);
                        }
                }
            }

            webrequest.Method = "POST";

            if (cookies != null)
            {
                webrequest.CookieContainer = cookies;
            }

            string boundary = "----------" + DateTime.Now.Ticks.ToString("x", CultureInfo.InvariantCulture);

            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;

            StringBuilder sbHeader = new StringBuilder();

            // add form fields, if any
            if (postData != null)
            {
                foreach (string key in postData.AllKeys)
                {
                    string[] values = postData.GetValues(key);
                    if (values != null)
                        foreach (string value in values)
                        {
                            sbHeader.AppendFormat("--{0}\r\n", boundary);
                            sbHeader.AppendFormat("Content-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}\r\n", key,
                                                  value);
                        }
                }
            }


            if (fileData != null)
            {
                sbHeader.AppendFormat("--{0}\r\n", boundary);
                sbHeader.AppendFormat("Content-Disposition: form-data; name=\"{0}\"; {1}\r\n", fileFieldName,
                                      string.IsNullOrEmpty(fileName)
                                          ?
                                              ""
                                          : string.Format(CultureInfo.InvariantCulture, "filename=\"{0}\";",
                                                          Path.GetFileName(fileName)));

                sbHeader.AppendFormat("Content-Type: {0}\r\n\r\n", fileContentType);
            }

            byte[] header = Encoding.UTF8.GetBytes(sbHeader.ToString());
            byte[] footer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            long contentLength = header.Length + (fileData != null ? fileData.Length : 0) + footer.Length;

            webrequest.ContentLength = contentLength;

            using (Stream requestStream = webrequest.GetRequestStream())
            {
                requestStream.Write(header, 0, header.Length);


                if (fileData != null)
                {
                    // write the file data, if any
                    byte[] buffer = new Byte[checked((uint) Math.Min(4096, (int) fileData.Length))];
                    int bytesRead;
                    while ((bytesRead = fileData.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                }

                // write footer
                requestStream.Write(footer, 0, footer.Length);

                return webrequest;
            }
        }

        #region Convenience overloads of CreateFilePost

        /// <summary>
        /// Uploads a file using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
        /// <param name="fileName">The physical path of the file to upload</param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(string requestUri, NameValueCollection postData, string fileName)
        {
            return CreateFilePost(requestUri, postData, null, fileName, null, null, null, null);
        }


        /// <summary>
        /// Uploads a file using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
        /// <param name="fileName">The physical path of the file to upload</param>
        /// <param name="fileContentType">Optional. If omitted, registry is queried using <paramref name="fileName"/>. 
        /// If content type is not available from registry, application/octet-stream will be submitted.</param>
        /// <param name="fileFieldName">Optional, a form field name to assign to the uploaded file data. 
        /// If ommited the value 'file' will be submitted.</param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(string requestUri, NameValueCollection postData, string fileName,
                                                    string fileContentType, string fileFieldName,
                                                    CookieContainer cookies, NameValueCollection headers)
        {
            return CreateFilePost(requestUri, postData, null, fileName, fileContentType, fileFieldName, cookies, headers);
        }

        /// <summary>
        /// Uploads a file using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="fileName">The physical path of the file to upload</param>
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(string requestUri, string fileName)
        {
            return CreateFilePost(requestUri, null, null, fileName, null, null, null, null);
        }


        /// <summary>
        /// Uploads a file using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="fileName">The physical path of the file to upload</param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(Uri requestUri, string fileName)
        {
            return CreateFilePost(requestUri, null, fileName, null, null, null, null);
        }

        /// <summary>
        /// Uploads a file using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
        /// <param name="fileName">The physical path of the file to upload</param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(Uri requestUri, NameValueCollection postData, string fileName)
        {
            return CreateFilePost(requestUri, postData, fileName, null, null, null, null);
        }

        /// <summary>
        /// Uploads a file using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
        /// <param name="fileName">The physical path of the file to upload</param>
        /// <param name="fileContentType">Optional. If omitted, registry is queried using <paramref name="fileName"/>. 
        /// If content type is not available from registry, application/octet-stream will be submitted.</param>
        /// <param name="fileFieldName">Optional, a form field name to assign to the uploaded file data. 
        /// If ommited the value 'file' will be submitted.</param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(Uri requestUri, NameValueCollection postData, string fileName,
                                                    string fileContentType, string fileFieldName,
                                                    CookieContainer cookies, NameValueCollection headers)
        {
            using (FileStream fileData = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return CreateFilePost(requestUri, postData, fileData, fileName, fileContentType, fileFieldName, cookies,
                                      headers);
            }
        }



 

        /// <summary>
        /// Uploads a stream using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="fileData">An open, positioned stream containing the file data</param>
        /// <param name="fileName">A name to assign to the file data and from which to infer <code>fileContentType</code> if necessary</param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(string requestUri, Stream fileData, string fileName)
        {
            return CreateFilePost(requestUri, null, fileData, fileName, null, null, null, null);
        }

        /// <summary>
        /// Uploads a stream using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
        /// <param name="fileData">An open, positioned stream containing the file data</param>
        /// <param name="fileName">A name to assign to the file data and from which to infer <code>fileContentType</code> if necessary</param>
        /// <param name="fileContentType">Optional. If omitted, registry is queried using <paramref name="fileName"/>. 
        /// If content type is not available from registry, application/octet-stream will be submitted.</param>
        /// <param name="fileFieldName">Optional, a form field name to assign to the uploaded file data. 
        /// If ommited the value 'file' will be submitted.</param>
        /// <param name="cookies">
        /// <para>Optional. Sharing a CookieCollection between requests is required to maintain session state, FormsAuthentication tickets and other cookies.</para>
        /// </param>
        /// <param name="headers">
        /// <para>Optional. Http headers to be added to the request.</para>
        /// </param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(string requestUri, NameValueCollection postData, Stream fileData,
                                                    string fileName, string fileContentType, string fileFieldName,
                                                    CookieContainer cookies, NameValueCollection headers)
        {
            return CreateFilePost(new Uri(requestUri), postData, fileData, fileName, fileContentType, fileFieldName,
                                  cookies, headers);
        }


        /// <summary>
        /// Uploads a stream using a multipart/form-data POST.
        /// </summary>
        /// <param name="requestUri">
        /// <para>Absolute Uri of resource</para>
        /// </param>
        /// <param name="fileData">An open, positioned stream containing the file data</param>
        /// <param name="fileName">A name to assign to the file data.</param>
        /// <returns></returns>
        public static HttpWebRequest CreateFilePost(Uri requestUri, Stream fileData, string fileName)
        {
            return CreateFilePost(requestUri, null, fileData, fileName, null, null, null, null);
        }

        #endregion

        #endregion

        #region Misc utility methods

        /// <summary>
        /// Builds an UrlEncoded query string from a NameValueCollection or an object.
        /// Result is suitable for use in a Url or as post body.
        /// </summary>
        /// <param name="postData">A NameValueCollection or object. Anonymous types are acceptable.</param>
        /// <returns></returns>
        public static string ToQueryString(object postData)
        {
            string encoded;
            NameValueCollection nvc = postData as NameValueCollection;
            if (nvc != null)
            {
                encoded = string.Join("&",
                                      Array.ConvertAll(nvc.AllKeys,
                                                       key =>
                                                       string.Format(CultureInfo.InvariantCulture, "{0}={1}",
                                                                     HttpUtility.UrlEncode(key),
                                                                     HttpUtility.UrlEncode(nvc[key]))));
            }
            else
            {
                // try to create a query string from any object that can be viewed as a Key:Value container.

                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                string json = jsSerializer.Serialize(postData);

                Dictionary<string, object> jsob =
                    (Dictionary<string, object>) jsSerializer.DeserializeObject(json);

                List<string> items = new List<string>();

                foreach (KeyValuePair<string, object> q in jsob)
                {
                    items.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", HttpUtility.UrlEncode(q.Key),
                                            HttpUtility.UrlEncode(q.Value.ToString())));
                }
                encoded = string.Join("&", items.ToArray());
            }

            return encoded;
        }


        /// <summary>
        /// Appends a query string to an Uri using the appropriate operator.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Uri AppendQueryString(string uri, string query)
        {
            Uri newUri = new Uri(uri);
            return AppendQueryString(newUri, query);
        }

        /// <summary>
        /// Appends a query string to an Uri using the appropriate operator.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Uri AppendQueryString(Uri uri, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return uri;
            }
            return new Uri(uri + (string.IsNullOrEmpty(uri.Query) ? "?" : "&") + query);
        }


        /// <summary>
        /// Attempts to query registry for content-type of suppied file name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static bool TryGetContentType(string fileName, out string contentType)
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type");

                if (key != null)
                {
                    foreach (string keyName in key.GetSubKeyNames())
                    {
                        RegistryKey subKey = key.OpenSubKey(keyName);
                        if (subKey != null)
                        {
                            string subKeyValue = (string) subKey.GetValue("Extension");

                            if (!string.IsNullOrEmpty(subKeyValue))
                            {
                                if (string.Compare(Path.GetExtension(fileName).ToUpperInvariant(),
                                                   subKeyValue.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase) ==
                                    0)
                                {
                                    contentType = keyName;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                // fail silently
                // TODO: rethrow registry access denied errors
            }
            // ReSharper restore EmptyGeneralCatchClause
            contentType = "";
            return false;
        }

        #endregion
    }

}