using Microsoft.Extensions.Options;
using model;
using model.Configuration;
using MongoDB.Driver;

namespace repository;

public class ArticleRepository : IArticleRepository
{
    private readonly IMongoCollection<Article> _articleCollection;

    public ArticleRepository(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var mongoDatabase = mongoClient.GetDatabase(options.Value.Database);
        _articleCollection = mongoDatabase.GetCollection<Article>(options.Value.ArticlesCollectionName);
    }

    public async Task<List<Article>> GetAsync() =>
        await _articleCollection.Find(_ => true).ToListAsync();

    public async Task<Article?> GetAsync(string id) =>
        await _articleCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Article newArticle) =>
        await _articleCollection.InsertOneAsync(newArticle);

    public async Task UpdateAsync(string id, Article updatedArticle) =>
        await _articleCollection.ReplaceOneAsync(x => x.Id == id, updatedArticle);

    public async Task RemoveAsync(string id) =>
        await _articleCollection.DeleteOneAsync(x => x.Id == id);
}