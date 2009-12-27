using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Cassini
{
    public static class IPUtility
    {
        /// <summary>
        /// Returns first available port on the specified IP address. The port scan excludes ports that are open on ANY loopback adapter. 
        /// If the address upon which a port is requested is an 'ANY' address all ports that are open on ANY IP are excluded.
        /// </summary>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <param name="ip">The IP address upon which to search for available port.</param>
        /// <param name="includeIdlePorts">If true includes ports in TIME_WAIT state in results. TIME_WAIT state is typically cool down period for recently released ports.</param>
        /// <returns></returns>
        public static ushort GetAvailablePort(UInt16 rangeStart, UInt16 rangeEnd, IPAddress ip, bool includeIdlePorts)
        {

            IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();

            // if the ip we want a port on is an 'any' port we need to exclude all ports that are active on any IP
            bool isIpAny = IPAddress.Any.Equals(ip) || IPAddress.IPv6Any.Equals(ip);
            
            // get all active ports on specified IP. 

            List<ushort> usedPorts = new List<ushort>();

            // when scanning for ports in use, if a port is open on any loopback it is excluded

            usedPorts.AddRange(from n in ipProps.GetActiveTcpConnections()
                               where 
                               n.LocalEndPoint.Port >= rangeStart 
                               && n.LocalEndPoint.Port <= rangeEnd
                               && (isIpAny || n.LocalEndPoint.Address.Equals(ip) || IPAddress.Loopback.Equals(n.LocalEndPoint.Address) || IPAddress.IPv6Loopback.Equals(n.LocalEndPoint.Address))
                               && (!includeIdlePorts || n.State != TcpState.TimeWait)
                               select (ushort)n.LocalEndPoint.Port);

            usedPorts.AddRange(from n in ipProps.GetActiveTcpListeners()
                               where n.Port >= rangeStart && n.Port <= rangeEnd
                               && (isIpAny || n.Address.Equals(ip) || IPAddress.Loopback.Equals(n.Address) || IPAddress.IPv6Loopback.Equals(n.Address))
                               select (ushort)n.Port);
            
            usedPorts.AddRange(from n in ipProps.GetActiveUdpListeners()
                               where n.Port >= rangeStart && n.Port <= rangeEnd
                               && (isIpAny || n.Address.Equals(ip) || IPAddress.Loopback.Equals(n.Address) || IPAddress.IPv6Loopback.Equals(n.Address))
                               select (ushort)n.Port);

            usedPorts.Sort();


            for (ushort port = rangeStart; port <= rangeEnd; port++)
            {
                if(!usedPorts.Contains(port))
                {
                    return port;
                }
            }

            return 0;
            
        }
    }
}
