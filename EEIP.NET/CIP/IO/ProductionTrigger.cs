namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// <see cref="ForwardOpenRequest.ProductionTrigger"/>.
    /// CIP Table 3-4.13.
    /// </summary>
    public enum ProductionTrigger :
        byte
    {
        /// <summary>
        /// The expiration of the Transmission Trigger Timer (<see cref="IOConnection.RequestedPacketRate"/>) triggers the data production
        /// </summary>
        Cyclic,
        /// <summary>
        /// Production occurs when a change of state is detected by the application or on expiration of the Transmission Trigger Timer
        /// </summary>
        ChangeOfState,
        /// <summary>
        /// The application decides when to trigger the production or on expiration of the Transmission Trigger Timer
        /// </summary>
        ApplicationObject
    }
}
