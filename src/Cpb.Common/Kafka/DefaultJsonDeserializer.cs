using System.Text.Json;
using Confluent.Kafka;

namespace Cpb.Common.Kafka;

public class DefaultJsonDeserializer<T> : IDeserializer<T> where T : class
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) =>
        JsonSerializer.Deserialize<T>(data);   
}
