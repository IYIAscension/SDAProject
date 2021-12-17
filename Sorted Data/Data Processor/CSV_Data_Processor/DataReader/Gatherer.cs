using System;
using System.IO;
using System.Collections.Generic;

namespace DataReader
{
    public static class Gatherer
    {
        private static readonly string parentDir = "..";
        private static readonly string expectedDir = Path.Combine(
            parentDir, parentDir, parentDir,
            parentDir, parentDir, parentDir
        );

        public static readonly HashSet<string> countryNames;

        static Gatherer()
        {
            countryNames = new HashSet<string>(
                File.ReadAllLines(
                    Path.Combine(expectedDir, "countries.txt")
                )
            );
        }

        public static DateTime[] GetDates(string countryName)
        {
            string path = Path.Combine(expectedDir, countryName, "date.data");
            
            unsafe
            {
                // Allocate 5 bytes on the stack.
                byte* block = stackalloc byte[5];
                // Wrap the block in a span.
                Span<byte> api = new Span<byte>(block, 5);
                using (var stream = File.OpenRead(path))
                {
                    int count = (int)(stream.Length / 5);
                    DateTime[] buf = new DateTime[count];

                    for (int i = 0; i < count; i++)
                    {
                        int read = stream.Read(api);
                        if (read == 0)
                            throw new EndOfStreamException();

                        // Unused byte; this data is never null.
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

                    return last_val;
                }
            }
        }
    }
}
