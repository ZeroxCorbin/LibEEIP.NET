namespace Sres.Net.EEIP.CIP.IO
{
    /// <summary>
    /// Implicit connection target = IO Adapter = device
    /// </summary>
    public record Target
    {
        public Target(TargetToOriginatorConnection connectionToOriginator)
            => this.ConnectionToOriginator = connectionToOriginator;

        public TargetToOriginatorConnection ConnectionToOriginator
        {
            get => connectionToOriginator;
            init => connectionToOriginator = value ?? throw new System.ArgumentNullException(nameof(ConnectionToOriginator));
        }

        private TargetToOriginatorConnection connectionToOriginator;
    }
}
