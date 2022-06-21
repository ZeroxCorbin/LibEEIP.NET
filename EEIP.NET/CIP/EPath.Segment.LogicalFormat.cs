namespace Sres.Net.EEIP.CIP
{
    public partial record EPath
    {
        public partial record Segment
        {
            public enum LogicalFormat :
                byte
            {
                Bit8,
                Bit16,
                Bit32
            }
        }
    }
}