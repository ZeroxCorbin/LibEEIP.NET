namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System.Collections.Generic;
    using Sres.Net.EEIP.CIP.Path;

    /// <summary>
    /// Assembly object
    /// </summary>
    public class Assembly :
        ObjectBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client</param>
        public Assembly(EIPClient client) :
            base(client, ClassId)
        { }

        public const uint ClassId = 4;
        public const uint DataAttributeId = 3;

        public static EPath DataPath(uint instanceId) => EPath.ToObject(ClassId, instanceId, DataAttributeId);

        /// <summary>
        /// Reads assembly instance data
        /// </summary>
        /// <param name="id">Instance identifier</param>
        /// <returns>Data</returns>
        public IReadOnlyList<byte> GetInstanceData(ushort id) => GetInstanceAttributeSingle(DataAttributeId, id);

        /// <summary>
        /// Sets assembly instance data
        /// </summary>
        /// <param name="id">Instance identifier</param>
        /// <param name="value">Data</param>
        public void SetInstanceData(ushort id, byte[] value) => SetInstanceAttributeSingle(DataAttributeId, value, id);

    }
}
