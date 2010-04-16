using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;

namespace Salient.Web.HttpLib.TestSite
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AjaxWcfService
    {
        private const string TestHeaderName = "x-test-header";
        // Add [WebGet] attribute to use HTTP GET
        [OperationContract]
        public TestClass EchoParams(string name, DateTime date)
        {
            return new TestClass() { Name = name, Date = date, Header = HttpContext.Current.Request.Headers[TestHeaderName] ?? "" };
        }

        [OperationContract]
        public TestClass EchoObject(TestClass testClass)
        {
            testClass.Header = HttpContext.Current.Request.Headers[TestHeaderName] ?? "";
            return testClass;
        }

        [OperationContract]
        public void ThrowException()
        {
            Exception inner = new Exception("This is the inner exception");
            throw new InvalidOperationException("Intentional",inner);
        }
    }
}
