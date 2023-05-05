using System.Diagnostics;
using Microsoft.Extensions.Options;
using model;
using model.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace repository;

public class PlaceRepository : IPlaceRepository
{
    private readonly IMongoCollection<BsonDocument> _placeCollection;

    public PlaceRepository(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var mongoDatabase = mongoClient.GetDatabase(options.Value.Database);
        _placeCollection = mongoDatabase.GetCollection<BsonDocument>(options.Value.PlacesCollectionName);
    }

    public async Task<IEnumerable<BsonDocument>> GetByFindAsync(double latitude, double longitude, int limit)
    {
        var query = new BsonDocument
                    {
                        { "location", new BsonDocument("$nearSphere", new BsonDocument
                            {
                                { "$geometry", new BsonDocument
                                    {
                                        { "type", "Point" },
                                        { "coordinates", new BsonArray { longitude, latitude } }
                                    } 
                                },
                                { "$maxDistance", 50000 }
                            }) 
                        },
                        { "asset", new BsonDocument("$elemMatch", new BsonDocument("status", "Available")) }
                    };

        var filterAsset =  new BsonDocument("$filter", 
                                        new BsonDocument {
                                            { "input", "$asset" }, 
                                            { "as", "out" }, 
                                            { "cond", new BsonDocument("$in", new BsonArray { "$$out.status", new BsonArray { "Available" }})}
                                        }
                                    );

        var mapAndFilterAsset = new BsonDocument("$map",
                                    new BsonDocument {
                                        { "input", filterAsset },
                                        { "as", "filteredAsset"},
                                        { "in", "$$filteredAsset.uid" } 
                                    }
                                );

        var project =  new BsonDocument {
                            { "placeId", 1 }, 
                            { "coordinates", "$location.coordinates" },
                            { "availableAssetIds", mapAndFilterAsset },
                            { "_id", 0 }
                        };

        var sw = new Stopwatch();
        sw.Start();

        var places = await _placeCollection.Find(query)
                                           .Project(project)
                                           .Limit(limit)
                                           .ToListAsync();

        sw.Stop();

        Console.WriteLine($"GetByFind MQL execution time: {sw.ElapsedMilliseconds} ms for {places.Count} places.");

        return places;
    }


    public async Task<IEnumerable<BsonDocument>> GetByAggregateAsync(double latitude, double longitude, int limit)
    {
        var geoNearStage =  new BsonDocument("$geoNear", 
                                new BsonDocument {
                                    { "near", 
                                        new BsonDocument {
                                            { "type", "Point" }, 
                                            { "coordinates", new BsonArray { longitude, latitude } }
                                        } 
                                    }, 
                                    { "maxDistance", 50000 }, 
                                    { "query", new BsonDocument("asset", 
                                                    new BsonDocument("$elemMatch", 
                                                        new BsonDocument("status", "Available")
                                                    )
                                                ) 
                                    }, 
                                    { "spherical", true }, 
                                    { "distanceField", "dist.calculated" }
                                }
                            );


        var addFieldsStage = new BsonDocument("$addFields", 
                                new BsonDocument("availableAssets", 
                                    new BsonDocument("$filter", 
                                        new BsonDocument {
                                            { "input", "$asset" }, 
                                            { "as", "out" }, 
                                            { "cond", new BsonDocument("$in", new BsonArray { "$$out.status", new BsonArray { "Available" }})}
                                        }
                                    )
                                )
                            );

        var projectStage = new BsonDocument("$project", 
                                new BsonDocument {
                                    { "placeId", 1 }, 
                                    { "coordinates", "$location.coordinates" },
                                    { "availableAssetIds", "$availableAssets.uid" },
                                    { "_id", 0 }
                                }
                            );

       
        var limitStage = new BsonDocument { { "$limit", limit } };
        
        var pipeline = new BsonDocument[] {
            geoNearStage,
            addFieldsStage,
            projectStage,
            limitStage
        };

        var sw = new Stopwatch();
        sw.Start();

        var places = await _placeCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

        sw.Stop();

        Console.WriteLine($"GetByAggregate MQL execution time: {sw.ElapsedMilliseconds} ms for {places.Count} places.");

        return places;
    }
}