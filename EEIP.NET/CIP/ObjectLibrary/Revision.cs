namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Sres.Net.EEIP.Data;

    public record Revision :
        Byteable
    {
        [JsonConstructor]
        public Revision() { }

        public Revision(byte major, byte minor)
        {
            this.Major = major;
            this.Minor = minor;
        }

        public Revision(IReadOnlyList<byte> bytes, ref int index) :
            base(bytes, ref index)
        {
            Major = bytes[index++];
            Minor = bytes[index++];
        }

        public byte Major { get; set; }
        public byte Minor { get; set; }

        public const int ByteCountStatic = 2;
        public override ushort ByteCount => ByteCountStatic;

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            bytes[index++] = Major;
            bytes[index++] = Minor;
        }

        public override string ToString() => $"{this.Major}.{this.Minor}";
    }
}
