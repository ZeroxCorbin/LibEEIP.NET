namespace Sres.Net.EEIP.CIP.Path
{
    public enum SegmentType :
        byte
    {
        //Port = 0b000,
        Logical = 0b001,
        Network = 0b010,
        //Symbolic = 0b011,
        Data = 0b100,
        //DataTypeConstructed = 0b101,
        //DataTypeElementary = 0b110,
        //Reserved = 0b111
    }
}
