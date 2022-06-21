namespace Sres.Net.EEIP.Encapsulation
{
    public enum SocketAddressItemType :
        ushort
    {
        OriginatorToTarget = 0x8000,
        TargetToOriginator = 0x8001
    }
}
