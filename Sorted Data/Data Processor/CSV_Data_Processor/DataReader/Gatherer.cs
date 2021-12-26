using System;
using System.IO;
using System.Collections.Generic;

namespace DataReader
{
    /// <summary>
    /// Helper class for importing from covid .data files.
    /// </summary>
    public static class Gatherer
    {
        private static readonly string parentDir = "..";
        private static readonly string expectedDir = Path.Combine(
            parentDir, parentDir, parentDir,
            parentDir, parentDir, parentDir
        );

        /// <summary>
        /// The set of known names. If a string is in this set, then it is
        /// the name of a country for which covid-19 data is available.
        /// </summary>
        public static readonly HashSet<string> countryNames;

        // Pre-init countryNames with the list of names as figured out by Python.
        static Gatherer()
        {
            countryNames = new HashSet<string>(
                File.ReadAllLines(
                    Path.Combine(expectedDir, "countries.txt")
                )
            );
        }

        /// <summary>
        /// Imports the <see cref="DateTime"/> data for the country with
        /// the given name.
        /// </summary>
        /// <param name="countryName">The name of the country to import the
        /// data point date values for.</param>
        public static DateTime[] GetDates(string countryName)
        {
            // Create the path to the date file.
            string path = Path.Combine(expectedDir, countryName, "date.data");
            
            unsafe
            {
                // Allocate 5 bytes on the stack.
                byte* block = stackalloc byte[5];
                // Wrap the block in a span.
                Span<byte> api = new Span<byte>(block, 5);

                // Open the file and start reading.
                using (var stream = File.OpenRead(path))
                {
                    /*  How many DateTime objects are there in this file,
                     *  given that each instance is 5 bytes? */
                    int count = (int)(stream.Length / 5);

                    // Create the output buffer, since we know the count.
                    DateTime[] buf = new DateTime[count];

                    /*  Start reading from the file. Although there's a check
                     *  for EOF, it should never happen. */
                    for (int i = 0; i < count; i++)
                    {
                        int read = stream.Read(api);
                        if (read == 0)
                            throw new EndOfStreamException();

                        // Unused byte; this data is never null here.
                        bool has_value = *(bool*)block;
                        ushort year = *(ushort*)(block + 1);
                        byte month = block[3];
                        byte day = block[4];

                        buf[i] = new DateTime(year, month, day);
                    }

                    return buf;
                }
            }
        }

        /// <summary>
        /// Imports the numerical data contained in the dataset of the given
        /// name.
        /// </summary>
        /// <param name="countryName">The name of the country to import the
        /// data of.</param>
        /// <param name="file">The name of the file to import the numerical
        /// data from.</param>
        public static double?[] GetNumerics(string countryName, string file)
        {
            string path = Path.Combine(expectedDir, countryName, file);

            unsafe
            {
                // Allocate 5 bytes on the stack.
                byte* block = stackalloc byte[9];
                // Wrap the block in a span.
                Span<byte> api = new Span<byte>(block, 9);
                using (var stream = File.OpenRead(path))
                {
                    int count = (int)(stream.Length / 9);
                    double?[] buf = new double?[count];

                    for (int i = 0; i < count; i++)
                    {
                        int read = stream.Read(api);
                        if (read == 0)
                            throw new EndOfStreamException();

                        bool has_value = *(bool*)block;
                        if (has_value)
                        {
                            buf[i] = *(double*)(block + 1);
                        }
                        else
                        {
                            buf[i] = null;
                        }
                    }

                    return buf;
                }
            }
        }

        /// <summary>
        /// Imports the numerical data contained in the dataset of the given
        /// name, as a time series. That is to say, null-values are considered
        /// an extension of the previously recorded value.
        /// </summary>
        /// <param name="countryName">The name of the country to import the
        /// data of.</param>
        /// <param name="file">The name of the file to import the numerical
        /// data from.</param>
        public static double[] GetTimeSeries(string countryName, string file)
        {
            string path = Path.Combine(expectedDir, countryName, file);

            unsafe
            {
                // Allocate 5 bytes on the stack.
                byte* block = stackalloc byte[9];
                // Wrap the block in a span.
                Span<byte> api = new Span<byte>(block, 9);
                using (var stream = File.OpenRead(path))
                {
                    int count = (int)(stream.Length / 9);
                    double[] buf = new double[count];
                    double last_val = 0.0;

                    for (int i = 0; i < count; i++)
                    {
                        int read = stream.Read(api);
                        if (read == 0)
                            throw new EndOfStreamException();

                        bool has_value = *(bool*)block;

                        if (has_value)
                        {
                            last_val = *(double*)(block + 1);
                        }

                        buf[i] = last_val;
                    }

                    return buf;
                }
            }
        }

        /// <summary>
        /// Imports the final value of numerical data contained in the dataset
        /// of the given name, as a time series. That is to say, null-values
        /// are considered an extension of the previously recorded value.
        /// </summary>
        /// <param name="countryName">The name of the country to import the
        /// data of.</param>
        /// <param name="file">The name of the file to import the numerical
        /// data from.</param>
        public static double? GetFinalTimeSeries(string countryName, string file)
        {
            string path = Path.Combine(expectedDir, countryName, file);

            unsafe
            {
                // Allocate 5 bytes on the stack.
                byte* block = stackalloc byte[9];
                // Wrap the block in a span.
                Span<byte> api = new Span<byte>(block, 9);
                using (var stream = File.OpenRead(path))
                {
                    int count = (int)(stream.Length / 9);
                    double? last_val = null;

                    for (int i = 0; i < count; i++)
                    {
                        int read = stream.Read(api);
                        if (read == 0)
                            throw new EndOfStreamException();

                        bool has_value = *(bool*)block;

                        if (has_value)
                        {
                            last_val = *(double*)(block + 1);
                        }
                    }

                    // Return the last known value instead of using buffers.
                    return last_val;
                }
            }
        }
    }
}
