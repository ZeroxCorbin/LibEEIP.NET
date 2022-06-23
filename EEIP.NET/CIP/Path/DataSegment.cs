namespace Sres.Net.EEIP.CIP.Path
{
    public abstract record DataSegment :
        Segment
    {
        public DataSegment(DataType dataType) :
            base(SegmentType.Data)
            => this.DataType = dataType;

        public DataType DataType { get; }
        public override byte Format => (byte)DataType;
    }
}