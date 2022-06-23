namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// <see cref="ConnectionRequest.Timeout"/> processing timeout.
    /// CIP 3-5.5.1.3 Connection Timing.
    /// </summary>
    public record Timeout :
        Byteable
    {
        public Timeout(byte timeTick = DefaultTimeTick, byte ticks = DefaultTicks)
        {
            TimeTick = timeTick;
            Ticks = ticks;
        }

        /// <summary>
        /// Default timeout: 8 * 250 ms = 2 s
        /// </summary>
        public static readonly Timeout Default = new();

        /// <summary>
        /// Timeout value
        /// </summary>
        /// <value>2 ^ <see cref="TimeTick"/> * <see cref="Ticks"/></value>
        public TimeSpan Value => TimeSpan.FromMilliseconds(Math.Pow(2,TimeTick) * Ticks);

        /// <summary>
        /// Time tick in ms
        /// </summary>
        /// <value>Only first 4 bits are used</value>
        public byte TimeTick
        {
            get => timeTick;
            init => timeTick = (byte)(value & 0b1111);
        }
        /// <summary>
        /// Default <see cref="TimeTick"/> = 3
        /// </summary>
        public const byte DefaultTimeTick = 3;

        public byte Ticks { get; init; }
        /// <summary>
        /// Default <see cref="Ticks"/> = 250
        /// </summary>
        public const byte DefaultTicks = 0xfa;

        public override ushort ByteCount => 2;

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            TimeTick.ToBytes(bytes, ref index);
            Ticks.ToBytes(bytes, ref index);
        }

        private byte timeTick;
    }
}
