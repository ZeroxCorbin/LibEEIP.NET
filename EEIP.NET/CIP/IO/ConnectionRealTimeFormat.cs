namespace Sres.Net.EEIP.CIP.IO
{
    public enum ConnectionRealTimeFormat : byte
    {
        Modeless = 0,
        ZeroLength = 1,
        Heartbeat = 2,
        Header32Bit = 3
    }
}
