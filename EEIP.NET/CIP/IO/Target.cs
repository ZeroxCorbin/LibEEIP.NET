namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// Implicit connection target = IO Adapter = device
    /// </summary>
    public record Target :
        IOConnectionEndPoint
    {
        public Target(ushort port = DefaultPort) :
            base(port)
        { }
    }
}
