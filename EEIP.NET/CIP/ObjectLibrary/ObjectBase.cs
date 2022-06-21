namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System.Collections.Generic;

    /// <summary>
    /// Object library object base
    /// </summary>
    public abstract class ObjectBase
    {
        protected ObjectBase(EEIPClient client, ushort classId, ushort instanceId = DefaultInstanceId)
        {
            this.Client = client ?? throw new System.ArgumentNullException(nameof(client));
            this.Path = EPath.ToObject(classId, instanceId);
        }

        public const int DefaultInstanceId = 1;

        protected readonly EEIPClient Client;
        public readonly EPath Path;

        protected IReadOnlyList<byte> GetClassAttributeAll() => Client.GetAttributeAll(Path.WithClassIdOnly());

        protected IReadOnlyList<byte> GetInstanceAttributeAll() => Client.GetAttributeAll(Path);
        protected IReadOnlyList<byte> GetInstanceAttributeSingle(ushort id, ushort? instanceId = null) => Client.GetAttributeSingle(GetAttributePath(id, instanceId));
        protected void SetInstanceAttributeSingle(ushort id, byte[] value, ushort? instanceId = null) => this.Client.SetAttributeSingle(GetAttributePath(id, instanceId), value);

        protected EPath GetInstancePath(ushort? instanceId = null) => instanceId.HasValue ?
            Path.WithInstanceId(instanceId.Value) :
            Path;

        protected EPath GetAttributePath(ushort id, ushort? instanceId = null) => GetInstancePath(instanceId).WithAttributeId(id);
    }
}
