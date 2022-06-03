
using Microsoft.AspNetCore.Mvc;
using model;

namespace api_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticleController : ControllerBase
{
    private readonly IArticleRepository _articleRepository;

    public ArticleController(IArticleRepository articleRepository) =>
        _articleRepository = articleRepository;

    [HttpGet]
    public async Task<List<Article>> Get() =>
        await _articleRepository.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Article>> Get(string id)
    {
        var article = await _articleRepository.GetAsync(id);

        if (article is null)
        {
            return NotFound();
        }

        return article;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Article newBook)
    {
        await _articleRepository.CreateAsync(newBook);

        return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Article updatedArticle)
    {
        var article = await _articleRepository.GetAsync(id);

        if (article is null)
        {
            return NotFound();
        }

        updatedArticle.Id = article.Id;

        await _articleRepository.UpdateAsync(id, updatedArticle);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var article = await _articleRepository.GetAsync(id);

        if (article is null)
        {
            return NotFound();
        }

        await _articleRepository.RemoveAsync(id);

        return NoContent();
    }
}