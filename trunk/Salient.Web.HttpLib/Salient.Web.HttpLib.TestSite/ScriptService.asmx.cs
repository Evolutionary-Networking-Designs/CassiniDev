using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace Salient.Web.HttpLib.TestSite
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService] // we uncommented the line! ;-)
    public class ScriptService : WebService
    {


        /// <summary>
        /// Notice that we have enabled session state for this method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public Result PutSessionVar(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input");
            }
            HttpContext.Current.Session["sessionVar"] = input;
            return new Result { Message = "Value set OK.", Session = HttpContext.Current.Session.SessionID, Value = input };

        }

        /// <summary>
        /// Notice that we have enabled session state for this method.
        /// </summary>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public Result GetSessionVar()
        {
            var val = HttpContext.Current.Session["sessionVar"] as string;
            if (string.IsNullOrEmpty(val))
            {
                throw new InvalidOperationException(String.Format("Session var is empty for session {0}", HttpContext.Current.Session.SessionID));
            }
            return new Result { Message = "Value pull OK.", Session = HttpContext.Current.Session.SessionID, Value = val };
        }


        [WebMethod]
        public void ThrowException()
        {
            Exception inner = new Exception("This is the inner exception");
            throw new InvalidOperationException("Intentional", inner);
        }
    }
}
