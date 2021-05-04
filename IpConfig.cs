using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace P2PChat
{
    class IpConfig
    {
        public static List<IPAddress> GetAllLocalHosts()
        {
            List<IPAddress> LocalIp = new List<IPAddress>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    LocalIp.Add(ip);
                }
            }

            return LocalIp;
        }

        public static IPAddress CountBroadcastIPINV(IPAddress ipAddress)
        {
            IPAddress BroadCastMask = GetSubnetMask(ipAddress);
            IPAddress broadcastIP = IPAddress.Parse(BroadCastMask.ToString());

            broadcastIP.Address = ipAddress.Address | BroadCastMask.Address;

            return broadcastIP;
        }

        // Return inversed mask
        private static IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            byte[] maskBytes = unicastIPAddressInformation.IPv4Mask.GetAddressBytes();

                            for (int i = 0; i < maskBytes.Length; i++)
                                maskBytes[i] = (byte)~maskBytes[i];

                            IPAddress SubnetMast = new IPAddress(maskBytes);

                            return SubnetMast;
                        }
                    }
                }
            }
            throw new ArgumentException($"Can't find subnetmask for IP address '{address}'");
        }
    }
}
