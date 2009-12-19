using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

namespace Cassini
{
    [Serializable]
    public class CassiniException : HttpException
    {
        public CassiniException()
        {
            
        }
        public CassiniException(string message)
            : base(message)
        {
            
        }
        public CassiniException(string message, int hr)
            : base(message, hr)
        {
            
        }
        public CassiniException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
        public CassiniException(int httpCode, string message, Exception innerException)
            : base(httpCode, message, innerException)
        {
            
        }
        public CassiniException(int httpCode, string message)
            : base(httpCode, message)
        {
            
        }
        public CassiniException(int httpCode, string message, int hr)
            : base(httpCode, message, hr)
        {
            
        }
        protected CassiniException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            
        }
        
    }
}
