using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

public class LabelDto
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("entityId")]
    public string EntityId { get; set; }

    [BsonElement("entityType"), BsonRepresentation(BsonType.String)]
    public string Type { get; set; }

    [BsonElement("translations")]
    public Dictionary<string, TranslationDto> Translations { get; set; } = new();

    [BsonElement("creaDate")]
    public DateTime? CreationDateUtc { get; set; }

    [BsonElement("updDate")]
    public DateTime? UpdateDateUtc { get; set; }
}



public class TranslationDto

{
    [BsonElement("val")]
    public string Value { get; set; }
    
    [BsonElement("additionalValues"), BsonIgnoreIfNull]
    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    public Dictionary<string, string> AdditionnalValues { get; set; }

}