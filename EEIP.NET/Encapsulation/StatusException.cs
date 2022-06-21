namespace Sres.Net.EEIP.Encapsulation
{
    using System;

    public class StatusException :
        Exception
    {
        public StatusException(EncapsulationStatus status) :
            base(CIP.GeneralException.GetMessage(status, status.ToString()))
            => this.Status = status;

        public readonly EncapsulationStatus Status;
    }
}