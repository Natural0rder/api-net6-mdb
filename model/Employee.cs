using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace model
{
    public class Page<T>
    {
        public long TotalItemsCount { get; set;}

        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        public int CurrentPageSize { get; set; }

        public int TotalPagesCount { get; set; }

        public IEnumerable<T> Items { get; set; }
    }

    public class EmployeeRead
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("birthDate")]
        public DateTime BirthDate { get; set; }

        [BsonElement("clientId")]
        public ObjectId ClientId { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("firstName")]
        public string? FirstName { get; set; }

        [BsonElement("lastName")]
        public string? LastName { get; set;}
    }


    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("birthDate")]
        public DateTime BirthDate { get; set; }

        [BsonElement("clientId")]
        public ObjectId ClientId { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set;}

        [BsonElement("userId")]
        public Object UserId { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }
    }
}

