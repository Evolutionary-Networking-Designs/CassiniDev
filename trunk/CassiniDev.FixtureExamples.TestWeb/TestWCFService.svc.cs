using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;

namespace CassiniDev.FixtureExamples.TestWeb
{
    
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TestWCFService : ITestWCFService
    {

        public string HelloWorld()
        {
            return "Hello World";
        }
        
        public HelloWorldArgs HelloWorldWithArgsInOut(HelloWorldArgs args)
        {
            args.Message = "you said: " + args.Message;
            return args;
        }
    }
}
