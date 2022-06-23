namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// <see cref="ForwardOpenRequest.Direction"/>.
    /// CIP Table 3-4.12.
    /// </summary>
    public enum Direction :
        byte
    {
        Client,
        Server
    }
}
