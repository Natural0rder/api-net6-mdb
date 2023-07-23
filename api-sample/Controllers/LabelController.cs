using api_sample.Controllers.DTO;
using Microsoft.AspNetCore.Mvc;
using model;
using MongoDB.Bson.Serialization;

namespace api_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabelController : ControllerBase
{
    private readonly ITranslationRepository _translationRepository;

    public LabelController(ITranslationRepository translationRepository) =>
        _translationRepository = translationRepository;


    [HttpGet("{id}/{type}/{lang}")]
    public async Task<LabelDto> GetTranslationAsync(string id, string type, string lang)
    {
        var label = await _translationRepository.GeTranslationAsync(id, type, lang);
    
        return label;
    }
}