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
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

#endregion

namespace Salient.Web.HttpLib.TestSite
{
    public class PostHandler : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            SetCachingNever(context);
            context.Response.Cookies.Add(new HttpCookie("fromServer", "fromServer"));
            try
            {
                if (context.Request.Files.Count > 0)
                {
                    HandleUpload(context);
                }
                else
                {
                    string op = context.Request["op"];
                    switch (op.ToUpperInvariant())
                    {
                        case "THROWEXCEPTION":
                            ArgumentException innerInner = new ArgumentException("inner inner");
                            Exception inner = new Exception("This is the inner exception", innerInner);
                            throw new InvalidOperationException("Intentional", inner);
                        /// Looks like:
                        ///{
                        ///    "ExceptionType": "InvalidOperationException",
                        ///    "HelpLink": null,
                        ///    "InnerException": {
                        ///        "ExceptionType": "Exception",
                        ///        "HelpLink": null,
                        ///        "InnerException": {
                        ///            "ExceptionType": "ArgumentException",
                        ///            "HelpLink": null,
                        ///            "InnerException": null,
                        ///            "Message": "inner inner",
                        ///            "ResponseText": null,
                        ///            "StackTrace": null,
                        ///            "Type": null
                        ///        },
                        ///        "Message": "This is the inner exception",
                        ///        "ResponseText": null,
                        ///        "StackTrace": null,
                        ///        "Type": null
                        ///    },
                        ///    "Message": "Intentional",
                        ///    "ResponseText": null,
                        ///    "StackTrace": "\n   at Salient.Web.HttpLib.TestSite.PostHandler.ProcessRequest(HttpContext context) in C:\\Projects\\salient......",
                        ///    "Type": null
                        ///}
                        case "POSTFORM":
                            HandleForm(context);
                            break;
                        default:
                            throw new NotImplementedException(op);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Intentional")
                {
                    throw;
                }
                WriteJsonResponse(context, new { ExceptionType = ex.GetType().FullName, ex.Message, ex.StackTrace });

            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        #endregion

        private static void HandleForm(HttpContext context)
        {
            WriteJsonResponse(context,
                              new
                                  {
                                      Status = "OK",
                                      Field1 = context.Request["field1"],
                                      Field2 = context.Request["field2"]
                                  });
        }

        private static void HandleUpload(HttpContext context)
        {
            string fileField = context.Request.Files.AllKeys[0];
            HttpPostedFile file = context.Request.Files[0];
            string path = context.Server.MapPath("bin/" + Path.GetFileName(file.FileName));
            File.WriteAllBytes(path, ReadFully(file.InputStream));
            string xHeader = context.Request.Headers["x-test-header"] ?? "";
            string fieldValue = context.Request["field"] ?? "";
            WriteJsonResponse(context,
                              new
                                  {
                                      file.FileName,
                                      file.ContentType,
                                      file.ContentLength,
                                      Text = File.ReadAllText(path),
                                      Header = xHeader,
                                      Field = fieldValue,
                                      FileField = fileField
                                  });
        }

        public static void SetCachingNever(HttpContext context)
        {
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
        }

        public static void WriteJsonResponse(HttpContext context, object data)
        {
            string json = new JavaScriptSerializer().Serialize(data);
            context.Response.ContentType = "text/plain";
            context.Response.Write(json);
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
    }
}