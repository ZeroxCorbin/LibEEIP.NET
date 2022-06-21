namespace Sres.Net.EEIP.CIP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sres.Net.EEIP.Data;

    /// <summary>
    /// Padded encrypted request logical path (EPATH)
    /// </summary>
    /// <remarks>CIP specification: C-1.4.2</remarks>
    public partial record EPath :
        Byteable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="segments"><see cref="Segments"/></param>
        public EPath(params Segment[] segments)
            => Segments = segments;

        public static EPath Concat(params EPath[] paths)
        {
            if (paths is null)
                throw new ArgumentNullException(nameof(paths));
            var segments = paths.
                SelectMany(i => i.Segments).
                ToArray();
            return new(segments);
        }

        /// <summary>
        /// Number of 16bit words of this path
        /// </summary>
        public byte Size => (byte)(ByteCount / 2);

        #region Segments

        /// <summary>
        /// Path segments
        /// </summary>
        public IReadOnlyList<Segment> Segments
        {
            get => segments;
            init => segments = value ?? throw new ArgumentNullException(nameof(Segments));
        }

        public Segment GetSegment(Segment.LogicalType type, bool optional)
        {
            var segment = Segments.FirstOrDefault(i => i.Type == type);
            if (!optional && segment is null)
                throw new ArgumentOutOfRangeException(nameof(type), type, "Segment not found");
            return segment;
        }

        public uint? GetSegmentValue(Segment.LogicalType type) => GetSegment(type, true)?.Value;

        public EPath WithSegmentValue(Segment.LogicalType type, uint value)
        {
            var oldSegment = GetSegment(type, false);
            var newSegment = oldSegment with { Value = value };
            return this with
            {
                Segments = Segments.
                    Select(i => ReferenceEquals(i, oldSegment) ?
                        newSegment :
                        i).
                    ToArray()
            };
        }

        #endregion

        #region Byteable

        public override ushort ByteCount => (ushort)(
            1 + // Size
            Segments.ByteCount());

        protected override void DoToBytes(byte[] bytes, ref int index)
        {
            bytes[index++] = Size;
            Segments.ToBytes(bytes, ref index);
        }

        #endregion

        #region ConnectionPoint

        public static EPath ToConnectionPoint(ushort classId, ushort connectionPoint, ushort memberId = 0) => new(
            new Segment(classId, false, Segment.LogicalType.ClassId),
            new Segment(connectionPoint, false, Segment.LogicalType.ConnectionPoint),
            new Segment(memberId, true, Segment.LogicalType.MemberId));

        public static EPath ToConnectionPoint(ushort classId, ushort instanceId, ushort connectionPoint, ushort memberId = 0) => new(
            new Segment(classId, false, Segment.LogicalType.ClassId),
            new Segment(instanceId, false, Segment.LogicalType.InstanceId),
            new Segment(connectionPoint, false, Segment.LogicalType.ConnectionPoint),
            new Segment(memberId, true, Segment.LogicalType.MemberId));

        #endregion

        #region Object

        /// <summary>
        /// Path to object
        /// </summary>
        /// <param name="classId"><see cref="ClassId"/></param>
        /// <param name="instanceId"><see cref="InstanceId"/></param>
        /// <param name="attributeId"><see cref="AttributeId"/></param>
        /// <param name="memberId"><see cref="MemberId"/></param>
        public static EPath ToObject(ushort classId, ushort instanceId = 0, ushort attributeId = 0, ushort memberId = 0) => new(
            new Segment(classId, false, Segment.LogicalType.ClassId),
            new Segment(instanceId, true, Segment.LogicalType.InstanceId),
            new Segment(attributeId, true, Segment.LogicalType.AttributeId),
            new Segment(memberId, true, Segment.LogicalType.MemberId));

        /// <summary>
        /// Class identifier
        /// </summary>
        public ushort? ClassId => (ushort?)GetSegmentValue(Segment.LogicalType.ClassId);
        /// <summary>
        /// Instance identifier. 0 means class itself without reference to any instance.
        /// </summary>
        public ushort? InstanceId => (ushort?)GetSegmentValue(Segment.LogicalType.InstanceId);
        /// <summary>
        /// Class/Instance identifier. 0 means class/instance itself without reference to any attribute.
        /// </summary>
        public ushort? AttributeId => (ushort?)GetSegmentValue(Segment.LogicalType.AttributeId);
        /// <summary>
        /// Class/Instance attribute member identifier. 0 means class/instance attribute itself without reference to any member.
        /// </summary>
        public ushort? MemberId => (ushort?)GetSegmentValue(Segment.LogicalType.MemberId);

        public EPath WithClassIdOnly() => EPath.ToObject(ClassId.Value);
        public EPath WithInstanceId(ushort id) => WithSegmentValue(Segment.LogicalType.InstanceId, id);
        public EPath WithAttributeId(ushort id) => WithSegmentValue(Segment.LogicalType.AttributeId, id);
        public EPath WithMemberId(ushort id) => WithSegmentValue(Segment.LogicalType.MemberId, id);

        #endregion

        private readonly IReadOnlyList<Segment> segments;
    }
}
