using System;
using System.IO;
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
                        string columnName = parts[i];
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
    }
}
