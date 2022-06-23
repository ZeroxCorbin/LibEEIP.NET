using System;
using System.Linq;
using System.Text;
using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.ObjectLibrary
{
    /// <summary>
    /// TCP/IP Interface Object - Class ID 0xF5
    /// </summary>
    public class TcpIpInterface :
        ObjectBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client</param>
        public TcpIpInterface(EIPClient client) :
            base(client, ClassId)
        { }

        public const uint ClassId = 0xF5;

        /// <summary>
        /// gets the Status / Read "TCP/IP Interface Object" Class Code 0xF5 - Attribute ID 1
        /// </summary>
        public InterfaceStatus Status
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(1);
                InterfaceStatus status = new InterfaceStatus();
                if ((byteArray[0] & 0x0F) == 0)
                    status.NotConfigured = true;
                if ((byteArray[0] & 0x0F) == 1)
                    status.ValidConfiguration = true;
                if ((byteArray[0] & 0x0F) == 2)
                    status.ValidManualConfiguration = true;
                if ((byteArray[0] & 0x10) != 0)
                    status.McastPending = true;
                return status;
            }
        }


        /// <summary>
        /// gets the Configuration capability / Read "TCP/IP Interface Object" Class Code 0xF5 - Attribute ID 2
        /// </summary>
        public InterfaceCapabilityFlags ConfigurationCapability
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(2);
                InterfaceCapabilityFlags configurationCapability = new InterfaceCapabilityFlags();
                if ((byteArray[0] & 0x01) != 0)
                    configurationCapability.BootPClient = true;
                if ((byteArray[0] & 0x02) != 0)
                    configurationCapability.DNSClient = true;
                if ((byteArray[0] & 0x04) != 0)
                    configurationCapability.DHCPClient = true;
                if ((byteArray[0] & 0x08) != 0)
                    configurationCapability.DHCPClient = true;
                if ((byteArray[0] & 0x10) != 0)
                    configurationCapability.ConfigurationSettable = true;
                return configurationCapability;
            }
        }

        /// <summary>
        /// gets the Path to the Physical Link object / Read "TCP/IP Interface Object" Class Code 0xF5 - Attribute ID 4
        /// </summary>
        public PhysicalLink PhysicalLinkObject
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(4).ToArray();
                PhysicalLink physicalLinkObject = new PhysicalLink();
                physicalLinkObject.PathSize = (ushort)(byteArray[1] << 8 | byteArray[0]);
                if (byteArray.Length > 2)
                    Buffer.BlockCopy(byteArray, 2, physicalLinkObject.Path, 0, byteArray.Length - 2);
                return physicalLinkObject;
            }
        }

        /// <summary>
        /// sets the Configuration control attribute / Write "TCP/IP Interface Object" Class Code 0xF5 - Attribute ID 3
        /// </summary>
        public InterfaceControlFlags ConfigurationControl
        {
            set
            {
                var valueToWrite = new byte[4];
                if (value.EnableBootP)
                    valueToWrite[0] = 1;
                if (value.EnableDHCP)
                    valueToWrite[0] = 2;
                if (value.EnableDNS)
                    valueToWrite[0] = (byte)(valueToWrite[0] | 0x10);
                SetInstanceAttributeSingle(3, valueToWrite);
            }
        }

        /// <summary>
        /// sets the TCP/IP Network interface Configuration / Write "TCP/IP Interface Object" Class Code 0xF5 - Attribute ID 5
        /// </summary>
        public NetworkInterfaceConfiguration InterfaceConfiguration
        {
            set
            {
                var valueToWrite = new byte[68];
                valueToWrite[0] = (byte)value.IPAddress;
                valueToWrite[1] = (byte)(value.IPAddress >> 8);
                valueToWrite[2] = (byte)(value.IPAddress >> 16);
                valueToWrite[3] = (byte)(value.IPAddress >> 24);
                valueToWrite[4] = (byte)value.NetworkMask;
                valueToWrite[5] = (byte)(value.NetworkMask >> 8);
                valueToWrite[6] = (byte)(value.NetworkMask >> 16);
                valueToWrite[7] = (byte)(value.NetworkMask >> 24);
                valueToWrite[8] = (byte)value.GatewayAddress;
                valueToWrite[9] = (byte)(value.GatewayAddress >> 8);
                valueToWrite[10] = (byte)(value.GatewayAddress >> 16);
                valueToWrite[11] = (byte)(value.GatewayAddress >> 24);
                valueToWrite[12] = (byte)value.NameServer;
                valueToWrite[13] = (byte)(value.NameServer >> 8);
                valueToWrite[14] = (byte)(value.NameServer >> 16);
                valueToWrite[15] = (byte)(value.NameServer >> 24);
                valueToWrite[16] = (byte)value.NameServer2;
                valueToWrite[17] = (byte)(value.NameServer2 >> 8);
                valueToWrite[18] = (byte)(value.NameServer2 >> 16);
                valueToWrite[19] = (byte)(value.NameServer2 >> 24);
                if (value.DomainName != null)
                {
                    var domainName = Encoding.ASCII.GetBytes(value.DomainName);
                    Buffer.BlockCopy(domainName, 0, valueToWrite, 20, domainName.Length);
                }
                SetInstanceAttributeSingle(5, valueToWrite);
            }
        }

    }
}
