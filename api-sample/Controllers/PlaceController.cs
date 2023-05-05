
using Microsoft.AspNetCore.Mvc;
using model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace api_sample.Controllers;

public class PlaceDto
{
    public string placeId { get; set;} = string.Empty;
    public double[] coordinates { get; set; } = new double[] {};

    public string[] availableAssetIds { get; set;} = new string[] {};
}

[ApiController]
[Route("api/[controller]")]
public class PlaceController : ControllerBase
{
    private readonly IPlaceRepository _placeRepository;

    public PlaceController(IPlaceRepository placeRepository) =>
        _placeRepository = placeRepository;


    [HttpGet("aggregate/{latitude}/{longitude}")]
    public async Task<IEnumerable<PlaceDto>> GetBtAggregateAsync(double latitude, double longitude, int? limit)
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