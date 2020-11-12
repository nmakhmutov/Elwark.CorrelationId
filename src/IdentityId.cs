using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;

namespace Elwark.People.Abstractions
{
    [
        JsonConverter(typeof(JsonConverter)),
        TypeConverter(typeof(TypesConverter))
    ]
    public readonly struct IdentityId : IComparable, IComparable<IdentityId>, IEquatable<IdentityId>, IFormattable
    {
        public Guid Value { get; }

        public IdentityId(Guid value) =>
            Value = value;

        public int CompareTo(IdentityId other) => Value.CompareTo(other.Value);

        public int CompareTo(object? obj) =>
            obj switch
            {
                null => 1,
                IdentityId code => CompareTo(code),
                _ => throw new ArgumentException($"Object is not {nameof(IdentityId)}")
            };

        public bool Equals(IdentityId other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is IdentityId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();

        public string ToString(string? format, IFormatProvider? formatProvider) =>
            Value.ToString(format, formatProvider);

        public static bool operator ==(IdentityId a, IdentityId b) => a.CompareTo(b) == 0;
        public static bool operator !=(IdentityId a, IdentityId b) => !(a == b);

        public static explicit operator Guid(IdentityId identityId) => identityId.Value;
        public static implicit operator IdentityId(Guid id) => new(id);

        public static IdentityId Parse(string? value) =>
            Guid.Parse(value ?? throw new ArgumentNullException(nameof(value)));

        public static bool TryParse(string? value, out IdentityId id)
        {
            if (Guid.TryParse(value, out var result))
            {
                id = result;
                return true;
            }

            id = new IdentityId(default);
            return false;
        }

        private class JsonConverter : JsonConverter<IdentityId>
        {
            public override void WriteJson(JsonWriter writer, IdentityId value, JsonSerializer serializer) =>
                serializer.Serialize(writer, value.Value);

            public override IdentityId ReadJson(JsonReader reader, Type objectType, IdentityId existingValue,
                bool hasExistingValue, JsonSerializer serializer) => serializer.Deserialize<Guid>(reader)!;
        }

        private class TypesConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
                sourceType == typeof(Guid) || base.CanConvertFrom(context, sourceType);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
                value switch
                {
                    Guid l => new IdentityId(l),
                    _ => throw new NotSupportedException($"Value {value} is not valid for {nameof(IdentityId)} type")
                };
        }
    }
}