using MongoDB.Bson;

namespace model;

public interface IPlaceRepository
{
   Task<IEnumerable<BsonDocument>> GetByAggregateAsync(double latitude, double longitude, int limit);
   Task<IEnumerable<BsonDocument>> GetByFindAsync(double latitude, double longitude, int limit);
}