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
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        [WebMethod]
        public TestClass EchoParams(string name, DateTime date)
        {
            return new TestClass() { Name = name, Date = date };
        }

        [WebMethod]
        public TestClass EchoObject(TestClass testClass)
        {
            return testClass;
        }

        [WebMethod]
        public void ThrowException()
        {
            Exception inner = new Exception("This is the inner exception");
            throw new InvalidOperationException("Intentional", inner);
        }
    }
}
