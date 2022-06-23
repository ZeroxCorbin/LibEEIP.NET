namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    /// <summary>
    /// Message Router Object - Class ID 2
    /// </summary>
    public partial class MessageRouter :
        ObjectBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client</param>
        public MessageRouter(EIPClient client) :
            base(client, ClassId)
        { }

        public const uint ClassId = 2;

        /// <summary>
        /// gets the Object List / Read "Message Router Object" Class Code 0x02 - Attribute ID 1
        /// </summary>
        public MessageRouterObjectList ObjectList
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(1);
                ushort number = (ushort)(byteArray[1] << 8 | byteArray[0]);
                MessageRouterObjectList returnValue = new()
                {
                    Number = number,
                    Classes = new ushort[number]
                };
                for (int i = 0; i < returnValue.Classes.Length; i++)
                {
                    returnValue.Classes[i] = (ushort)(byteArray[i*2+3] << 8 | byteArray[i*2+2]);
                }
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Maximum of connections supported / Read "Message Router Object" Class Code 0x02 - Attribute ID 2
        /// </summary>
        public ushort NumberAvailable
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(2);
                ushort returnValue;
                returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the number of active connections / Read "Message Router Object" Class Code 0x02 - Attribute ID 3
        /// </summary>
        public ushort NumberActive
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(3);
                ushort returnValue;
                returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the active connections / Read "Message Router Object" Class Code 0x02 - Attribute ID 4
        /// </summary>
        public ushort[] ActiveConnections
        {
            get
            {
                var byteArray = GetInstanceAttributeSingle(4);
                ushort[] returnValue = new ushort[byteArray.Count / 2];
                for (int i = 0; i < returnValue.Length; i++)
                {
                    returnValue[i] = (ushort)(byteArray[1 + 2*i] << 8 | byteArray[0 + 2*i]);
                }
                return returnValue;
           
            }
        }

    }
}
