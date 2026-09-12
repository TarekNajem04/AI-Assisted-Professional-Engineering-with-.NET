// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Api/Serialization/ValueObjectConverters.cs
//
// HTTP boundary concern, not a domain concern: System.Text.Json cannot bind
// a bare JSON string ("3F2504E0-…") to the single-property record structs
// CustomerId/PaymentId — it expects {"Value":"…"}. Without these converters
// every POST /payments with natural JSON fails model binding with a 400,
// which is exactly the kind of boundary assumption S01 says to make explicit.
// The domain keeps its strong types; the boundary translates. Neither leaks
// into the other.

using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentService.Core.Domain;

namespace PaymentService.Api.Serialization;

public sealed class CustomerIdJsonConverter : JsonConverter<CustomerId>
{
    public override CustomerId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var text = reader.GetString();
        if (!Guid.TryParse(text, out var value))
            throw new JsonException($"Invalid CustomerId: '{text}'. Expected a GUID string.");
        return new CustomerId(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CustomerId value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value.ToString("D"));
    }
}

public sealed class PaymentIdJsonConverter : JsonConverter<PaymentId>
{
    public override PaymentId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var text = reader.GetString();
        if (!Guid.TryParse(text, out var value))
            throw new JsonException($"Invalid PaymentId: '{text}'. Expected a GUID string.");
        return new PaymentId(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        PaymentId value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value.ToString("D"));
    }
}
