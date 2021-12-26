using System;
using System.Globalization;

namespace CSV_Processor
{
    /// <summary>
    /// This static class contains a series of pre-defined parsers for built-in
    /// types.
    /// </summary>
    public static class BuiltinParsers
    {
        /// <summary>
        /// The CultureInfo (en-us) that is typically used in CSV files. Use
        /// this CultureInfo to correctly parse and write strings describing
        /// numeric values for CSV files.
        /// </summary>
        public static readonly CultureInfo culture = CultureInfo.GetCultureInfo("en-us");

        /// <summary>
        /// Attempts to parse a <see cref="DateTime"/> object. Returns null if
        /// the given string is null or whitespace.
        /// <para>Exceptions:</para>
        /// <para><see cref="FormatException"/> if the string is improperly
        /// formatted.</para>
        /// </summary>
        /// <param name="str">The string to process.</param>
        /// <returns></returns>
        public static DateTime? ParseDateTime(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            // All CSV datetime objects typically follow the format YYYY-MM-DD
            if (str.Length != 10)
                return DateTime.Parse(str);

            // A'ight, let's parse this quickly.
            DateTime dt = new DateTime(
                int.Parse(str.Substring(0, 4)),
                int.Parse(str.Substring(5, 2)),
                int.Parse(str.Substring(8, 2))
            );

            /*  Note: the above parsing operation is inefficient. Better would
             *  be to use a ReadOnlySpan<char> converter from string, which
             *  performs a slicing operation instead of creating a separate
             *  string object. Thus, it prevents three heap allocations
             *  by instead reading from the one string allocation that is
             *  provided. */
            return dt;
        }

        /// <summary>
        /// Attempts to parse a <see cref="double"/> object. Returns null if
        /// the given string is null or whitespace.
        /// <para><see cref="FormatException"/> if the string is improperly
        /// formatted.</para>
        /// <para><see cref="OverflowException"/> if the value encoded in the
        /// string is too large to fit in a 64-bit floating point number.
        /// </para>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double? ParseDouble(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            return double.Parse(str, culture);
        }
    }
}
