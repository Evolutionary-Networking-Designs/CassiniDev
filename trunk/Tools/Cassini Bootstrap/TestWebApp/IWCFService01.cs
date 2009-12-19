using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TestWebApp
{
    // NOTE: If you change the interface name "IWCFService01" here, you must also update the reference to "IWCFService01" in Web.config.
    [ServiceContract]
    public interface IWCFService01
    {
        [OperationContract]
        void DoWork();
    }
}
