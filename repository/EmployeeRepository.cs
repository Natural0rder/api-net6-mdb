using Microsoft.Extensions.Options;
using model;
using model.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IMongoCollection<Employee> _employeeCollection;
    private readonly IMongoCollection<EmployeeRead> _employeeReadCollection;
    private readonly IMongoCollection<BsonDocument> _employeeReadBsonCollection;

    private const string __AUTOCOMPLETE = "autocomplete";
    private const string __REGEX = "regex";

    public EmployeeRepository(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var mongoDatabase = mongoClient.GetDatabase(options.Value.Database);
        _employeeCollection = mongoDatabase.GetCollection<Employee>(options.Value.EmployeeCollectionName);
        _employeeReadCollection = mongoDatabase.GetCollection<EmployeeRead>(options.Value.EmployeeCollectionName);
        _employeeReadBsonCollection = mongoDatabase.GetCollection<BsonDocument>(options.Value.EmployeeCollectionName);
    }

    public async Task SetClientIdAsync()
    {
        while (true)
        {
            var filterDefinition = Builders<Employee>.Filter.Exists(p => p.ClientId, false);
            var projectionBuilder = Builders<Employee>.Projection;
            var projection = projectionBuilder.Include(x => x.Id);
            Random rnd = new Random();
            int chunk = rnd.Next(10, 10000);
            var employeesToUpdate = await _employeeCollection.Find(filterDefinition)
                                                             .Limit(chunk)
                                                             .Project(projection)
                                                             .ToListAsync();

            if (!employeesToUpdate.Any()) break;

            var clientId = ObjectId.GenerateNewId();
            var updateDefinition = Builders<Employee>.Update.Set(p => p.ClientId, clientId);
            var listWrites = new List<WriteModel<Employee>>();

            foreach (var employee in employeesToUpdate)
            {
                listWrites.Add(new UpdateOneModel<Employee>(Builders<Employee>.Filter.Eq(x => x.Id, employee[0]),
                                                            updateDefinition));
            }

            var bulkWriteResult = await _employeeCollection.BulkWriteAsync(listWrites);
            Console.WriteLine($"{bulkWriteResult.ModifiedCount} items updated.");
        }
    }

    #region BTree implementation private members

    private FilterDefinition<EmployeeRead> BuildFilter(ObjectId clientId, string? startWith = null)
    {
        var builder = Builders<EmployeeRead>.Filter;
        var filter = builder.Eq(x => x.ClientId, clientId);

        if (!string.IsNullOrEmpty(startWith) && startWith?.Length >= 2)
        {
            filter = filter &
            (
                builder.Regex(x => x.FirstName, new BsonRegularExpression($"^{startWith}", "i")) |
                builder.Regex(x => x.LastName, new BsonRegularExpression($"^{startWith}", "i")) |
                builder.Regex(x => x.Email, new BsonRegularExpression($"^{startWith}", "i"))
            );
        }

        return filter;
    }

    private ProjectionDefinition<EmployeeRead> BuildProjection()
    {
        var projectionBuilder = Builders<EmployeeRead>.Projection;
        var projection = projectionBuilder
            .Include(x => x.FirstName)
            .Include(x => x.LastName)
            .Include(x => x.Email);

        return projection;
    }

    private SortDefinition<EmployeeRead> BuildSort()
    {
        var sort = Builders<EmployeeRead>.Sort
            .Ascending(x => x.LastName)
            .Ascending(x => x.FirstName);

        return sort;
    }

    private (AggregateFacet<EmployeeRead, AggregateCountResult> CountFacet,
            AggregateFacet<EmployeeRead, EmployeeRead> DataFacet) BuildFacets(
                string countFacetName,
                string dataFacetName,
                int page,
                int pageSize,
                SortDefinition<EmployeeRead> sort)
    {
        var countFacet = AggregateFacet.Create(countFacetName,
            PipelineDefinition<EmployeeRead, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<EmployeeRead>()
            }));

        var dataFacet = AggregateFacet.Create(dataFacetName,
            PipelineDefinition<EmployeeRead, EmployeeRead>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Sort(sort),
                PipelineStageDefinitionBuilder.Skip<EmployeeRead>((page - 1) * pageSize),
                PipelineStageDefinitionBuilder.Limit<EmployeeRead>(pageSize)
            }));

        return (countFacet, dataFacet);
    }

    #endregion

    public async Task<Page<EmployeeRead>> GetByClientIdAsync(ObjectId clientId, int page, int pageSize, string? startWith = null)
    {
        const string countFacetName = "count";
        const string dataFacetName = "data";

        var filter = BuildFilter(clientId, startWith);
        var projection = BuildProjection();
        var sort = BuildSort();
        var facets = BuildFacets(countFacetName, dataFacetName, page, pageSize, sort);

       
        var agg = _employeeReadCollection.Aggregate()
            .Match(filter)
            .Project<EmployeeRead>(projection)
            .Facet(facets.CountFacet, facets.DataFacet);

        var aggregateResult = await _employeeReadCollection.Aggregate()
            .Match(filter)
            .Project<EmployeeRead>(projection)
            .Facet(facets.CountFacet, facets.DataFacet)
            .ToListAsync();

        var count = aggregateResult.First()
            .Facets.First(x => x.Name == countFacetName)
            .Output<AggregateCountResult>()
            .First()
            .Count;

        var employees = aggregateResult.First()
            .Facets.First(x => x.Name == dataFacetName)
            .Output<EmployeeRead>();

        var currentPage = new Page<EmployeeRead>
        {
            TotalItemsCount = count,
            Items = employees,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentPageSize = employees.Count,
            TotalPagesCount = (((int)count + pageSize - 1) / pageSize)
        };

        return currentPage;
    }

    #region Search implementation private members

    private BsonArray BuildSearchMustCondition(ObjectId clientId)
    {
        return new BsonArray {
                    new BsonDocument("equals",
                        new BsonDocument
                        {
                            { "path", "clientId" },
                            { "value", clientId }
                        }
                    )
                };
    }

    private BsonDocument BuildSearchSortStage()
    {
        return new BsonDocument("$sort",
                new BsonDocument {
                        { "lastName", 1 },
                        { "firstName", 1 }
                    }
                );
    }

    private BsonDocument BuildSearchProjectStage()
    {
        return new BsonDocument("$project", 
                    new BsonDocument {
                        { "firstName", 1 },
                        { "lastName", 1 },
                        { "email", 1 }
                    }
                );
    }

    private BsonDocument BuildSearchFacetStage(int page, int pageSize)
    {
        return new BsonDocument("$facet",
                    new BsonDocument {
                        { 
                            "rows",
                            new BsonArray
                            {
                                new BsonDocument("$skip", (page - 1) * pageSize),
                                new BsonDocument("$limit", pageSize)
                            } 
                        },
                        { 
                            "totalRows",
                            new BsonArray
                            {
                                new BsonDocument("$replaceWith", "$$SEARCH_META"),
                                new BsonDocument("$limit", 1)
                            } 
                        }
                    }
                );
    }

    private BsonDocument BuildSearchSetStage()
    {
        return new BsonDocument("$set",
                    new BsonDocument("totalRows",
                        new BsonDocument("$arrayElemAt",
                            new BsonArray {
                                    "$totalRows.count.lowerBound",
                                    0
                            }
                        )
                    )
                );
    }

    private BsonArray BuildSearchShouldCondition(string queryString, string searchMode)
    {
        if (searchMode == __AUTOCOMPLETE)
        {
            return new BsonArray {
                        new BsonDocument("autocomplete",
                            new BsonDocument {
                                { "path", "firstName" },
                                { "query", queryString }
                            }
                        ),
                        new BsonDocument("autocomplete",
                            new BsonDocument {
                                { "path", "lastName" },
                                { "query", queryString }
                            }
                        ),
                        new BsonDocument("autocomplete",
                            new BsonDocument {
                                { "path", "email" },
                                { "query", queryString }
                            }
                        )
                    };
        }

        return new BsonArray {
                    new BsonDocument("regex",
                        new BsonDocument {
                            { "path", "firstName" },
                            { "query", queryString },
                            { "allowAnalyzedField", true }
                        }
                    ),
                    new BsonDocument("regex",
                        new BsonDocument {
                            { "path", "lastName" },
                            { "query", queryString },
                            { "allowAnalyzedField", true }
                        }
                    ),
                    new BsonDocument("regex",
                        new BsonDocument {
                            { "path", "email" },
                            { "query", queryString },
                            { "allowAnalyzedField", true }
                        }
                    )
                };
    }

    #endregion

    public async Task<Page<EmployeeRead>> SearchAsync(ObjectId clientId, int page, int pageSize, string startWith)
    {
        if (string.IsNullOrEmpty(startWith)) throw new ArgumentNullException(nameof(startWith));

        var regexString = $"{startWith}(.*)";

        var searchStage = new BsonDocument("$search",
                                new BsonDocument {
                                    { "index", "employeeSearch" },
                                    { "compound",
                                        new BsonDocument {
                                            { 
                                                "must",
                                                BuildSearchMustCondition(clientId) 
                                            },
                                            { 
                                                "should",
                                                BuildSearchShouldCondition(regexString, __REGEX) 
                                            },
                                            { "minimumShouldMatch", 1 }
                                        } 
                                    }   
                                }
                            );

        var searchPipeline = new BsonDocument[]
        {
                searchStage,
                BuildSearchSortStage(),
                BuildSearchProjectStage(),
                BuildSearchFacetStage(page, pageSize),
                BuildSearchSetStage()
        };

        var aggResult = await _employeeReadBsonCollection.Aggregate<PaginatedSearchResult<EmployeeRead>>(searchPipeline)
                                                         .FirstOrDefaultAsync();

        var currentPage = new Page<EmployeeRead>
        {
            TotalItemsCount = aggResult.TotalRows,
            Items = aggResult.Rows,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentPageSize = aggResult.Rows.Count(),
            TotalPagesCount = (((int)aggResult.TotalRows + pageSize - 1) / pageSize)
        };

        return currentPage;
    }

    public async Task<Page<EmployeeRead>> SearchAutocompleteAsync(ObjectId clientId, int page, int pageSize, string startWith)
    {
        if (string.IsNullOrEmpty(startWith)) throw new ArgumentNullException(nameof(startWith));

        var searchStage = new BsonDocument("$search",
                                new BsonDocument {
                                    { "index", "employeeSearchAutocomplete" },
                                    { "compound",
                                        new BsonDocument {
                                            { 
                                                "must",
                                                BuildSearchMustCondition(clientId) 
                                            },
                                            { 
                                                "should",
                                                BuildSearchShouldCondition(startWith, __AUTOCOMPLETE)
                                            },
                                            { "minimumShouldMatch", 1 }
                                        } 
                                    }   
                                }
                            );

        var searchPipeline = new BsonDocument[]
        {
                searchStage,
                BuildSearchSortStage(),
                BuildSearchProjectStage(),
                BuildSearchFacetStage(page, pageSize),
                BuildSearchSetStage()
        };

        var aggResult = await _employeeReadBsonCollection.Aggregate<PaginatedSearchResult<EmployeeRead>>(searchPipeline)
                                                         .FirstOrDefaultAsync();

        var currentPage = new Page<EmployeeRead>
        {
            TotalItemsCount = aggResult.TotalRows,
            Items = aggResult.Rows,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentPageSize = aggResult.Rows.Count(),
            TotalPagesCount = (((int)aggResult.TotalRows + pageSize - 1) / pageSize)
        };

        return currentPage;
    }
}