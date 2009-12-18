using System;

namespace Cassini.CommandLine
{
    class CassiniArgs
    {
        
        [Argument(ArgumentType.Required, HelpText = "The physical location of the root directory. Default is the current path.")]
        // ReSharper disable InconsistentNaming
            public string path;
        // ReSharper restore InconsistentNaming
        [Argument(ArgumentType.AtMostOnce, HelpText = "The port to listen on. Default is 8088", DefaultValue = 8088)]
        // ReSharper disable InconsistentNaming
            public int port;
        // ReSharper restore InconsistentNaming
        [Argument(ArgumentType.AtMostOnce, HelpText = "The virtual root of the site. Default is '/'", DefaultValue = "/")]
        // ReSharper disable InconsistentNaming
            public string vroot;
        // ReSharper restore InconsistentNaming
    }
}