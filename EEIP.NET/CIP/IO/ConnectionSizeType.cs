namespace Sres.Net.EEIP.CIP.IO
{
    public enum ConnectionSizeType :
        byte
    {
        /// <summary>
        /// With a fixed size connection, the amount of data shall be the size of specified in <see cref="IOConnection.DataSize"/>
        /// </summary>
        Fixed,
        /// <summary>
        /// With a variable size, the amount of data could be up to the size specified in <see cref="IOConnection.DataSize"/>
        /// </summary>
        Variable
    }
}
