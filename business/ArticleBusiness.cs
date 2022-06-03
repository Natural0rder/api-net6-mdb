using model;

namespace business;

/// <summary>
/// Put business logic here!
/// </summary>
public class ArticleBusiness : IArticleBusiness
{
    private readonly IArticleRepository _repository;

    public ArticleBusiness(IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Article>> GetAsync() =>
        await _repository.GetAsync();

    public async Task<Article?> GetAsync(string id) =>
        await _repository.GetAsync(id);

    public async Task CreateAsync(Article newArticle) =>
        await _repository.CreateAsync(newArticle);

    public async Task UpdateAsync(string id, Article updatedArticle) =>
        await _repository.UpdateAsync(id, updatedArticle);

    public async Task RemoveAsync(string id) =>
        await _repository.RemoveAsync(id);
}