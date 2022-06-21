namespace Sres.Net.EEIP.Encapsulation
{
    /// <summary>
    /// Table 2-3.2 Encapsulation Commands
    /// </summary>
    public enum Command :
        ushort
    {
        NOP = 0x0000,
        ListServices = 0x0004,
        ListIdentity = 0x0063,
        ListInterfaces = 0x0064,
        RegisterSession = 0x0065,
        UnRegisterSession = 0x0066,
        /// <summary>
        /// Request/Reply call
        /// </summary>
        SendRRData = 0x006F,
        SendUnitData = 0x0070,
        IndicateStatus = 0x0072,
        Cancel = 0x0073
    }
}
