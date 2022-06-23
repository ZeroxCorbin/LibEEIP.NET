namespace Sres.Net.EEIP
{
    using System;

    /// <summary>
    /// Implementation version 1.x backwards compatible <see cref="EIPClient"/>
    /// </summary>
    [Obsolete($"Use {nameof(EIPClient)} instead")]
    public class EEIPClient :
        EIPClient
    {
        // TODO
    }
}
