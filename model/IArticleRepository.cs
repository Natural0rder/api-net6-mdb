namespace model;

public interface IArticleRepository
{
   Task<List<Article>> GetAsync();

   Task<Article?> GetAsync(string id);

   Task CreateAsync(Article newArticle);

   Task UpdateAsync(string id, Article updatedArticle);

   Task RemoveAsync(string id);
}