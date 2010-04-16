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
using System.Web;
using System.Web.Services;
using System.Web.UI;

#endregion

namespace Salient.Web.HttpLib.TestSite
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string query = Request.Url.Query;
            string header = Request.Headers["x-test-header"] ?? "";

            OutputLiteral.Text = String.Format("{0}{1}{2}", query, Environment.NewLine, header);
            
        }

        [WebMethod]
        public static TestClass PageMethod(string name, DateTime date)
        {
            TestClass result = new TestClass {Name = name, Date = date};
            return result;
        }

        /// <summary>
        /// Notice that we have enabled session state for this method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public static Result PutSessionVar(string input)
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
        public static Result GetSessionVar()
        {
            var val = HttpContext.Current.Session["sessionVar"] as string;
            if (string.IsNullOrEmpty(val))
            {
                throw new InvalidOperationException(String.Format("Session var is empty for session {0}", HttpContext.Current.Session.SessionID));
            }
            return new Result { Message = "Value pull OK.", Session = HttpContext.Current.Session.SessionID, Value = val };
        }

        [WebMethod]
        public static void ThrowException()
        {
            Exception inner = new Exception("This is the inner exception");
            throw new InvalidOperationException("Intentional", inner);
        }
    }
}