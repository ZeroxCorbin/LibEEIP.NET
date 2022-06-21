using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using System.Net;
    using Sres.Net.EEIP.Data;
    using Sres.Net.EEIP.Encapsulation;

    /// <summary>
    /// Forward open request for implicit communication. Table 3-5.16 (Vol. 1).
    /// </summary>
    public record ForwardOpenRequest :
        MessageRouterRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="large">If true, maximum data size is 65535 otherwise 511 bytes</param>
        public ForwardOpenRequest(bool large, Originator originator, Target target) :
            base(
                large ?
                    LargeService :
                    SmallService,
                EPath.ToObject(ConnectionManager.ClassId, ObjectBase.DefaultInstanceId),
                new BytesConcatenation(
                    TimeTick.AsByteable(),
                    TimeoutTicks.AsByteable(),
                    (originator?.ConnectionToTarget.Id ?? throw new ArgumentNullException(nameof(originator))).AsByteable(),
                    (target?.ConnectionToOriginator.Id ?? throw new ArgumentNullException(nameof(target))).AsByteable(),
                    originator.ConnectionToTarget.SerialNumber.AsByteable(),
                    originator.VendorId.AsByteable(),
                    originator.SerialNumber.AsByteable(),
                    TimeoutMultiplier.AsByteable(),
                    Reserved,
                    originator.ConnectionToTarget.RequestedPacketRate.AsByteable(),
                    originator.ConnectionToTarget.GetNetworkParameters(large),
                    target.ConnectionToOriginator.RequestedPacketRate.AsByteable(),
                    target.ConnectionToOriginator.GetNetworkParameters(large),
                    TransportTypeAndTrigger.AsByteable(),
                    GetConnectionPath(originator, target)))
        {
            this.Large = large;
            this.Originator = originator;
            this.Target = target;
        }

        #region Address

        public SocketAddressItem GetOriginatorSocketAddress(IPAddress address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            var socketAddress = new SocketAddressItem(
                SocketAddressItemType.OriginatorToTarget,
                new(
                    this.Originator.ConnectionToTarget.Type == ConnectionType.Multicast ?
                        GetMulticastAddress(address) :
                        0,
                    this.Originator.ConnectionToTarget.Port));
            return socketAddress;
        }

        private static uint GetMulticastAddress(IPAddress address) => GetMulticastAddress(EEIP.Encapsulation.SocketAddress.GetAddress(address));

        private static uint GetMulticastAddress(uint address)
        {
            uint cip_Mcast_Base_Addr = 0xEFC00100;
            uint cip_Host_Mask = 0x3FF;
            uint netmask = 0;

            //Class A Network?
            if (address <= 0x7FFFFFFF)
                netmask = 0xFF000000;
            //Class B Network?
            if (address >= 0x80000000 && address <= 0xBFFFFFFF)
                netmask = 0xFFFF0000;
            //Class C Network?
            if (address >= 0xC0000000 && address <= 0xDFFFFFFF)
                netmask = 0xFFFFFF00;

            uint hostID = address & ~netmask;
            uint mcastIndex = hostID - 1;
            mcastIndex = mcastIndex & cip_Host_Mask;

            return (uint)(cip_Mcast_Base_Addr + mcastIndex * (uint)32);

        }

        #endregion

        public bool Large { get; }
        public const byte SmallService = 0x54;
        public const byte LargeService = 0x5B;
        public Originator Originator { get; }
        public Target Target { get; }
        /// <summary>
        /// Priority (0) and Time Tick
        /// </summary>
        public const byte TimeTick = 3;
        public const byte TimeoutTicks = 0xfa;
        public const byte TimeoutMultiplier = 3;
        private static readonly Bytes Reserved = new Bytes(0, 0, 0);
        /// <summary>
        /// Transport Type/Trigger
        /// </summary>
        /// <remarks>
        /// <code>
        /// X------- = 0= Client; 1= Server
        /// -XXX---- = Production Trigger, 0 = Cyclic, 1 = ChangeOfState, 2 = Application Object
        /// ----XXXX = Transport class, 0 = Class 0, 1 = Class 1, 2 = Class 2, 3 = Class 3
        /// </code>
        /// </remarks>
        public const byte TransportTypeAndTrigger = 1; // = Class 1

        internal EPath GetConnectionPath() => GetConnectionPath(Originator, Target);

        private static EPath GetConnectionPath(Originator originator, Target target)
            => GetConnectionPath(originator.ConnectionToTarget.Path, target.ConnectionToOriginator.OutputPath);

        private static EPath GetConnectionPath(EPath originator, EPath target) =>
            target is null ||
            ReferenceEquals(originator, target) ?
                originator :
                EPath.Concat(originator, target);
    }
}
