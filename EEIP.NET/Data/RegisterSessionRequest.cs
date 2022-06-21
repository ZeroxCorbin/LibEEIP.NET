namespace Sres.Net.EEIP.Data
{
    using Sres.Net.EEIP.Encapsulation;

    /// <summary>
    /// <see cref="Command.RegisterSession"/> request. EtherNet/IP specification 2-4.4.
    /// </summary>
    public record RegisterSessionRequest :
        Encapsulation
    {
        private RegisterSessionRequest() :
            base(
                Command.RegisterSession,
                Data)
        { }

        public static readonly RegisterSessionRequest Instance = new();

        private static new readonly Bytes Data = new Bytes(
            // Protocol version (should be set to 1)
            1, 0,
            // Session options shall be set to "0"
            0, 0);
    }
}