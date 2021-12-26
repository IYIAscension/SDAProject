using System;
using System.Linq;
using System.Collections.Generic;

namespace CSV_Processor
{
    /// <summary>
    /// Represents a generic CSV column with data of unknown type.
    /// </summary>
    public abstract class Column
    {
        /// <summary>
        /// The name of this <see cref="Column"/> as present in the first line
        /// of the CSV file.
        /// </summary>
        public string name;

        /// <summary>
        /// The <see cref="Type"/> of the data that is stored in this Column.
        /// </summary>
        public readonly Type backingType;

        // For child classes: sets the fields of this column.
        protected Column(string name, Type backingType)
        {
            this.name = name;
            this.backingType = backingType;
        }

        /// <summary>
        /// The number of elements stored in this CSV column.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Appends the given string value to the end of this CSV Column.
        /// <para> This also performs conversion to the <see cref="Column"/>'s
        /// backing <see cref="Type"/> under the hood, and may throw whichever
        /// <see cref="Exception"/> may occur in the parsing method used.
        /// </para>
        /// </summary>
        /// <param name="value">The string to append.</param>
        public abstract void Append(string value);

        /// <summary>
        /// Returns the <see cref="string"/> representation of the values
        /// stored in this <see cref="Column"/>.
        /// </summary>
        public abstract override string ToString();

        /// <summary>
        /// Returns the <see cref="string"/> of the element at the given index
        /// in the <see cref="Column"/>.
        /// <para>Throws an <see cref="IndexOutOfRangeException"/> if the
        /// index is greater than or equal to <see cref="Length"/>.</para>
        /// </summary>
        /// <param name="index">The index to convert to a string.</param>
        /// <returns></returns>
        public abstract string ToString(int index);

        /// <summary>
        /// Returns the <see cref="object"/> stored at the given index in the
        /// <see cref="Column"/>, which will be of <see cref="Type"/>
        /// <see cref="backingType"/>.
        ///  <para>Throws an <see cref="IndexOutOfRangeException"/> if the
        /// index is greater than or equal to <see cref="Length"/>.</para>
        /// </summary>
        /// <param name="index">The index whose corresponding
        /// <see cref="object"/> ought to be returned.</param>
        /// <returns></returns>
        public abstract object GetValue(int index);
    }

    /// <summary>
    /// Represents a CSV <see cref="Column"/> that stores binary struct data
    /// at each index.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of struct to
    /// store at each index.</typeparam>
    public sealed class ValueColumn<T> : Column where T : struct
    {
        /// <summary>
        /// The function that is used to convert strings to the backing type.
        /// </summary>
        private readonly Func<string, T?> converter;

        /// <summary>
        /// The function that is used to convert an instance of the backing
        /// type to a <see cref="string"/>, in case custom functionality is
        /// needed.
        /// </summary>
        private Func<T, string> writer;

        /// <summary>
        /// The backing list of values that represent the <see cref="Column"/>.
        /// </summary>
        private readonly List<T?> values = new List<T?>();

        /// <summary>
        /// Changes the writer function to the supplied method. Use this to
        /// assign custom ToString behaviour, or assign null to reset the
        /// writer to the default ToString method.
        /// </summary>
        /// <param name="writer"></param>
        public void SetWriter(Func<T, string> writer)
        {
            if (writer == null)
                this.writer = x => x.ToString();
            else
                this.writer = writer;
        }

        /// <summary>
        /// Creates a new <see cref="ValueColumn{T}"/> with the given name,
        /// the given converter function, and optionally, a custom writer
        /// function.
        /// </summary>
        /// <param name="columnName">The name of this column.</param>
        /// <param name="converter">The method used to convert text objects
        /// to instances of the column's backing struct type.</param>
        /// <param name="writer">If necessary, a custom behaviour to supersede
        /// <see cref="T.ToString()"/></param>
        public ValueColumn(string columnName, Func<string, T?> converter, Func<T, string> writer = null) : base(columnName, typeof(T))
        {
            // Base ctor invocation assigned columnName and backingType.
            // Assign the remaining fields.
            this.converter = converter;
            SetWriter(writer);
        }

        /// <summary>
        /// Returns the object stored at the given index in this column.
        /// </summary>
        /// <param name="index">The index of the object to return.</param>
        /// <returns></returns>
        public T? this[int index] => values[index];

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int Length => values.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        /// <returns></returns>
        public override object GetValue(int index)
            => values[index];

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public override void Append(string value)
            => values.Add(converter(value));

        /// <summary>
        /// Appends the given <see cref="T"/> object to the end of this
        /// <see cref="ValueColumn{T}"/> instance.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void Append(T? value)
            => values.Add(value);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
            => string.Join(
                ", ",
                values.Select(x => x.HasValue ? x.ToString() : string.Empty)
            );

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"></param>
        public override string ToString(int index)
        {
            var value = values[index];
            return value.HasValue ? writer(value.Value) : string.Empty;
        }
    }

    /// <summary>
    /// Represents a CSV <see cref="Column"/> that exclusively stores text
    /// objects.
    /// </summary>
    public sealed class TextColumn : Column
    {
        /// <summary>
        /// The values that are stored in this TextColumn object.
        /// </summary>
        private readonly List<string> values = new List<string>();

        /// <summary>
        /// Creates a new <see cref="TextColumn"/> with the given name.
        /// </summary>
        /// <param name="columnName">The name of this column.</param>
        public TextColumn(string columnName) : base(columnName, typeof(string))
        { }

        /// <summary>
        /// Returns the <see cref="string"/> stored at this index in the
        /// <see cref="TextColumn"/>.
        /// </summary>
        /// <param name="index">The index to look up.</param>
        public string this[int index] => values[index];

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int Length => values.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        /// <returns></returns>
        public override object GetValue(int index)
            => values[index];

        /// <summary>
        /// Appends the given <see cref="string"/> to the end of this
        /// <see cref="TextColumn"/>.
        /// </summary>
        /// <param name="value">The text to append.</param>
        public override void Append(string value)
            => values.Add(value);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
            => string.Join(", ", values);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        public override string ToString(int index)
            => values[index];
    }
}
