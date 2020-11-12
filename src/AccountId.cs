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
    public readonly struct AccountId : IComparable, IComparable<AccountId>, IEquatable<AccountId>
    {
        public long Value { get; }

        public AccountId(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            Value = value;
        }

        public int CompareTo(AccountId other) => Value.CompareTo(other.Value);

        public int CompareTo(object? obj) =>
            obj switch
            {
                null => 1,
                AccountId code => CompareTo(code),
                _ => throw new ArgumentException($"Object is not {nameof(AccountId)}")
            };

        public bool Equals(AccountId other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is AccountId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();

        public static bool operator ==(AccountId a, AccountId b) => a.CompareTo(b) == 0;
        public static bool operator !=(AccountId a, AccountId b) => !(a == b);

        public static bool operator >(AccountId a, AccountId b) => a.Value > b.Value;
        public static bool operator <(AccountId a, AccountId b) => a.Value < b.Value;

        public static bool operator <=(AccountId a, AccountId b) => a.Value <= b.Value;
        public static bool operator >=(AccountId a, AccountId b) => a.Value >= b.Value;

        public static explicit operator long(AccountId accountId) => accountId.Value;
        public static implicit operator AccountId(long id) => new AccountId(id);

        public static AccountId Parse(string? value) =>
            long.Parse(value ?? throw new ArgumentNullException(nameof(value)));

        public static bool TryParse(string? value, out AccountId id)
        {
            if (long.TryParse(value, out var result))
            {
                id = result;
                return true;
            }

            id = new AccountId(default);
            return false;
        }

        private class JsonConverter : JsonConverter<AccountId>
        {
            public override void WriteJson(JsonWriter writer, AccountId value, JsonSerializer serializer) =>
                serializer.Serialize(writer, value.Value);

            public override AccountId ReadJson(JsonReader reader, Type objectType, AccountId existingValue,
                bool hasExistingValue, JsonSerializer serializer) => serializer.Deserialize<long>(reader)!;
        }

        private class TypesConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
                sourceType == typeof(long) || base.CanConvertFrom(context, sourceType);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
                value switch
                {
                    long l => new AccountId(l),
                    _ => throw new NotSupportedException($"Value {value} is not valid for {nameof(AccountId)} type")
                };
        }
    }
}