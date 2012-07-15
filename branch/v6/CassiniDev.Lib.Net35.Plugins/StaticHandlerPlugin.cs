using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassiniDev.Plugins
{
    [Serializable]
    public class StaticHandlerPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns>signal to CassiniDev whether it should continue processing request</returns>
        public bool ProcessRequest(Request request)
        {
            
            request.GetConnection().WriteEntireResponseFromString(9,"","",true);
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        public void PrepareResponse(Request request)
        {
            
        }
    }
}
