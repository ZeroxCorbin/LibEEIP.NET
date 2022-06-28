namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    /// <summary>
    /// <see cref="Identity"/> state
    /// </summary>
    public enum IdentityState :
        byte
    {
        Nonexistent = 0,
        DeviceSelfTesting = 1,
        Standby = 2,
        Operational = 3,
        MajorRecoverableFault = 4,
        MajorUnrecoverableFault = 5,
        DefaultForGetAttributesAll = 255
    }
}
