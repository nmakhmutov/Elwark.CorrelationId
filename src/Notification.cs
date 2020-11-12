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
    public abstract class Notification : IEquatable<Notification>, IComparable<Notification>, IComparable
    {
        private Notification(NotificationType type, string value)
        {
            Type = type;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public NotificationType Type { get; }

        public string Value { get; }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;

            return obj is Notification other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Notification)}", nameof(obj));
        }

        public int CompareTo(Notification? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0) return typeComparison;

            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        public bool Equals(Notification? other)
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
            return Equals((Notification) obj);
        }

        public override int GetHashCode() =>
            HashCode.Combine((int) Type, Value);

        public static bool operator ==(Notification left, Notification right) =>
            Equals(left, right);

        public static bool operator !=(Notification left, Notification right) =>
            !Equals(left, right);

        public override string ToString() =>
            $"{Type:G}: {Value}";

        public static Notification Create(int type, string value) =>
            Create((NotificationType) type, value);

        public static Notification Create(NotificationType type, string value) =>
            type switch
            {
                NotificationType.None => new NoneNotification(),
                NotificationType.PrimaryEmail => new PrimaryEmail(value),
                NotificationType.SecondaryEmail => new SecondaryEmail(value),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public static Notification Parse(string? value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var separatorIndex = value.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
            if (separatorIndex < 0)
                throw new ArgumentException($"Incorrect json format for type '{nameof(Notification)}'",
                    nameof(value));

            if (!Enum.TryParse(value[..separatorIndex], true, out NotificationType newType))
                throw new ArgumentException($"Unknown type for '{nameof(Notification)}'", nameof(value));

            return Create(newType, value[(separatorIndex + 1)..].Trim());
        }

        private class JsonConverter : JsonConverter<Notification?>
        {
            public override void WriteJson(JsonWriter writer, Notification? value, JsonSerializer serializer)
            {
                if (value is null)
                    writer.WriteNull();
                else
                    writer.WriteValue($"{value.Type:G}:{value.Value}");
            }

            public override Notification? ReadJson(JsonReader reader, Type objectType, Notification? existingValue,
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
                || sourceType == typeof(Notification)
                || base.CanConvertFrom(context, sourceType);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
                value switch
                {
                    string s => Parse(s),
                    Notification i => i,
                    _ => throw new NotSupportedException($"Value {value} is not valid for {nameof(AccountId)} type")
                };
        }

        public sealed class NoneNotification : Notification
        {
            public NoneNotification()
                : base(NotificationType.None, string.Empty)
            {
            }
        }

        public sealed class PrimaryEmail : Notification
        {
            public PrimaryEmail(string email)
                : base(NotificationType.PrimaryEmail, email.ValidateEmail().ToLower())
            {
            }
        }

        public sealed class SecondaryEmail : Notification
        {
            public SecondaryEmail(string email)
                : base(NotificationType.SecondaryEmail, email.ValidateEmail().ToLower())
            {
            }
        }
    }
}