using System.Linq;
using System.Text;

namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    /// <summary>
    /// Identity Object - Class ID 1
    /// </summary>
    /// <remarks>
    /// This object provides identification of and general information about the device. The Identity Object shall be present in all CIP products.
    /// If autonomous components of a device exist, use multiple instances of the Identity Object.
    /// </remarks>
    public class Identity :
        ObjectBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client</param>
        public Identity(EIPClient client) :
            base(client, ClassId)
        { }

        public const uint ClassId = 1;

        /// <summary>
        /// gets the Vendor ID / Read "Identity Object" Class Code 0x01 - Attribute ID 1
        /// </summary>
        public ushort VendorID
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(1);
                ushort returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Device Type / Read "Identity Object" Class Code 0x01 - Attribute ID 2
        /// </summary>
        public ushort DeviceType
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(2);
                ushort returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }


        /// <summary>
        /// gets the Product code / Read "Identity Object" Class Code 0x01 - Attribute ID 3
        /// </summary>
        public ushort ProductCode
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(3);
                ushort returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Revision / Read "Identity Object" Class Code 0x01 - Attribute ID 4
        /// </summary>
        /// <returns>Revision</returns>
        public Revision Revision
        {
            get
            {
                var bytes = GetInstanceAttributeSingle(4);
                int index = 0;
                return new(bytes, ref index);
            }
        }

        /// <summary>
        /// gets the Status / Read "Identity Object" Class Code 0x01 - Attribute ID 5
        /// </summary>
        public ushort Status
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(5);
                ushort returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Serial number / Read "Identity Object" Class Code 0x01 - Attribute ID 6
        /// </summary>
        public uint SerialNumber
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(6);
                uint returnValue = ((uint)byteArray[3] << 24 | (uint)byteArray[2] << 16 | (uint)byteArray[1] << 8 | (uint)byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Product Name / Read "Identity Object" Class Code 0x01 - Attribute ID 7
        /// </summary>
        public string ProductName
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(7);
                string returnValue = Encoding.UTF8.GetString(byteArray.ToArray());
                return returnValue;
            }
        }

        /// <summary>
        /// gets the State / Read "Identity Object" Class Code 0x01 - Attribute ID 8
        /// </summary>
        public IdentityState State
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(8);
                IdentityState returnValue = (IdentityState)byteArray[0];
                return returnValue;
            }
        }

        /// <summary>
        /// gets the State / Read "Identity Object" Class Code 0x01 - Attribute ID 9
        /// </summary>
        public ushort ConfigurationConsistencyValue
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(9);
                ushort returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Heartbeat intervall / Read "Identity Object" Class Code 0x01 - Attribute ID 10
        /// </summary>
        public byte HeartbeatInterval
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(10);
                byte returnValue = (byte)byteArray[0];
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Supported Language List / Read "Identity Object" Class Code 0x01 - Attribute ID 12
        /// </summary>
        public string[] SupportedLanguageList
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(12).ToArray();
                string[] returnValue = new string[byteArray.Length / 3];
                for (int i = 0; i < returnValue.Length; i++)
                {
                    var byteArray2 = new byte[3];
                    System.Buffer.BlockCopy(byteArray, i * 3, byteArray2, 0, 3);
                    returnValue[i] = Encoding.UTF8.GetString(byteArray2);
                }
                return returnValue;
            }
        }

        /// <summary>
        /// gets all instance attributes
        /// </summary>
        public IdentityInstance Instance
        {
            get
            {
                var bytes = GetInstanceAttributeAll().ToArray();
                int index = 0;
                return new(bytes, ref index);
            }
        }
    }
}
