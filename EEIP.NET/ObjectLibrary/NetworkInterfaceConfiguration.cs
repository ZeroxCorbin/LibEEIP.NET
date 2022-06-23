using System;

namespace Sres.Net.EEIP.ObjectLibrary
{
    /// <summary>
    /// Chapter 5-3.2.2.5 Volume 2
    /// </summary>
    public struct NetworkInterfaceConfiguration
    {
        public uint IPAddress;
        public uint NetworkMask;
        public uint GatewayAddress;
        public uint NameServer;
        public uint NameServer2;
        public string DomainName;
    }


}
