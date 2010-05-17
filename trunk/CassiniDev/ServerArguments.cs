using System.Net;

namespace CassiniDev
{
    /// <summary>
    /// Server Constructor arguments
    /// </summary>
    public class ServerArguments
    {
        public int Port { get; set; }
        public string VirtualPath { get; set; }
        public string ApplicationPath { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public int TimeOut { get; set; }
    }
}