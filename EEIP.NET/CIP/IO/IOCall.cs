namespace Sres.Net.EEIP.CIP.IO
{
    using System;

    /// <summary>
    /// IO call for implicit messaging
    /// </summary>
    public abstract class IOCall :
        IDisposable
    {
        protected IOCall(ForwardOpenRequest forwardOpenRequest, ForwardOpenResponse forwardOpenResponse)
        {
            this.ForwardOpenRequest = forwardOpenRequest ?? throw new ArgumentNullException(nameof(forwardOpenRequest));
            this.ForwardOpenResponse = forwardOpenResponse ?? throw new ArgumentNullException(nameof(forwardOpenResponse));
            InputData = new(forwardOpenRequest.Originator.ConnectionToTarget.CreateData(nameof(OriginatorToTargetConnection)));
            OutputData = new(forwardOpenRequest.Target.ConnectionToOriginator.CreateData(nameof(TargetToOriginatorConnection)));
        }

        public ForwardOpenRequest ForwardOpenRequest { get; }
        public ForwardOpenResponse ForwardOpenResponse { get; }

        /// <summary>
        /// Data to be sent to target device
        /// </summary>
        public IOData InputData { get; }
        /// <summary>
        /// Raised before <see cref="InputData"/> is sent
        /// </summary>
        public event EventHandler<IOData> InputDataSending;
        /// <summary>
        /// Raised after <see cref="InputData"/> is sent
        /// </summary>
        public event EventHandler<IOData> InputDataSent;

        protected void OnInputDataSending(IOData data) => InputDataSending?.Invoke(this, data);
        protected void OnInputDataSent(IOData data) => InputDataSent?.Invoke(this, data);

        /// <summary>
        /// Data received from target device
        /// </summary>
        public IOData OutputData { get; }
        /// <summary>
        /// Raised when <see cref="OutputData"/> is received
        /// </summary>
        public event EventHandler<IOData> OutputDataReceived;

        protected void OnOutputDataReceived(IOData data) => OutputDataReceived?.Invoke(this, data);

        public virtual void Dispose()
        {
            InputDataSending = null;
            InputDataSent = null;
            OutputDataReceived = null;
        }
    }
}
