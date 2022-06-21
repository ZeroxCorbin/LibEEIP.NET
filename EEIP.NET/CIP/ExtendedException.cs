namespace Sres.Net.EEIP.CIP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ExtendedException :
        GeneralException
    {
        public ExtendedException(byte status, IReadOnlyList<ushort> extendedStatuses, Func<ushort, string> getStatusMessage = null, GeneralException innerException = null) :
            base(
                GetMessage(status) +
                Environment.NewLine +
                string.Join(
                    Environment.NewLine,
                    extendedStatuses?.Select(extendedStatus => GetMessage(extendedStatus, getStatusMessage?.Invoke(extendedStatus)))),
                status,
                innerException)
            => ExtendedStatuses = extendedStatuses;

        public IReadOnlyList<ushort> ExtendedStatuses { get; }
    }
}
