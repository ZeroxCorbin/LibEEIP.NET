namespace Sres.Net.EEIP.CIP.Path
{
    public enum LogicalType :
        byte
    {
        ClassId = 0b000,
        InstanceId = 0b001,
        MemberId = 0b010,
        ConnectionPoint = 0b011,
        AttributeId = 0b100,
        //Special = 0b101,
        //ServiceId = 0b110,
        //Reserved = 0b111
    }
}
