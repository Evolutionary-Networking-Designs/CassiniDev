using System.Configuration;

namespace CassiniDev.Configuration
{
    ///<summary>
    ///</summary>
    public class CassiniDevProfileElement : ConfigurationElement
    {
        /// <summary>
        /// Port is used as profile selector
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = "*", IsKey = true, IsRequired = true)]
        public string Port
        {
            get
            {
                return (string)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("path")]
        public string Path
        {
            get
            {
                return (string)this["path"];
            }
            set
            {
                this["path"] = value;
            }
        }




        ///<summary>
        ///</summary>
        [ConfigurationProperty("hostName")]
        public string HostName
        {
            get
            {
                return (string)this["hostName"];
            }
            set
            {
                this["hostName"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("ip")]
        public string IpAddress
        {
            get
            {
                return (string)this["ip"];
            }
            set
            {
                this["ip"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("ipMode", DefaultValue = CassiniDev.IPMode.Loopback)]
        public IPMode IpMode
        {
            get
            {
                return (IPMode)this["ipMode"];
            }
            set
            {
                this["ipMode"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("v6", DefaultValue = false)]
        public bool IpV6
        {
            get
            {
                return (bool)this["v6"];
            }
            set
            {
                this["v6"] = value;
            }
        }


        ///<summary>
        ///</summary>
        [ConfigurationProperty("plugins")]
        public PluginElementCollection Plugins
        {
            get
            {
                return (PluginElementCollection)this["plugins"];
            }
        }
    }
}