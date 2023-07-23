using System.Diagnostics;
using Microsoft.Extensions.Options;
using model;
using model.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace repository;

public class TranslationRepository : ITranslationRepository
{
    private readonly IMongoCollection<LabelDto> _labelCollection;

    public TranslationRepository(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var mongoDatabase = mongoClient.GetDatabase(options.Value.Database);
        _labelCollection = mongoDatabase.GetCollection<LabelDto>(options.Value.TranslationCollectionName);
    }

    public async Task<LabelDto> GeTranslationAsync(string id, string type, string lang)
    {
        
        var builder = Builders<LabelDto>.Filter;
        var filter = builder.Eq(x => x.EntityId, id) & builder.Eq(x => x.Type, type) & builder.Where(l => l.Translations.ContainsKey(lang));

        var projectionBuilder = Builders<LabelDto>.Projection;
        var projection = projectionBuilder
            .Include(x => x.EntityId)
            .Include(x => x.Type)
            .Include(x => x.Translations[lang]);

        var result = await _labelCollection
            .Find(filter)
            .Project<LabelDto>(projection)
            .FirstOrDefaultAsync();

       return result;
    }
}