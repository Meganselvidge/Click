using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace myapp.Models;

public class CounterDoc
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; } = default!;

    [BsonElement("Value")]
    public long Value { get; set; }

    [BsonElement("LastInstanceId")]
    public string? LastInstanceId { get; set; }

    // Lagra ISO8601 som i din Dynamo-kod (enkelt att visa i UI/Compass)
    [BsonElement("LastAt")]
    public string? LastAt { get; set; }

    [BsonElement("InstanceCounts")]
    public Dictionary<string, long> InstanceCounts { get; set; } = new();
}