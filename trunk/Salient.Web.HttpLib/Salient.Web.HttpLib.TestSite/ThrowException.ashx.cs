using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Salient.Web.HttpLib.TestSite
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    public class ThrowException : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var x = new ArgumentException("HEY");
            var y = new InvalidOperationException("HEY", x);
            throw y;
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
