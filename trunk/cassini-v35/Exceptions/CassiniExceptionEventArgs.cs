using System;

namespace Cassini
{
    [Serializable]
    public class CassiniExceptionEventArgs:EventArgs
    {
        private readonly CassiniException _exception;
        public CassiniException Exception
        {
            get
            {
                return _exception;
            }
        }
        public CassiniExceptionEventArgs(CassiniException exception)
        {
            this._exception = exception;
        }

        
    }
}