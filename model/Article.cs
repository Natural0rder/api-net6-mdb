using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace model
{
    public class Article
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string ArticleName { get; set; } = null!;

        public decimal Price { get; set; }

        public string Category { get; set; } = null!;
    }
}