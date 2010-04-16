using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;

namespace Salient.Web.HttpLib.TestSite
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AjaxService
    {
        [OperationContract]
        public Result PutSessionVar(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input");
            }
            HttpContext.Current.Session["sessionVar"] = input;
            return new Result { Message = "Value set OK.", Session = HttpContext.Current.Session.SessionID, Value = input };

        }

        [OperationContract]
        public Result GetSessionVar()
        {
            var val = HttpContext.Current.Session["sessionVar"] as string;
            if (string.IsNullOrEmpty(val))
            {
                throw new InvalidOperationException(String.Format("Session var is empty for session {0}", HttpContext.Current.Session.SessionID));
            }
            return new Result { Message = "Value pull OK.", Session = HttpContext.Current.Session.SessionID, Value = val };
        }

        [OperationContract]
        public void ThrowException()
        {
            NotImplementedException innerInner = new NotImplementedException("Something you wish was implemented");
            Exception inner = new Exception("This is the inner exception");
            throw new InvalidOperationException("Intentional", inner);
        }

        [OperationContract]
        public void Noop()
        {
            // noop
        }
    }
}
