namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    public readonly record struct Revision
    {
        public byte Major { get; init; }
        public byte Minor { get; init; }

        public override string ToString() => $"{this.Major}.{this.Minor}";
    }
}
