using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace CSV_Processor
{
    /// <summary>
    /// Represents a generic CSV File.
    /// </summary>
    public sealed class CSVFile : IEnumerable<object[]>
    {
        /// <summary>
        /// The path to the <see cref="File"/> that this <see cref="CSVFile"/>
        /// represents.
        /// <para>This value is null if this <see cref="CSVFile"/> was
        /// created entirely in memory and doesn't (yet) correspond to any
        /// object on the disk.</para>
        /// </summary>
        private readonly string filepath;

        /// <summary>
        /// The list of <see cref="Column"/> objects that are contained in
        /// this <see cref="CSVFile"/>.
        /// </summary>
        private readonly List<Column> columns = new List<Column>();

        /// <summary>
        /// The number of rows that this <see cref="CSVFile"/> currently
        /// contains.
        /// </summary>
        private int rows = 0;

        /// <summary>
        /// The number of rows presently contained in this
        /// <see cref="CSVFile"/>.
        /// </summary>
        public int Length => rows;

        /// <summary>
        /// Creates a new, empty <see cref="CSVFile"/> bound to the given
        /// file path.
        /// </summary>
        /// <param name="filepath">The path of the file that this
        /// <see cref="CSVFile"/> represents.</param>
        public CSVFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException();
            }

            this.filepath = filepath;
        }

        /// <summary>
        /// Creates a new <see cref="CSVFile"/> represented by the given
        /// set of <see cref="Column"/> objects.
        /// </summary>
        /// <param name="columns">The columns of this <see cref="CSVFile"/>,
        /// in order.</param>
        public CSVFile(params Column[] columns)
        {
            /*  Check if columns were supplied.
             *  If so, they must be of equal length. */
            int length = columns.Length;
            if (length > 1)
            {
                // Store the length of the first column.
                rows = columns[0].Length;

                /*  Loop over the rest of the columns and throw an error
                 *  if they aren't the same length as the first column. */
                for (int i = 1; i < length; i++)
                {
                    if (columns[i].Length != rows)
                        throw new ArgumentOutOfRangeException(
                            $"The provided columns must be of equal length. Column {i} was found to differ from the length as set by the first provided column."
                        );
                }

                /*  Verified. Create a new backing list from
                 *  the supplied column objecs. */
                this.columns = new List<Column>(columns);
            }
            else
            {
                // No columns supplied -> create empty list.
                this.columns = new List<Column>();
            }
        }

        /// <summary>
        /// Returns the array of <see cref="Column"/> names present in this
        /// <see cref="CSVFile"/>, in order.
        /// </summary>
        public string[] GetColumnNames()
        {
            int columnCount = columns.Count;
            string[] names = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
                names[i] = columns[i].name;

            return names;
        }

        /// <summary>
        /// Returns the array of values stored at the given row in this
        /// <see cref="CSVFile"/>.
        /// <para>Throws an <see cref="IndexOutOfRangeException"/> if the
        /// given row index is greater than or equal to <see cref="Length"/>.
        /// </para>
        /// </summary>
        /// <param name="row">The index of the row to look up.</param>
        public object[] GetRow(int row)
        {
            int columnCount = columns.Count;
            object[] output = new object[columnCount];
            for (int i = 0; i < columnCount; i++)
                output[i] = columns[i].GetValue(row);

            return output;
        }

        /// <summary>
        /// Returns the array of textual representations of the values stored
        /// at the given row in this <see cref="CSVFile"/>.
        /// <para>Throws an <see cref="IndexOutOfRangeException"/> if the
        /// given row index is greater than or equal to <see cref="Length"/>.
        /// </para>
        /// </summary>
        /// <param name="row">The index of the row to look up.</param>
        public string[] ToString(int row)
        {
            int columnCount = columns.Count;
            string[] output = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
                output[i] = columns[i].ToString(row);

            return output;
        }

        /// <summary>
        /// A data structure bound to a <see cref="CSVFile"/> that can be
        /// used to exclude certain columns.
        /// </summary>
        public struct Sampler
        {
            /// <summary>
            /// The list of indices of columns to return.
            /// </summary>
            private readonly List<int> indices;

            /// <summary>
            /// The <see cref="CSVFile"/> that this <see cref="Sampler"/>
            /// instance is bound to.
            /// </summary>
            private readonly CSVFile progenitor;

            /// <summary>
            /// Creates a new <see cref="Sampler"/> object from the given
            /// <see cref="CSVFile"/>, with the given list of indices.
            /// </summary>
            /// <param name="source">The <see cref="CSVFile"/> to selectively
            /// sample <see cref="Column"/>s from.</param>
            /// <param name="indices">The list of indices of the
            /// columns that this <see cref="Sampler"/> should target.</param>
            internal Sampler(CSVFile source, List<int> indices)
            {
                progenitor = source;
                this.indices = indices;
            }

            /// <summary>
            /// Returns the array of values stored at the given row in this
            /// <see cref="CSVFile"/>, filtered by this <see cref="Sampler"/>.
            /// <para>Throws an <see cref="IndexOutOfRangeException"/> if the
            /// given row index is greater than or equal to <see cref="Length"/>.
            /// </para>
            /// </summary>
            /// <param name="row">The index of the row to look up.</param>
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

        /// <summary>
        /// Gets the index of the <see cref="Column"/> in this
        /// <see cref="CSVFile"/> that has the given name.
        /// <para>Returns -1 if no <see cref="Column"/> by the given name
        /// exists in this <see cref="CSVFile"/>.</para>
        /// </summary>
        /// <param name="name">The name of the <see cref="Column"/>
        /// to look up.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a <see cref="Sampler"/> object limited to the given range
        /// of <see cref="Column"/> indices.
        /// </summary>
        /// <param name="columnMin">The lowermost index to include.</param>
        /// <param name="columnMax">The uppermost index to include.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a <see cref="Sampler"/> object that samples the
        /// <see cref="Column"/> objects in this <see cref="CSVFile"/> with
        /// the given indices in the given order.
        /// </summary>
        /// <param name="columns">The indices of the columns to look up.
        /// </param>
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

        /// <summary>
        /// Appends all of the data in the backing disk-stored file to the
        /// columns in this <see cref="CSVFile"/>. Missing columns are
        /// auto-inserted.
        /// <para>This method expects this <see cref="CSVFile"/> to have
        /// the same column layout as the file on the disk.</para>
        /// </summary>
        /// <param name="linesToRead">The amount of lines to read.
        /// Negative numbers mean that all lines are read.</param>
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

        /// <summary>
        /// Adds a <see cref="TextColumn"/> to this <see cref="CSVFile"/>
        /// with the given name.
        /// </summary>
        /// <param name="columnName">The name of the column to append.</param>
        public void BindTextColumn(string columnName)
            => columns.Add(new TextColumn(columnName));

        /// <summary>
        /// Adds a <see cref="ValueColumn{T}"/> to this <see cref="CSVFile"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the column.
        /// </typeparam>
        /// <param name="columnName">The name of the column to append.</param>
        /// <param name="converter">The converter method to use
        /// for converting new text values to the backing type of the
        /// column.</param>
        public void BindValueColumn<T>(string columnName, Func<string, T?> converter) where T : struct
            => columns.Add(new ValueColumn<T>(columnName, converter));

        /// <summary>
        /// Adds several <see cref="TextColumn"/> objects to this
        /// <see cref="CSVFile"/> with the given names and in the given order.
        /// </summary>
        /// <param name="columnNames">The names of the columns
        /// to append.</param>
        public void BindTextColumns(params string[] columnNames)
        {
            foreach (string name in columnNames)
                columns.Add(new TextColumn(name));
        }

        /// <summary>
        /// Adds several <see cref="ValueColumn{T}"/> objects to this
        /// <see cref="CSVFile"/> with the given names.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the columns.
        /// </typeparam>
        /// <param name="converter">The converter method to use
        /// for converting new text values to the backing type of the
        /// new columns.</param>
        /// <param name="columnNames">The names of the columns to
        /// append.</param>
        public void BindValueColumns<T>(Func<string, T?> converter, params string[] columnNames) where T : struct
        {
            foreach (string columnName in columnNames)
                columns.Add(new ValueColumn<T>(columnName, converter));
        }

        #region IEnumerable
        // Note: might've made an error here; I'll have to dig into the docs to check.
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
        #endregion

        /// <summary>
        /// Returns the <see cref="Column"/> present in this
        /// <see cref="CSVFile"/> at the given index.
        /// </summary>
        /// <param name="index">The index of the column to return.</param>
        public Column this[int index]
            => columns[index];

        /// <summary>
        /// Returns the <see cref="Column"/> present in this
        /// <see cref="CSVFile"/> that has the given name.
        /// <para>Throws an <see cref="IndexOutOfRangeException"/> if the
        /// name was not found.</para>
        /// </summary>
        /// <param name="name">The name of the <see cref="Column"/> to
        /// look up.</param>
        public Column this[string name]
            => columns[GetIndexOfColumn(name)];

        /// <summary>
        /// Adds an existing <see cref="Column"/> to the list of columns
        /// present in this <see cref="CSVFile"/>.
        /// <para>Throws an <see cref="ArgumentException"/> if the number of
        /// rows in the <see cref="CSVFile"/> doesn't match the length of the
        /// given <see cref="Column"/>.</para>
        /// </summary>
        /// <param name="column">The <see cref="Column"/> to append.</param>
        public void AddColumn(Column column)
        {
            // Throw an exception if the lengths don't match.
            if (rows != column.Length)
            {
                throw new ArgumentException(
                    "Appending a new Column requires it to have the same length as the columns present in this CSVFile."
                );
            }

            columns.Add(column);
        }

        /// <summary>
        /// Adds existing <see cref="Column"/> objects to the list of columns
        /// present in this <see cref="CSVFile"/>.
        /// <para>Throws an <see cref="ArgumentException"/> if the number of
        /// rows in the <see cref="CSVFile"/> doesn't match the length of the
        /// given <see cref="Column"/> instances.</para>
        /// </summary>
        /// <param name="columns">The <see cref="Column"/> objects to
        /// append.</param>
        public void AddColumns(params Column[] columns)
        {
            // Validate column lengths.
            for (int i = 0; i < columns.Length; i++)
            {
                Column column = columns[i];
                if (column.Length != rows)
                {
                    throw new ArgumentException(
                        $"Appending a new Column requires it to have the same length as the columns present in this CSVFile.\nProblematic column: index {i}, name {column.name}."
                    );
                }
            }

            this.columns.AddRange(columns);
        }

        /// <summary>
        /// Gets the column at the given index, cast to the given subclass
        /// type. Be wary of exceptions.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Column"/>
        /// to cast to.</typeparam>
        /// <param name="index">The index of the <see cref="Column"/>
        /// to return.</param>
        public T GetColumn<T>(int index) where T : Column
            => (T)columns[index];

        /// <summary>
        /// Gets the column with the given name, cast to the given subclass
        /// type. Be wary of <see cref="IndexOutOfRangeException"/>, in case
        /// the name is not found, and <see cref="InvalidCastException"/> if
        /// the targeted <see cref="Column"/> is not of the appropriate
        /// subclass.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Column"/>
        /// to cast to.</typeparam>
        /// <param name="name">The name of the <see cref="Column"/> to look
        /// up.</param>
        public T GetColumn<T>(string name) where T : Column
            => (T)columns[GetIndexOfColumn(name)];

        /// <summary>
        /// Renames a column with the given name to the given new name.
        /// <para>Throws an <see cref="ArgumentException"/> if the current
        /// name that is given is either not valid or not found, or if a
        /// column already exists that has the given new name.</para>
        /// </summary>
        /// <param name="currentName">The current name of the column
        /// that ought to be renamed.</param>
        /// <param name="newName">The name that the column should be
        /// given.</param>
        public void RenameColumn(string currentName, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException(
                    $"The new name is either null or whitespace and is thus not valid!",
                    nameof(newName)
                );
            }

            int columnIndex = GetIndexOfColumn(currentName);
            if (columnIndex == -1)
            {
                throw new ArgumentException(
                    $"There is no column named {currentName} in this CSV file!",
                    nameof(currentName)
                );
            }

            if (GetIndexOfColumn(newName) != -1)
            {
                throw new ArgumentException(
                    $"There is already a column named {newName} in this CSV file!",
                    nameof(newName)
                );
            }

            columns[columnIndex].name = newName;
        }

        /// <summary>
        /// Returns the textual representation of this <see cref="CSVFile"/>,
        /// ready to be written as a .csv file.
        /// </summary>
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

        /// <summary>
        /// Returns the textual representation of this <see cref="CSVFile"/>,
        /// ready to be written as a .csv file. This variant pre-computes the
        /// width of each column for maximal human readability.
        /// <para>The memory requirement of this variant increase
        /// exponentially with the size of the <see cref="CSVFile"/>.</para>
        /// </summary>
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
                    string value = column.ToString(row);
                    
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
