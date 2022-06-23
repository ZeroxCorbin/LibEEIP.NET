using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using System.Net;
    using Sres.Net.EEIP.CIP.Path;
    using Sres.Net.EEIP.Data;
    using Sres.Net.EEIP.Encapsulation;

    /// <summary>
    /// Forward open request for implicit communication. Table 3-5.16 (Vol. 1).
    /// </summary>
    public record ForwardOpenRequest :
        ConnectionRequest
    {
        #region Init

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="large"><see cref="Large"/></param>
        public ForwardOpenRequest(
            bool large,
            OriginatorToTargetConnection originatorToTargetConnection,
            TargetToOriginatorConnection targetToOriginatorConnection,
            Originator originator = null,
            Target target = null,
            ProductionTrigger productionTrigger = default,
            Timeout timeout = null,
            ConnectionTimeoutMultiplier connectionTimeoutMultiplier = IO.ConnectionTimeoutMultiplier.Value32) :
            this(
                large,
                originator ?? new(),
                originatorToTargetConnection,
                target ?? new(),
                targetToOriginatorConnection,
                GetTransportTypeAndTrigger(productionTrigger),
                timeout ?? Timeout.Default,
                connectionTimeoutMultiplier)
            => ProductionTrigger = productionTrigger;

        private ForwardOpenRequest(
            bool large,
            Originator originator,
            OriginatorToTargetConnection originatorToTargetConnection,
            Target target,
            TargetToOriginatorConnection targetToOriginatorConnection,
            byte transportTypeAndTrigger,
            Timeout timeout,
            ConnectionTimeoutMultiplier connectionTimeoutMultiplier) :
            base(
                GetService(large),
                EPath.ToObject(ConnectionManager.ClassId, ObjectBase.DefaultInstanceId),
                GetData(large, ref originator, originatorToTargetConnection, targetToOriginatorConnection, transportTypeAndTrigger, timeout, connectionTimeoutMultiplier),
                timeout)
        {
            Large = large;
            Originator = originator;
            OriginatorToTargetConnection = originatorToTargetConnection;
            originatorToTargetConnection.SetTimeoutMultiplier(connectionTimeoutMultiplier);
            Target = target;
            TargetToOriginatorConnection = targetToOriginatorConnection;
            targetToOriginatorConnection.SetTimeoutMultiplier(connectionTimeoutMultiplier);
            TransportTypeAndTrigger = transportTypeAndTrigger;
            ConnectionTimeoutMultiplier = connectionTimeoutMultiplier;
        }

        private static BytesConcatenation GetData(
            bool large,
            ref Originator originator,
            OriginatorToTargetConnection originatorToTargetConnection,
            TargetToOriginatorConnection targetToOriginatorConnection,
            byte transportTypeAndTrigger,
            Timeout timeout,
            ConnectionTimeoutMultiplier connectionTimeoutMultiplier)
        {
            if (originatorToTargetConnection is null)
                throw new ArgumentNullException(nameof(originatorToTargetConnection));
            if (targetToOriginatorConnection is null)
                throw new ArgumentNullException(nameof(targetToOriginatorConnection));
            return new(
                timeout,
                originatorToTargetConnection.Id.AsByteable(),
                targetToOriginatorConnection.Id.AsByteable(),
                originator.SerialNumber.AsByteable(),
                originator.VendorId.AsByteable(),
                originator.SerialNumber.AsByteable(),
                ((byte)connectionTimeoutMultiplier).AsByteable(),
                Reserved,
                originatorToTargetConnection.RequestedPacketRate.TotalMicrosecondsUint().AsByteable(),
                originatorToTargetConnection.GetNetworkParameters(large),
                targetToOriginatorConnection.RequestedPacketRate.TotalMicrosecondsUint().AsByteable(),
                targetToOriginatorConnection.GetNetworkParameters(large),
                transportTypeAndTrigger.AsByteable(),
                GetConnectionPath(originatorToTargetConnection, targetToOriginatorConnection));
        }

        #endregion

        #region Address

        public SocketAddressItem GetOriginatorToTargetSocketAddress(IPAddress address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            var socketAddress = new SocketAddressItem(
                SocketAddressItemType.OriginatorToTarget,
                new(
                    OriginatorToTargetConnection.Type == ConnectionType.Multicast ?
                        GetMulticastAddress(address) :
                        0,
                    Originator.Port));
            OriginatorToTargetConnection.SocketAddress = socketAddress.Value.EndPoint;
            return socketAddress;
        }

        private static uint GetMulticastAddress(IPAddress address) => GetMulticastAddress(EEIP.Encapsulation.SocketAddress.GetAddress(address));

        private static uint GetMulticastAddress(uint address)
        {
            const uint cip_Mcast_Base_Addr = 0xEFC00100;
            const uint cip_Host_Mask = 0x3FF;
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
            mcastIndex &= cip_Host_Mask;

            return cip_Mcast_Base_Addr + mcastIndex * 32;

        }

        #endregion

        /// <summary>
        /// Determines
        /// <see cref="MessageRouterRequest.Service"/> (<see cref="ServiceLarge"/>/<see cref="ServiceSmall"/>) and
        /// <see cref="MaxDataSize"/> (<see cref="MaxDataSizeLarge"/>/<see cref="MaxDataSizeSmall"/>)
        /// </summary>
        public bool Large { get; }

        #region Service

        /// <summary>
        /// Forward Open service code
        /// </summary>
        public const byte ServiceSmall = 0x54;
        /// <summary>
        /// Large Forward Open service code
        /// </summary>
        public const byte ServiceLarge = 0x5B;

        public static byte GetService(bool large) => large ?
            ServiceLarge :
            ServiceSmall;

        #endregion

        #region Data

        /// <summary>
        /// Maximum connection data size
        /// </summary>
        public ushort MaxDataSize => GetMaxDataSize(Large);

        public const ushort MaxDataSizeSmall = 511;
        public const ushort MaxDataSizeLarge = ushort.MaxValue;

        public static ushort GetMaxDataSize(bool large) => large ?
            MaxDataSizeLarge :
            MaxDataSizeSmall;

        public void ValidateDataSize()
        {
            ushort maxDataSize = MaxDataSize;
            OriginatorToTargetConnection.ValidateDataSize(maxDataSize);
            TargetToOriginatorConnection.ValidateDataSize(maxDataSize);
        }

        internal void CreateData()
        {
            ushort maxDataSize = MaxDataSize;
            this.OriginatorToTargetConnection.CreateData(maxDataSize);
            this.TargetToOriginatorConnection.CreateData(maxDataSize);
        }

        #endregion

        #region Originator <-> Target

        public Originator Originator { get; }
        public OriginatorToTargetConnection OriginatorToTargetConnection { get; }
        public Target Target { get; }
        public TargetToOriginatorConnection TargetToOriginatorConnection { get; }

        #endregion

        public ConnectionTimeoutMultiplier ConnectionTimeoutMultiplier { get; }

        #region TransportTypeAndTrigger

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
        public byte TransportTypeAndTrigger { get; }

        public const Direction Direction = IO.Direction.Client;
        public ProductionTrigger ProductionTrigger { get; }
        public const TransportClass TransportClass = IO.TransportClass.Class1;

        private static byte GetTransportTypeAndTrigger(ProductionTrigger productionTrigger) => (byte)(
            ((byte)TransportClass) |
            ((byte)((byte)productionTrigger << 4)) |
            (byte)Direction << 7
            );

        #endregion

        #region ConnectionPath

        internal EPath GetConnectionPath()
            => GetConnectionPath(OriginatorToTargetConnection, TargetToOriginatorConnection);

        private static EPath GetConnectionPath(IOConnection originatorToTargetConnection, IOConnection targetToOriginatorConnection)
            => GetConnectionPath(originatorToTargetConnection.Path, targetToOriginatorConnection.Path);

        // CIP Table 3-5.13: Encoded Application Path Ordering
        private static EPath GetConnectionPath(EPath originator, EPath target) =>
            target is null ||
            ReferenceEquals(originator, target) ?
                originator :
                EPath.Concat(originator, target);

        #endregion

        private static readonly Bytes Reserved = new Bytes(0, 0, 0);
    }
}
