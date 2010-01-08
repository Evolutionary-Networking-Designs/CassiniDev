using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CassiniDev.FixtureExamples.TestWeb
{
    [ServiceContract(Namespace = "http://TestWebApp")]
    public interface ITestWCFService
    {
        [OperationContract]
        string HelloWorld();

        [OperationContract]
        HelloWorldArgs HelloWorldWithArgsInOut(HelloWorldArgs args);

    }
}
