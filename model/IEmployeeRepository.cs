using MongoDB.Bson;

namespace model;

public interface IEmployeeRepository
{
   Task<Page<EmployeeRead>> GetByClientIdAsync(ObjectId clientId, int page = 1, string? startWith = null);

   Task SetClientIdAsync();
}