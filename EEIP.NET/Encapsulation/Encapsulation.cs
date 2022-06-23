namespace Sres.Net.EEIP.Encapsulation
{
    using System;
    using System.Collections.Generic;
    using Sres.Net.EEIP.Data;

    public partial record Encapsulation :
        Byteable
    {
        public Encapsulation(Command command = Command.NOP, IByteable data = null)
        {
            this.Command = command;
            this.Data = data ?? Bytes.Empty;
        }

        public Encapsulation(IReadOnlyList<byte> bytes)
        {
            bytes.ValidateEnoughBytes(HeaderLength, nameof(Encapsulation) + " header");
            int index = 0;
            Command = (Command)bytes.ToUshort(ref index);
            var dataLength = bytes.ToUshort(ref index);
            bytes.ValidateEnoughBytes(HeaderLength + dataLength, nameof(Encapsulation) + " data");
            SessionHandle = bytes.ToUint(ref index);
            Status = (EncapsulationStatus)bytes.ToUint(ref index);
            bytes.Copy(ref index, SenderContext, 0, SenderContext.Length);
            Options = bytes.ToUint(ref index);
            var data = bytes.Segment(ref index, dataLength);
            Data = new Bytes(data);
            senderContext = new Bytes(SenderContext);
        }

        public const byte HeaderLength = 24;
        public Command Command { get; init; }
        public uint SessionHandle { get; internal set; }
        public EncapsulationStatus Status { get; init; }
        public byte[] SenderContext { get; } = new byte[8];
        private readonly Bytes senderContext;
        public uint Options { get; } = 0;
        public IByteable Data { get; }

        public ushort DataLength => Data.ByteCount;

        public sealed override ushort ByteCount => (ushort)(HeaderLength + Data.ByteCount);

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
