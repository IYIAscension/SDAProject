using System;
using System.IO;
using System.Text;

namespace DataSplitter
{
    /// <summary>
    /// Represents a collection of FileStreams. This is used to represent
    /// the set of data file streams for a specific country's covid-19 data.
    /// </summary>
    public sealed class StreamCollection : IDisposable
    {
        /// <summary>
        /// The array of streams, one <see cref="FileStream"/> per data file
        /// and one <see cref="StreamCollection"/> per country.
        /// </summary>
        private readonly FileStream[] streams;

        /// <summary>
        /// The encoding to use for text data.
        /// </summary>
        private static readonly Encoding encoding;

        /*  Set the encoding to UTF8. We don't need the characters offered by
         *  Unicode, so this will halve the file size. */
        static StreamCollection()
            => encoding = Encoding.UTF8;

        /// <summary>
        /// Creates a collection of FileStreams, one for each string in the
        /// given array of file paths.
        /// </summary>
        /// <param name="paths">The array of paths to create streams for.
        /// </param>
        public StreamCollection(string[] paths)
        {
            int length = paths.Length;
            streams = new FileStream[length];
            for (int i = 0; i < length; i++)
            {
                string path = paths[i];
                // Create or clear the output file.
                File.WriteAllText(path, string.Empty);
                // Create a stream for the file.
                streams[i] = File.OpenWrite(path);
            }    
        }

        /// <summary>
        /// A static buffer of bytes shared by each write operation to
        /// greatly reduce allocations.
        /// </summary>
        private static byte[] bytes = new byte[9];

        /// <summary>
        /// Appends a row of values to the data streams of this country's
        /// data set.
        /// </summary>
        /// <param name="row">The row of data to write.</param>
        public void Append(object[] row)
        {
            // First element is a DateTime object.
            WriteDateTime(streams[0], (DateTime?)row[3]);

            // Next few elements are numerical data.
            for (int i = 4; i < 33; i++)
            {
                WriteDouble(streams[i-3], (double?)row[i]);
            }

            // Then a text column.
            WriteString(streams[30], (string)row[33]);

            // And finally a few more numerical columns.
            for (int i = 34; i < row.Length; i++)
                WriteDouble(streams[i - 3], (double?)row[i]);
        }

        /// <summary>
        /// Writes a given <see cref="DateTime"/> object to the given
        /// <see cref="FileStream"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="dt">The value to write.</param>
        private static void WriteDateTime(FileStream stream, DateTime? dt)
        {
            // Unpack the datetime value into stack-allocated objects.
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

            unsafe
            {
                // Copy the stack-allocated objects to the buffer.
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
            // Write the buffer to the stream.
            stream.Write(bytes, 0, 5);
        }

        /// <summary>
        /// Writes a given <see cref="double"/> object to the given
        /// <see cref="FileStream"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The value to write.</param>
        private static void WriteDouble(FileStream stream, double? value)
        {
            // Unpack the Nullable.
            bool hasValue = value.HasValue;
            double val = hasValue ? value.Value : 0.0;
            unsafe
            {
                // Write the Nullable's unpacked data to the buffer.
                fixed (byte* ptr = bytes)
                {
                    *ptr = *(byte*)&hasValue;
                    double* valptr = (double*)(ptr + 1);
                    *valptr = val;
                }
            }
            // Write the buffer to the file.
            stream.Write(bytes, 0, 9);
        }

        /// <summary>
        /// Writes a given <see cref="string"/> object to the given
        /// <see cref="FileStream"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The value to write.</param>
        private static void WriteString(FileStream stream, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = string.Empty;

            // This is a text column, ensure the newline separator is there.
            if (!value.EndsWith('\n'))
                value += "\n";

            // Convert the string to bytes and write to the stream.
            var bytes = encoding.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);

            /* Note: this can be optimized with Spans: a Span for the string,
             * and a Span on top of a stackalloc byte[] for the bytes.
             * This completely removes the heap allocation created by
             * encoding.GetBytes(). */
        }

        /// <summary>
        /// Closes all of the streams held on to by this
        /// <see cref="StreamCollection"/>.
        /// </summary>
        public void Dispose()
        {
            foreach(var stream in streams)
                stream.Dispose();
        }
    }
}
