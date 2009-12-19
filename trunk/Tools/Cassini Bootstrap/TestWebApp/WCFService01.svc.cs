using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TestWebApp
{
    // NOTE: If you change the class name "WCFService01" here, you must also update the reference to "WCFService01" in Web.config.
    public class WCFService01 : IWCFService01
    {
        public void DoWork()
        {
        }
    }
}
