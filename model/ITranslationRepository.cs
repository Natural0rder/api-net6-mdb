using MongoDB.Bson;

namespace model;

public interface ITranslationRepository
{
   Task<LabelDto> GeTranslationAsync(string id, string type, string lang);
}