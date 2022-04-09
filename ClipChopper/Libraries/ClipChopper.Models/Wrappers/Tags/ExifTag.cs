using System.Collections.Generic;
using Acolyte.Assertions;

namespace ClipChopper.Models.Wrappers.Tags
{
    public sealed class ExifTag
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string TableName { get; }
        public string Group { get; }
        public string Value { get; }
        public string? NumberValue { get; }
        public IReadOnlyList<string>? List { get; }


        public ExifTag(
            string id,
            string name,
            string description,
            string tableName,
            string group,
            string value,
            string? numberValue,
            IReadOnlyList<string>? list)
        {
            Id = id.ThrowIfNull(nameof(id));
            Name = name.ThrowIfNull(nameof(name));
            Description = description.ThrowIfNull(nameof(description));
            TableName = tableName.ThrowIfNull(nameof(tableName));
            Group = group.ThrowIfNull(nameof(group));
            Value = value.ThrowIfNull(nameof(value));
            NumberValue = numberValue;
            List = list;
        }
    }
}
