namespace Sres.Net.EEIP.Encapsulation
{
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    public record Encapsulation :
        Byteable
    {
        public Encapsulation(Command command = Command.NOP, IByteable data = null)
        {
            this.Command = command;
            this.Data = data ?? Bytes.Empty;
        }

        public Encapsulation(IReadOnlyList<byte> bytes, int index = 0) :
            base(bytes, ref index)
        {
            Command = (Command)bytes.ToUshort(ref index);
            var dataLength = bytes.ToUshort(ref index);
            bytes.ValidateEnoughBytes(MinByteCount + dataLength, nameof(Encapsulation) + " data");
            SessionHandle = bytes.ToUint(ref index);
            Status = (EncapsulationStatus)bytes.ToUint(ref index);
            bytes.ToBytes(ref index, SenderContext, count: SenderContext.Length);
            Options = bytes.ToUint(ref index);
            var data = bytes.Segment(ref index, dataLength);
            Data = new Bytes(data);
            senderContext = new Bytes(SenderContext);
        }

        public Command Command { get; init; }
        public uint SessionHandle { get; internal set; }
        public EncapsulationStatus Status { get; init; }
        public byte[] SenderContext { get; } = new byte[8];
        private readonly Bytes senderContext;
        public uint Options { get; } = 0;
        public IByteable Data { get; }

        public CommonPacket GetCommonPacket(int dataIndex = 0) => GetCommonPacket(ref dataIndex);
        public CommonPacket GetCommonPacket(ref int dataIndex)
        {
            var data = Data.ToBytesReadOnly();
            return new(data, ref dataIndex);
        }

        public const byte MinByteCount = 24;
        public sealed override ushort ByteCount => (ushort)(MinByteCount + DataLength);
        public ushort DataLength => Data?.ByteCount ?? 0;

        protected sealed override void DoToBytes(byte[] bytes, ref int index)
        {
            ((ushort)Command).ToBytes(bytes, ref index);
            DataLength.ToBytes(bytes, ref index);
            SessionHandle.ToBytes(bytes, ref index);
            ((uint)Status).ToBytes(bytes, ref index);
            senderContext.ToBytes(bytes, ref index);
            Options.ToBytes(bytes, ref index);
            Data.ToBytes(bytes, ref index);
        }
    }
}
