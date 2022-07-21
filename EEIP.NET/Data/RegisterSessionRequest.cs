namespace Sres.Net.EEIP.Data
{
    using Sres.Net.EEIP.Encapsulation;

    /// <summary>
    /// <see cref="Command.RegisterSession"/> request.
    /// EIP 2-4.4 RegisterSession.
    /// </summary>
    public record RegisterSessionRequest :
        Encapsulation
    {
        private RegisterSessionRequest() :
            base(
                Command.RegisterSession,
                new Bytes(
                    // Protocol version (should be set to 1)
                    1, 0,
                    // Session options shall be set to "0"
                    0, 0))
        { }

        public static readonly RegisterSessionRequest Instance = new();
    }
}