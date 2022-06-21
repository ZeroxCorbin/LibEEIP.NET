using System;

namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    /// <summary>
    /// Chapter 5-3.2.2.5 Volume 2
    /// </summary>
    public struct NetworkInterfaceConfiguration
    {
        public UInt32 IPAddress;
        public UInt32 NetworkMask;
        public UInt32 GatewayAddress;
        public UInt32 NameServer;
        public UInt32 NameServer2;
        public string DomainName;
    }


}
