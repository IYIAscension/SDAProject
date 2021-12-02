using System;
using System.IO;
using System.Text;

namespace DataSplitter
{
    public sealed class StreamCollection : IDisposable
    {
        private FileStream[] streams;
        private static Encoding encoding;

        static StreamCollection()
            => encoding = Encoding.UTF8;

        public StreamCollection(string[] paths)
        {
            int length = paths.Length;
            streams = new FileStream[length];
            for (int i = 0; i < length; i++)
            {
                string path = paths[i];
                File.WriteAllText(path, string.Empty);
                streams[i] = File.OpenWrite(path);
            }    
        }

        private static byte[] bytes = new byte[9];

        public void Append(object[] row)
        {
            WriteDateTime(streams[0], (DateTime?)row[3]);

            for (int i = 4; i < 33; i++)
            {
                WriteDouble(streams[i-3], (double?)row[i]);
            }

            WriteString(streams[30], (string)row[33]);

            for (int i = 34; i < row.Length; i++)
                WriteDouble(streams[i - 3], (double?)row[i]);
        }

        private static void WriteDateTime(FileStream stream, DateTime? dt)
        {
            bool hasValue = dt.HasValue;
            ushort year = 0;
            byte month = 0;
            byte day = 0;
            if (hasValue)
            {
                var dateval = dt.Value;
                year = (ushort)dateval.Year;
                month = (byte)dateval.Month;
                day = (byte)dateval.Day;
            }

            long val = hasValue ? dt.Value.ToBinary() : 0L;
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    *ptr = *(byte*)&hasValue;
                    ushort* yearptr = (ushort*)(ptr + 1);
                    *yearptr = year;
                    // Write month and year.
                    // Idx 0: yes/no value
                    // Idx 1, 2: year
                    // Idx 3: month
                    // Idx 4: day
                    ptr[3] = month;
                    ptr[4] = day;
                }
            }
            stream.Write(bytes, 0, 5);
        }

        private static void WriteDouble(FileStream stream, double? value)
        {
            bool hasValue = value.HasValue;
            double val = hasValue ? value.Value : 0.0;
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    *ptr = *(byte*)&hasValue;
                    double* valptr = (double*)(ptr + 1);
                    *valptr = val;
                }
            }
            stream.Write(bytes, 0, 9);
        }

        private static void WriteString(FileStream stream, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = string.Empty;

            if (!value.EndsWith('\n'))
                value += "\n";

            var bytes = encoding.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            foreach(var stream in streams)
                stream.Dispose();
        }
    }
}
