using System;
using Sres.Net.EEIP.Data;

namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    /// <summary>
    /// Message router request. CIP specification 2-4.1.
    /// </summary>
    public record MessageRouterRequest :
        Byteable
    {
        public MessageRouterRequest(byte service, EPath path, IByteable data = null)
        {
            this.Service = service;
            this.Path = path ?? throw new ArgumentNullException(nameof(path));
            this.Data = data ?? Bytes.Empty;
        }

        /// <summary>
        /// Service code
        /// </summary>
        public byte Service { get; init; }
        /// <summary>
        /// Request path
        /// </summary>
        public EPath Path { get; }
        /// <summary>
        /// Request data
        /// </summary>
        public IByteable Data { get; }

        public override ushort ByteCount => (ushort)(1 + Path.ByteCount + Data.ByteCount);

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            bytes[index++] = Service;
            Path.ToBytes(bytes, ref index);
            Data.ToBytes(bytes, ref index);
        }
    }

}
