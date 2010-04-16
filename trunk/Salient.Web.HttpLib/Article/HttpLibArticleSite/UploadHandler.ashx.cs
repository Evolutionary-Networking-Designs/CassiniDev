using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace HttpLibArticleSite
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    public class UploadHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            StringBuilder sbForm = new StringBuilder();
            foreach (string key in request.Form.AllKeys)
            {
                sbForm.AppendLine(string.Format("{0}={1}", key, request.Form[key]));
            }

            var file = request.Files[0];

            var uploadResult = new
            {
                postData = sbForm.ToString(),
                fileContentLength = file.ContentLength,
                fileName = file.FileName,
                fileContentType = file.ContentType,
                fileFieldName = request.Files.AllKeys[0]
            };

            // just logging the request, not writing the file to disk

            var response = context.Response;
            response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.ContentType = "text/plain";
            response.Write(new JavaScriptSerializer().Serialize(uploadResult));
            response.Flush();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
