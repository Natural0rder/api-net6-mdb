using api_sample.Controllers.DTO;
using Microsoft.AspNetCore.Mvc;
using model;
using MongoDB.Bson.Serialization;

namespace api_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaceController : ControllerBase
{
    private readonly IPlaceRepository _placeRepository;

    public PlaceController(IPlaceRepository placeRepository) =>
        _placeRepository = placeRepository;


    [HttpGet("aggregate/{latitude}/{longitude}")]
    public async Task<IEnumerable<PlaceDto>> GetByAggregateAsync(double latitude, double longitude, int? limit)
    {
        var places = await _placeRepository.GetByAggregateAsync(latitude, 
                                                                longitude, 
                                                                limit.HasValue ? limit.Value : 20000);
        var dto = places.Select(x => BsonSerializer.Deserialize<PlaceDto>(x));

        return dto;
    }

    [HttpGet("find/{latitude}/{longitude}")]
    public async Task<IEnumerable<PlaceDto>> GetByFindAsync(double latitude, double longitude, int? limit)
    {
        var places = await _placeRepository.GetByFindAsync(latitude, 
                                                           longitude, 
                                                           limit.HasValue ? limit.Value : 20000);
        var dto = places.Select(x => BsonSerializer.Deserialize<PlaceDto>(x));

        return dto;
    }
}