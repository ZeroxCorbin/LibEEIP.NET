namespace Sres.Net.EEIP.CIP.Path
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

        public LogicalSegment GetSegment(LogicalType type, bool optional)
        {
            var segment = Segments.
                OfType<LogicalSegment>().
                FirstOrDefault(i => i.LogicalType == type);
            if (!optional && segment is null)
                throw new ArgumentOutOfRangeException(nameof(type), type, type + " segment not found");
            return segment;
        }

        public uint? GetSegmentValue(LogicalType type) => GetSegment(type, true)?.Value;

        public EPath WithSegmentValue(LogicalType type, uint value)
        {
            var oldSegment = GetSegment(type, false);
            var newSegment = oldSegment with { Value = value };
            return Replace(oldSegment, newSegment);
        }

        public EPath Add(Segment segment, bool appendOtherwisePrepend = true)
        {
            if (segment is null)
                throw new ArgumentNullException(nameof(segment));
            var segments = new[] { segment };
            return this with
            {
                Segments = (appendOtherwisePrepend ?
                    Segments.Concat(segments) :
                    segments.Concat(Segments)).
                    ToArray()
            };
        }

        public EPath Replace(Segment oldSegment, Segment newSegment)
        {
            if (oldSegment is null)
                throw new ArgumentNullException(nameof(oldSegment));
            if (newSegment is null)
                throw new ArgumentNullException(nameof(newSegment));
            return this with
            {
                Segments = Segments.
                    Select(i => ReferenceEquals(i, oldSegment) ?
                        newSegment :
                        i).
                    ToArray()
            };
        }

        public EPath AddOrReplace<T>(
            Func<T> create,
            Func<T, bool> isSame = null,
            Func<T, bool> find = null,
            bool appendOtherwisePrepend = true)
            where T : Segment
        {
            if (create is null)
                throw new ArgumentNullException(nameof(create));
            var oldSegment = GetSegment(find);
            if (oldSegment != null &&
                (isSame?.Invoke(oldSegment) ?? false))
            {
                return this;
            }
            var newSegment = create();
            return oldSegment is null ?
                Add(newSegment, appendOtherwisePrepend) :
                Replace(oldSegment, newSegment);
        }

        public T GetSegment<T>(Func<T, bool> find = null)
            where T : Segment
            => GetSegments(find).FirstOrDefault();

        public IEnumerable<T> GetSegments<T>(Func<T, bool> find = null)
            where T : Segment
        {
            var segments = Segments.OfType<T>();
            if (find != null)
                segments.Where(find);
            return segments;
        }

        public bool HasSegment<T>(Func<T, bool> find = null)
            where T : Segment
            => GetSegment(find) != null;

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
            new LogicalSegment(classId, false, LogicalType.ClassId),
            new LogicalSegment(connectionPoint, false, LogicalType.ConnectionPoint),
            new LogicalSegment(memberId, true, LogicalType.MemberId));

        public static EPath ToConnectionPoint(ushort classId, ushort instanceId, ushort connectionPoint, ushort memberId = 0) => new(
            new LogicalSegment(classId, false, LogicalType.ClassId),
            new LogicalSegment(instanceId, false, LogicalType.InstanceId),
            new LogicalSegment(connectionPoint, false, LogicalType.ConnectionPoint),
            new LogicalSegment(memberId, true, LogicalType.MemberId));

        #endregion

        #region Object

        /// <summary>
        /// Path to object
        /// </summary>
        /// <param name="classId"><see cref="ClassId"/></param>
        /// <param name="instanceId"><see cref="InstanceId"/></param>
        /// <param name="attributeId"><see cref="AttributeId"/></param>
        /// <param name="memberId"><see cref="MemberId"/></param>
        public static EPath ToObject(uint classId, uint instanceId = 0, uint attributeId = 0, uint memberId = 0) => new(
            new LogicalSegment(classId, false, LogicalType.ClassId),
            new LogicalSegment(instanceId, true, LogicalType.InstanceId),
            new LogicalSegment(attributeId, true, LogicalType.AttributeId),
            new LogicalSegment(memberId, true, LogicalType.MemberId));

        /// <summary>
        /// Class identifier
        /// </summary>
        public uint? ClassId => (ushort?)GetSegmentValue(LogicalType.ClassId);
        /// <summary>
        /// Instance identifier. 0 means class itself without reference to any instance.
        /// </summary>
        public uint? InstanceId => (ushort?)GetSegmentValue(LogicalType.InstanceId);
        /// <summary>
        /// Class/Instance identifier. 0 means class/instance itself without reference to any attribute.
        /// </summary>
        public uint? AttributeId => (ushort?)GetSegmentValue(LogicalType.AttributeId);
        /// <summary>
        /// Class/Instance attribute member identifier. 0 means class/instance attribute itself without reference to any member.
        /// </summary>
        public uint? MemberId => (ushort?)GetSegmentValue(LogicalType.MemberId);

        public EPath WithClassIdOnly() => EPath.ToObject(ClassId.Value);
        public EPath WithInstanceId(uint id) => WithSegmentValue(LogicalType.InstanceId, id);
        public EPath WithAttributeId(uint id) => WithSegmentValue(LogicalType.AttributeId, id);
        public EPath WithMemberId(uint id) => WithSegmentValue(LogicalType.MemberId, id);

        #endregion

        #region Data

        /// <summary>
        /// Whether this path has <see cref="SimpleDataSegment"/>
        /// </summary>
        public bool HasData => HasSegment<SimpleDataSegment>();

        /// <summary>
        /// Adds <see cref="SimpleDataSegment"/> to <see cref="Segments"/>
        /// </summary>
        /// <param name="data">Data to add</param>
        public EPath AddData(params byte[] data)
        {
            var segment = new SimpleDataSegment(data);
            return Add(segment);
        }

        #endregion

        /// <summary>
        /// Adds <see cref="ProductionInhibitTimeSegment"/> as first or replaces existing one
        /// </summary>
        /// <param name="value"><see cref="ProductionInhibitTimeSegment.Value"/></param>
        public EPath WithProductionInhibitTime(byte value) => AddOrReplace<ProductionInhibitTimeSegment>(
            () => new(value),
            i => i.Value == value,
            appendOtherwisePrepend: false);

        private readonly IReadOnlyList<Segment> segments;
    }
}
