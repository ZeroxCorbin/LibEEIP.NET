namespace Sres.Net.EEIP.Data
{
    using Sres.Net.EEIP.Encapsulation;

    /// <summary>
    /// <see cref="Command.UnRegisterSession"/> request. EtherNet/IP specification 2-4.5.
    /// </summary>
    public record UnregisterSessionRequest :
        Encapsulation
    {
        public UnregisterSessionRequest(uint sessionHandle) :
            base(Command.UnRegisterSession)
            => SessionHandle = sessionHandle;
    }
}