namespace Sres.Net.EEIP.CIP
{
    public partial record EPath
    {
        public partial record Segment
        {
            public enum LogicalType :
                byte
            {
                ClassId,
                InstanceId = 0b100,
                MemberId = 0b1000,
                ConnectionPoint = 0b1100,
                AttributeId = 0b10000,
                Special = 0b10100,
                ServiceId = 0b11000,
                Reserved = 0b11100
            }
        }
    }
}