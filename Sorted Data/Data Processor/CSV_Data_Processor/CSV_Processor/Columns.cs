using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace CSV_Processor
{
    /// <summary>
    /// Represents a generic CSV column with data of unknown type.
    /// </summary>
    public abstract class Column
    {
        public readonly string name;
        public readonly Type backingType;

        protected Column(string name, Type backingType)
        {
            this.name = name;
            this.backingType = backingType;
        }

        public abstract void Append(string value);
        public abstract override string ToString();
        public abstract string ToString(int index);
        public abstract object GetValue(int index);
    }

    public sealed class ValueColumn<T> : Column where T : struct
    {
        private readonly Func<string, T?> converter;
        private readonly List<T?> values = new List<T?>();

        public ValueColumn(string columnName, Func<string, T?> converter) : base(columnName, typeof(T))
        {
            this.converter = converter;
        }

        public T? this[int index] => values[index];

        public override object GetValue(int index)
            => values[index];

        public override void Append(string value)
        {
            values.Add(converter(value));
        }

        public override string ToString()
            => string.Join(
                ", ",
                values.Select(x => x.HasValue ? x.ToString() : string.Empty)
            );

        public override string ToString(int index)
        {
            var value = values[index];
            return value.HasValue ? value.ToString() : string.Empty;
        }
    }

    public sealed class TextColumn : Column
    {
        private readonly List<string> values = new List<string>();

        public TextColumn(string columnName) : base(columnName, typeof(string))
        { }

        public string this[int index] => values[index];

        public override object GetValue(int index)
            => values[index];

        public override void Append(string value)
            => values.Add(value);

        public override string ToString()
            => string.Join(", ", values);

        public override string ToString(int index)
            => values[index];
    }
}
