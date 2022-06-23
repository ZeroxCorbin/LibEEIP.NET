namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// Specifies the multiplier applied to the <see cref="IOConnection.RequestedPacketRate"/> to obtain the connection timeout value.
    /// Devices shall stop transmitting on a connection whenever the connection times out even if the pending close has been sent.
    /// CIP Table 3-5.12 Connection Timeout Multiplier Values.
    /// </summary>
    public enum ConnectionTimeoutMultiplier :
        byte
    {
        Value4,
        Value8,
        Value16,
        Value32,
        Value64,
        Value128,
        Value256,
        Value512
    }
}
