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
    public partial class Identity :
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
                var byteArray = GetInstanceAttributeSingle(4);
                Revision returnValue = new()
                {
                    Major = byteArray[0],
                    Minor = byteArray[1]
                };
                return returnValue;
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
        public States State
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(8);
                States returnValue = (States)byteArray[0];
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
        /// gets all class attributes
        /// </summary>
        public ClassAttributes Class
        {
            get
            {
                var byteArray = GetClassAttributeAll();
                ClassAttributes returnValue;
                returnValue.Revision = (ushort)(byteArray[1] << 8 | byteArray[0]);
                returnValue.MaxInstance = (ushort)(byteArray[3] << 8 | byteArray[2]);
                returnValue.MaxIDNumberOfClassAttributes = (ushort)(byteArray[5] << 8 | byteArray[4]);
                returnValue.MaxIDNumberOfInstanceAttributes = (ushort)(byteArray[7] << 8 | byteArray[6]);
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
                var byteArray = GetInstanceAttributeAll().ToArray();
                byte productNameLength = byteArray[14];
                var productName = new byte[productNameLength];
                System.Buffer.BlockCopy(byteArray, 15, productName, 0, productName.Length);
                return new()
                {
                    VendorId = (ushort)(byteArray[1] << 8 | byteArray[0]),
                    DeviceType = (ushort)(byteArray[3] << 8 | byteArray[2]),
                    ProductCode = (ushort)(byteArray[5] << 8 | byteArray[4]),
                    Revision = new()
                    {
                        Major = byteArray[6],
                        Minor = byteArray[7]
                    },
                    Status = (ushort)(byteArray[9] << 8 | byteArray[8]),
                    SerialNumber = ((uint)byteArray[13] << 24 | (uint)byteArray[12] << 16 | (uint)byteArray[11] << 8 | (uint)byteArray[10]),
                    ProductName = Encoding.UTF8.GetString(productName)
                };
            }
        }
    }
}
