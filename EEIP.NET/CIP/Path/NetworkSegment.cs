namespace Sres.Net.EEIP.CIP.Path
{
    public abstract record NetworkSegment :
        Segment
    {
        protected NetworkSegment(NetworkType networkType) :
            base(SegmentType.Network)
        {
            this.NetworkType = networkType;
        }

        public NetworkType NetworkType { get; }
        public override byte Format => (byte)NetworkType;
    }
}
