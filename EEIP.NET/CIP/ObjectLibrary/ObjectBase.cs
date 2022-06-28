namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    using System.Collections.Generic;
    using Sres.Net.EEIP.CIP.Path;

    /// <summary>
    /// Object library object base
    /// </summary>
    public abstract class ObjectBase
    {
        protected ObjectBase(EIPClient client, uint classId, uint instanceId = DefaultInstanceId)
        {
            this.Client = client ?? throw new System.ArgumentNullException(nameof(client));
            this.Path = EPath.ToObject(classId, instanceId);
        }

        protected readonly EIPClient Client;

        /// <summary>
        /// Object path
        /// </summary>
        public readonly EPath Path;

        public const uint DefaultInstanceId = 1;

        /// <summary>
        /// Object class
        /// </summary>
        public ObjectClass Class
        {
            get
            {
                var bytes = GetClassAttributeAll();
                int index = 0;
                return new(bytes, ref index);
            }
        }

        protected IReadOnlyList<byte> GetClassAttributeAll() => Client.GetAttributeAll(Path.WithClassIdOnly());

        protected IReadOnlyList<byte> GetInstanceAttributeAll() => Client.GetAttributeAll(Path);
        protected IReadOnlyList<byte> GetInstanceAttributeSingle(uint id, uint? instanceId = null) => Client.GetAttributeSingle(GetAttributePath(id, instanceId));
        protected void SetInstanceAttributeSingle(uint id, byte[] value, uint? instanceId = null) => this.Client.SetAttributeSingle(GetAttributePath(id, instanceId), value);

        protected EPath GetInstancePath(uint? instanceId = null) => instanceId.HasValue ?
            Path.WithInstanceId(instanceId.Value) :
            Path;

        protected EPath GetAttributePath(uint id, uint? instanceId = null) => GetInstancePath(instanceId).WithAttributeId(id);
    }
}
