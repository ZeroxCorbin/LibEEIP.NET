namespace Sres.Net.EEIP.CIP.IO
{
    using System;
    using System.Threading;

    /// <summary>
    /// IO context of implicit messaging
    /// </summary>
    public abstract class IOContext :
        IDisposable
    {
        private protected IOContext(bool backwardsCompatible, ForwardOpenRequest forwardOpenRequest, ForwardOpenResponse forwardOpenResponse)
        {
            ForwardOpenRequest = forwardOpenRequest ?? throw new ArgumentNullException(nameof(forwardOpenRequest));
            ForwardOpenResponse = forwardOpenResponse ?? throw new ArgumentNullException(nameof(forwardOpenResponse));
            ForwardOpenRequest.CreateData();
            ForwardCloseTimeout = forwardOpenRequest.Timeout;
            if (!backwardsCompatible &&
                !OriginatorToTargetConnection.IsNull &&
                forwardOpenResponse.OriginatorToTargetActualPacketRate > TimeSpan.Zero)
            {
                InitSendDataToTargetRequested();
            }
        }

        #region Originator <-> Target

        protected ForwardOpenRequest ForwardOpenRequest { get; }
        protected ForwardOpenResponse ForwardOpenResponse { get; }

        public Originator Originator => ForwardOpenRequest.Originator;
        public OriginatorToTargetConnection OriginatorToTargetConnection => ForwardOpenRequest.OriginatorToTargetConnection;
        public Target Target => ForwardOpenRequest.Target;
        public TargetToOriginatorConnection TargetToOriginatorConnection => ForwardOpenRequest.TargetToOriginatorConnection;

        #endregion

        #region Sending

        /// <summary>
        /// Whether <see cref="SendDataToTarget"/> is in progress
        /// </summary>
        public bool SendingDataToTarget { get; private set; }

        protected virtual void InitSendDataToTarget(bool backwardsCompatible)
        {
            if (backwardsCompatible)
                StartSendDataToTargetAtActualPackageRate();
        }

        /// <summary>
        /// Sends <see cref="OriginatorToTargetConnection"/>.<see cref="IOConnection.Data"/> to target
        /// </summary>
        /// <returns>Whether sending was not stropped already</returns>
        /// <exception cref="InvalidOperationException"><see cref="OriginatorToTargetConnection"/><see cref="IOConnection.Type"/> is <see cref="ConnectionType.Null"/></exception>
        public bool SendDataToTarget()
        {
            lock (sendLock)
            {
                if (!send)
                    return false;
                if (OriginatorToTargetConnection.IsNull)
                    throw new InvalidOperationException("Sending data to target is not allowed with connection type " + nameof(ConnectionType.Null));
                OriginatorToTargetConnection.OnDataSending(this);
                DoSendDataToTarget();
                OriginatorToTargetConnection.OnDataSent(this);
                return true;
            }
        }

        protected abstract void DoSendDataToTarget();

        /// <summary>
        /// Start sending for <see cref="EEIPClient"/>
        /// </summary>
        internal void StartSendDataToTargetAtActualPackageRate()
        {
            if (ForwardOpenRequest.ProductionTrigger == ProductionTrigger.Cyclic)
            {
                sendingThread = new Thread(SendDataToTargetAtActualPackageRate);
                sendingThread.Start();
            }
        }

        /// <summary>
        /// Sending for <see cref="EEIPClient"/>
        /// </summary>
        private void SendDataToTargetAtActualPackageRate()
        {
            while (send)
            {
                SendDataToTarget();
                Thread.Sleep(ForwardOpenResponse.OriginatorToTargetActualPacketRate);
            }
        }

        /// <summary>
        /// Waits for <see cref="SendDataToTarget"/> to finish
        /// </summary>
        public abstract void FinishSendDataToTarget();

        private void InitSendDataToTargetRequested() => originatorToTargetActualPacketRateTimer =
            new(_ => OnSendDataToTargetRequested(), null, TimeSpan.Zero, ForwardOpenResponse.OriginatorToTargetActualPacketRate);

        protected virtual void OnSendDataToTargetRequested()
        {
            var timeSinceLastSend = OriginatorToTargetConnection.TimeSinceLastDataTransfer;
            // do next
            if (!SendingDataToTarget &&
                (
                    timeSinceLastSend is null ||
                    timeSinceLastSend >= ForwardOpenResponse.OriginatorToTargetActualPacketRate
                ))
            {
                SendDataToTarget();
            }
            // plan next
            else
            {
                originatorToTargetActualPacketRateTimer.Change(
                    System.Threading.Timeout.InfiniteTimeSpan,
                    System.Threading.Timeout.InfiniteTimeSpan);
                FinishSendDataToTarget();
                timeSinceLastSend = OriginatorToTargetConnection.TimeSinceLastDataTransfer;
                var timeToNextSend = ForwardOpenResponse.OriginatorToTargetActualPacketRate - timeSinceLastSend.Value;
                this.originatorToTargetActualPacketRateTimer.Change(
                    timeToNextSend, ForwardOpenResponse.OriginatorToTargetActualPacketRate);
            }
        }

        internal void StopSendDataToTarget()
        {
            // wait for last send
            lock (sendLock)
            {
                send = false;
            }
        }

        private bool send = true;
        private readonly object sendLock = new();
        private Thread sendingThread;
        private Timer originatorToTargetActualPacketRateTimer;

        #endregion

        #region Receiving

        protected void BeginReceiveDataFromTarget()
        {
            lock (receiveLock)
            {
                if (!receive)
                    return;
                TargetToOriginatorConnection.OnDataReceiving(this);
                DoBeginReceiveDataFromTarget();
            }
        }

        protected abstract void DoBeginReceiveDataFromTarget();

        protected void EndReceiveDataFromTarget(IAsyncResult result)
        {
            lock (receiveLock)
            {
                if (!DoEndReceiveDataFromTarget(result))
                    return;
                TargetToOriginatorConnection.OnDataReceived(this);
                BeginReceiveDataFromTarget();
            }
        }

        protected abstract bool DoEndReceiveDataFromTarget(IAsyncResult result);

        private void StopReceiveDataFromTarget()
        {
            // wait for last receive
            lock (receiveLock)
            {
                receive = false;
            }
        }

        private bool receive = true;
        private readonly object receiveLock = new();

        #endregion

        #region Close

        /// <summary>
        /// <see cref="ForwardCloseRequest"/>.<see cref="ConnectionRequest.Timeout"/> for <see cref="CreateForwardCloseRequest"/>
        /// </summary>
        /// <value>
        /// Default: <see cref="ForwardCloseRequest"/>.<see cref="ConnectionRequest.Timeout"/>.
        /// null means <see cref="Timeout.Default"/>.
        /// </value>
        public Timeout ForwardCloseTimeout { get; set; }

        public ForwardCloseRequest CreateForwardCloseRequest() => new(
            ForwardOpenRequest.GetConnectionPath(),
            ForwardOpenResponse,
            ForwardCloseTimeout);

        public virtual void Dispose()
        {
            originatorToTargetActualPacketRateTimer?.Dispose();
            StopSendDataToTarget();
            StopReceiveDataFromTarget();
            ForwardOpenRequest.OriginatorToTargetConnection.Dispose();
            ForwardOpenRequest.TargetToOriginatorConnection.Dispose();
        }

        #endregion
    }
}
