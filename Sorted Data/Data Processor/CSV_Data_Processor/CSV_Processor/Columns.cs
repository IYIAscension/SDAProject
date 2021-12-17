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
        public string name;
        public readonly Type backingType;

        protected Column(string name, Type backingType)
        {
            this.name = name;
            this.backingType = backingType;
        }

        public abstract int Length { get; }
        public abstract void Append(string value);
        public abstract override string ToString();
        public abstract string ToString(int index);
        public abstract object GetValue(int index);
    }

    public sealed class ValueColumn<T> : Column where T : struct
    {
        private readonly Func<string, T?> converter;
        private Func<T, string> writer;
        private readonly List<T?> values = new List<T?>();

        public void SetWriter(Func<T, string> writer)
        {
            if (writer == null)
                this.writer = x => x.ToString();
            else
                this.writer = writer;
        }

        public ValueColumn(string columnName, Func<string, T?> converter, Func<T, string> writer = null) : base(columnName, typeof(T))
        {
            this.converter = converter;
            SetWriter(writer);
        }

        public T? this[int index] => values[index];

        public override int Length => values.Count;

        public override object GetValue(int index)
            => values[index];

        public override void Append(string value)
        {
            values.Add(converter(value));
        }

        public void Append(T? value)
            => values.Add(value);

        public override string ToString()
            => string.Join(
                ", ",
                values.Select(x => x.HasValue ? x.ToString() : string.Empty)
            );

        public override string ToString(int index)
        {
            var value = values[index];
            return value.HasValue ? writer(value.Value) : string.Empty;
        }
    }

    public sealed class TextColumn : Column
    {
        private readonly List<string> values = new List<string>();

        public TextColumn(string columnName) : base(columnName, typeof(string))
        { }

        public string this[int index] => values[index];

        public override int Length => values.Count;

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
