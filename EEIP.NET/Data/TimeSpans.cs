namespace Sres.Net.EEIP.Data
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// <see cref="TimeSpan"/> helper
    /// </summary>
    public static class TimeSpans
    {
        /// <summary>
        /// Number of μs in 1 ms
        /// </summary>
        public const int MicrosecondsInMillisecond = 1000;

        /// <summary>
        /// Constructs <see cref="TimeSpan"/> from μs
        /// </summary>
        /// <remarks>.NET 7 already has this built in</remarks>
        /// <param name="value">Value in μs</param>
        public static TimeSpan FromMicroseconds(double value) => TimeSpan.FromMilliseconds(value / MicrosecondsInMillisecond);

        /// <summary>
        /// <paramref name="value"/> in μs
        /// </summary>
        /// <remarks>.NET 7 already has this built in</remarks>
        /// <param name="value">Value</param>
        /// <returns>Total μs</returns>
        public static double TotalMicroseconds(this TimeSpan value) => value.TotalMilliseconds * MicrosecondsInMillisecond;

        /// <summary>
        /// <paramref name="value"/> in <c>uint</c> μs
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Total μs</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is &lt; <see cref="TimeSpan.Zero"/></exception>
        /// <exception cref="OverflowException"><paramref name="value"/> is &gt; <see cref="uint.MaxValue"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/>.<see cref="TotalMicroseconds"/> is &gt; <see cref="uint.MaxValue"/></exception>
        public static uint TotalMicrosecondsUint(this TimeSpan value)
        {
            value.ValidatePositiveOrZeroUint(out var totalMicroseconds, nameof(value));
            return totalMicroseconds;
        }

        /// <summary>
        /// Validates <paramref name="value"/> is &gt;= <see cref="TimeSpan.Zero"/>
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="name"><paramref name="value"/> name</param>
        /// <returns><paramref name="value"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is &lt; <see cref="TimeSpan.Zero"/></exception>
        public static TimeSpan ValidatePositiveOrZero(this TimeSpan value, [CallerMemberName] string name = null)
        {
            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(name, value, "Value must be >= 0");
            return value;
        }

        /// <summary>
        /// Validates <paramref name="value"/> is &gt;= <see cref="TimeSpan.Zero"/> and <see cref="TotalMicroseconds"/> &lt;= <see cref="uint.MaxValue"/>
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="name"><paramref name="value"/> name</param>
        /// <returns><paramref name="value"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is &lt; <see cref="TimeSpan.Zero"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/>.<see cref="TotalMicroseconds"/> is &gt; <see cref="uint.MaxValue"/></exception>
        public static TimeSpan ValidatePositiveOrZeroUint(this TimeSpan value, out uint totalMicroseconds, [CallerMemberName] string name = null)
        {
            value.ValidatePositiveOrZero(name);
            var microseconds = value.TotalMicroseconds();
            if (microseconds > uint.MaxValue)
                throw new ArgumentOutOfRangeException(name, value, nameof(totalMicroseconds)+ " must be <= " + uint.MaxValue);
            totalMicroseconds = (uint)microseconds;
            return value;
        }
    }
}
