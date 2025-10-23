using MongoDB.Driver;
using myapp.Models;

namespace myapp.Services;

public class CounterService
{
    private readonly IMongoCollection<CounterDoc> _col;
    private const string TableName = "click";             // ← collection-namn
    private const string KeyValue  = "counter#frontpage";  // ← dokumentets "Name"

    public CounterService(IMongoClient client, IConfiguration cfg)
    {
        var dbName = cfg["Mongo:Database"] ?? "myapp";
        var db = client.GetDatabase(dbName);
        _col = db.GetCollection<CounterDoc>(TableName);

        // Säkerställ unikt index på Name (så vi har exakt ett dokument)
        var idx = new CreateIndexModel<CounterDoc>(
            Builders<CounterDoc>.IndexKeys.Ascending(x => x.Name),
            new CreateIndexOptions { Unique = true });
        _col.Indexes.CreateOne(idx);
    }

    public async Task<(int value, string inst, string at)> IncrementAsync(CancellationToken ct = default)
    {
        var inst = Environment.GetEnvironmentVariable("HOSTNAME")
                   ?? Environment.MachineName
                   ?? "local";
        var nowIso = DateTimeOffset.UtcNow.ToString("o");

        var filter = Builders<CounterDoc>.Filter.Eq(x => x.Name, KeyValue);

        // $inc skapar fält om de inte finns; funkar även på nested path "InstanceCounts.<inst>"
        var update = Builders<CounterDoc>.Update
            .Inc(x => x.Value, 1)
            .Inc($"InstanceCounts.{inst}", 1)
            .Set(x => x.LastInstanceId, inst)
            .Set(x => x.LastAt, nowIso);

        var opts = new FindOneAndUpdateOptions<CounterDoc>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var doc = await _col.FindOneAndUpdateAsync(filter, update, opts, ct);
        var newVal = (int)doc.Value; // cast räcker om räknaren är rimlig storlek
        return (newVal, inst, nowIso);
    }

    public async Task<(int value, string lastInst, string lastAt)> ReadAsync(CancellationToken ct = default)
    {
        var doc = await _col.Find(x => x.Name == KeyValue).FirstOrDefaultAsync(ct);
        if (doc is null)
            return (0, "-", "-");

        var value = (int)doc.Value;
        var lastInst = doc.LastInstanceId ?? "-";
        var lastAt   = doc.LastAt ?? "-";
        return (value, lastInst, lastAt);
    }
}
