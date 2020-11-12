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
    public abstract class Identification : IEquatable<Identification>, IComparable<Identification>, IComparable
    {
        private Identification(IdentificationType type, string value)
        {
            Type = type;
            
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            
            Value = value;
        }

        public IdentificationType Type { get; }

        public string Value { get; }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;

            return obj is Identification other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Identification)}", nameof(obj));
        }

        public int CompareTo(Identification? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0) return typeComparison;

            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        public bool Equals(Identification? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Type == other.Type && Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Identification) obj);
        }

        public override int GetHashCode() =>
            HashCode.Combine((int) Type, Value);

        public static bool operator ==(Identification left, Identification right) =>
            Equals(left, right);

        public static bool operator !=(Identification left, Identification right) =>
            !Equals(left, right);

        public override string ToString() =>
            $"{Type:G}: {Value}";

        public static Identification Create(int type, string value) =>
            Create((IdentificationType) type, value);

        public static Identification Create(IdentificationType type, string value) =>
            type switch
            {
                IdentificationType.Email => new Email(value),
                IdentificationType.Google => new Google(value),
                IdentificationType.Facebook => new Facebook(value),
                IdentificationType.Microsoft => new Microsoft(value),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public static Identification Parse(string? value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var separatorIndex = value.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
            if (separatorIndex < 0)
                throw new ArgumentException($"Incorrect json format for type '{nameof(Identification)}'",
                    nameof(value));
            
            if (!Enum.TryParse(value[..separatorIndex], true, out IdentificationType newType))
                throw new ArgumentException($"Unknown type for '{nameof(Identification)}'", nameof(value));

            return Create(newType, value[(separatorIndex + 1)..].Trim());
        }

        private class JsonConverter : JsonConverter<Identification?>
        {
            public override void WriteJson(JsonWriter writer, Identification? value, JsonSerializer serializer)
            {
                if (value is null)
                    writer.WriteNull();
                else
                    writer.WriteValue($"{value.Type:G}:{value.Value}");
            }

            public override Identification? ReadJson(JsonReader reader, Type objectType, Identification? existingValue,
                bool hasExistingValue, JsonSerializer serializer)
            {
                var value = serializer.Deserialize<string>(reader);
                return value is null ? null : Parse(value);
            }
        }

        private class TypesConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
                sourceType == typeof(string)
                || sourceType == typeof(Identification)
                || base.CanConvertFrom(context, sourceType);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
                value switch
                {
                    string s => Parse(s),
                    Identification i => i,
                    _ => throw new NotSupportedException($"Value {value} is not valid for {nameof(AccountId)} type")
                };
        }

        public sealed class Email : Identification
        {
            public Email(string email)
                : base(IdentificationType.Email, email.ValidateEmail().ToLower())
            {
            }
        }

        public sealed class Google : Identification
        {
            public Google(string value)
                : base(IdentificationType.Google, value)
            {
            }
        }

        public sealed class Facebook : Identification
        {
            public Facebook(string value)
                : base(IdentificationType.Facebook, value)
            {
            }
        }

        public sealed class Microsoft : Identification
        {
            public Microsoft(string value)
                : base(IdentificationType.Microsoft, value)
            {
            }
        }
    }
}