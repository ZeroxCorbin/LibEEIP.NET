namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// Table 2-3.3 Error Codes
    /// </summary>
    public enum EncapsulationStatus :
        uint
    {
        Success = 0x0000,
        InvalidCommand = 0x0001,
        InsufficientMemory = 0x0002,
        IncorrectData = 0x0003,
        InvalidSessionHandle = 0x0064,
        InvalidLength = 0x0065,
        UnsupportedEncapsulationProtocol = 0x0069
    }
}
