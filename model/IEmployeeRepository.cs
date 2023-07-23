using MongoDB.Bson;

namespace model;

public interface IEmployeeRepository
{
   Task<Page<EmployeeRead>> GetByClientIdAsync(ObjectId clientId, int page, int pageSize, string? startWith = null);

   Task<Page<EmployeeRead>> SearchAsync(ObjectId clientId, int page, int pageSize, string? startWith = null);

   Task SetClientIdAsync();
}