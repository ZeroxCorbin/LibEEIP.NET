namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    public partial class Identity
    {
        public enum States
        {
            Nonexistent = 0,
            DeviceSelfTesting = 1,
            Standby = 2,
            Operational = 3,
            MajorRecoverableFault = 4,
            MajorUnrecoverableFault = 5,
            DefaultforGet_Attributes_All_service = 255
        }
    }
}
