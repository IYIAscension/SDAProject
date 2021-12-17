using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace CSV_Processor
{
    public sealed class CSVFile : IEnumerable<object[]>
    {
        private readonly string filepath;

        private readonly List<Column> columns = new List<Column>();
        private int rows = 0;

        public CSVFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException();
            }

            this.filepath = filepath;
        }

        public CSVFile(params Column[] columns)
        {
            int length = columns.Length;
            if (length > 1)
            {
                rows = columns[0].Length;

                for (int i = 1; i < length; i++)
                {
                    if (columns[i].Length != rows)
                        throw new ArgumentOutOfRangeException(
                            $"The provided columns must be of equal length. Column {i} was found to differ from the length as set by the first provided column."
                        );
                }

                this.columns = new List<Column>(columns);
            }
            else
            {
                this.columns = new List<Column>();
            }
        }

        public string[] GetColumnNames()
        {
            int columnCount = columns.Count;
            string[] names = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
                names[i] = columns[i].name;

            return names;
        }

        public object[] GetRow(int row)
        {
            int columnCount = columns.Count;
            object[] output = new object[columnCount];
            for (int i = 0; i < columnCount; i++)
                output[i] = columns[i].GetValue(row);

            return output;
        }

        public string[] ToString(int row)
        {
            int columnCount = columns.Count;
            string[] output = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
                output[i] = columns[i].ToString(row);

            return output;
        }

        public struct Sampler
        {
            private readonly List<int> indices;
            private CSVFile progenitor;

            internal Sampler(CSVFile source, List<int> indices)
            {
                progenitor = source;
                this.indices = indices;
            }

            public object[] GetRow(int row)
            {
                int indexCount = indices.Count;
                object[] output = new object[indexCount];
                for (int i = 0; i < indexCount; i++)
                {
                    output[i] = progenitor.columns[indices[i]].GetValue(row);
                }
                return output;
            }
        }

        private int GetIndexOfColumn(string name)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public Sampler GetSampler(int columnMin, int columnMax)
        {
            if (columnMin < 0)
                columnMin = 0;
            if (columnMax < 0)
                columnMax = columns.Count - 1;

            List<int> indices = new List<int>();
            for (int i = columnMin; i <= columnMax; i++)
            {
                indices.Add(i);
            }
            return new Sampler(this, indices);
        }

        public Sampler GetSampler(params string[] columns)
        {
            List<int> indices = new List<int>();
            foreach(string columnName in columns)
            {
                int index = GetIndexOfColumn(columnName);
                if (index != -1)
                    indices.Add(index);
            }

            return new Sampler(this, indices);
        }

        public void BeginRead(int linesToRead = -1)
        {
            Console.Clear();
            // Create a stream of file data. This prevents memory from being
            // detonated.
            using (FileStream stream = File.OpenRead(filepath))
            {
                /* The stream reads binary data - which is neat, but we need 
                 * a textual data processor. */
                using (StreamReader sr = new StreamReader(stream))
                {
                    /*  Get the first line of the CSV file, which
                     *  describes the columns. */
                    string columnData = sr.ReadLine();
                    string[] parts = columnData.Split(',');
                    // Ensure all columns are present.
                    // If columns are missing, add text columns for them.
                    for(int i = 0; i < parts.Length; i++)
                    {
                        string columnName = parts[i].Trim();
                        try
                        {
                            if (columns[i].name != columnName)
                            {
                                columns.Insert(i, new TextColumn(columnName));
                                Console.WriteLine($"CSV Reader: auto-inserted missing column '{columnName}' at index {i}.");
                            }
                        }
                        catch(IndexOutOfRangeException)
                        {
                            // Missing column at index. Insert a new column.
                            BindTextColumn(columnName);
                            Console.WriteLine($"CSV Reader: auto-inserted missing column '{columnName}' at index {i}.");
                        }
                    }

                    // Now read the remainder of the file.
                    int columnCount = columns.Count;
                    bool cont = true;
                    double scalar = 100.0 / stream.Length;
                    while (!sr.EndOfStream && cont)
                    {
                        // Process line and append to columns.
                        parts = sr.ReadLine().Split(',');
                        for (int i = 0; i < columnCount; i++)
                        {
                            try
                            {
                                columns[i].Append(parts[i]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(
                                    $"CSV Reader: exception caught processing line {rows}.\nAttempted to bind value [{parts[i]}] to column [{columns[i].name}]\nLine content: [{string.Join(", ", parts)}]\nException: {e}"
                                );
                                columns[i].Append(null);
                            }
                        }
                        rows++;
                        cont = linesToRead < 0 || rows < linesToRead;
                        double progress = stream.Position * scalar;
                        Console.SetCursorPosition(0, 0);
                        Console.Write(
                            $"CSV Reader: {progress:###.0000}% of file read."
                        );
                    }

                    Console.Clear();
                }
            }
        }

        public void BindTextColumn(string columnName)
            => columns.Add(new TextColumn(columnName));

        public void BindValueColumn<T>(string columnName, Func<string, T?> converter) where T : struct
            => columns.Add(new ValueColumn<T>(columnName, converter));

        public void BindTextColumns(params string[] columnNames)
        {
            foreach (string name in columnNames)
                columns.Add(new TextColumn(name));
        }

        public void BindValueColumns<T>(Func<string, T?> converter, params string[] columnNames) where T : struct
        {
            foreach (string columnName in columnNames)
                columns.Add(new ValueColumn<T>(columnName, converter));
        }

        private struct RowEnumerator : IEnumerator<object[]>
        {
            private int rowIndex;
            private object[] array;
            private readonly CSVFile progenitor;

            public RowEnumerator(CSVFile progenitor)
            {
                this.progenitor = progenitor;
                rowIndex = 0;
                array = progenitor.GetRow(rowIndex);
            }

            public object[] Current => array;

            object IEnumerator.Current => array;

            public void Dispose() { }

            public bool MoveNext()
            {
                rowIndex += 1;
                if (rowIndex < progenitor.rows)
                {
                    array = progenitor.GetRow(rowIndex);
                    return true;
                }

                array = null;
                return false;
            }

            public void Reset()
            {
                rowIndex = 0;
                array = progenitor.GetRow(rowIndex);
            }
        }

        public IEnumerator<object[]> GetEnumerator()
            => new RowEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => new RowEnumerator(this);

        public Column this[int index]
            => columns[index];

        public Column this[string name]
            => columns[GetIndexOfColumn(name)];

        public T GetColumn<T>(int index) where T : Column
            => (T)columns[index];

        public T GetColumn<T>(string name) where T : Column
            => (T)columns[GetIndexOfColumn(name)];

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            // Compile headers.
            string separator = ", ";
            sb.AppendJoin(separator, columns.Select(x => x.name));
            sb.AppendLine();

            int columnCount = columns.Count;
            for (int row = 0; row < rows; row++)
            {
                // Compile this row data efficiently.
                sb.AppendJoin(separator, columns.Select(x => x.GetValue(row)));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ToStringAligned()
        {
            // Apologies to RAM memory, this may sting. We need string data.
            string[,] valueMatrix = new string[rows+1, columns.Count];
            int columnCount = columns.Count;
            int[] columnWidths = new int[columnCount];
            string separator = ", ";

            // Fill the matrix column-wise.
            for (int col = 0; col < columnCount; col++)
            {
                Column column = columns[col];
                string name = column.name;
                bool addsep = col != columnCount - 1;
                if (addsep)
                    name += separator;

                valueMatrix[0, col] = name;
                columnWidths[col] = name.Length;
                for (int row = 0; row < rows; row++)
                {
                    object val = column.GetValue(row);
                    string value;
                    if (val is string str)
                        value = str;
                    else if (val is double num)
                        value = num.ToString(BuiltinParsers.culture);
                    else
                        value = val.ToString();

                    // Add the trailing comma if needed.
                    if (addsep)
                        value += separator;

                    columnWidths[col] = Math.Max(
                        columnWidths[col], value.Length
                    );
                    valueMatrix[row+1, col] = value;
                }
            }

            // That created a lot of garbage. Sorry, C#.
            // Now we have the CSV as a string matrix, including headers.
            // Start writing the data in a padded format.
            var sb = new System.Text.StringBuilder();
            int padded = rows + 1;
            for (int row = 0; row < padded; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    sb.Append(
                        valueMatrix[row, col].PadLeft(columnWidths[col])
                    );
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
