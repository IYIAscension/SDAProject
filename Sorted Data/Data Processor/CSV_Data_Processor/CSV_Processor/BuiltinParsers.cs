using System;

namespace CSV_Processor
{
    public static class BuiltinParsers
    {
        public static readonly System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.GetCultureInfo("en-us");

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
            return dt;
        }

        public static double? ParseDouble(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            return double.Parse(str, culture);
        }
    }
}
