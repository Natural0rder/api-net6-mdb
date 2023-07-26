using api_sample.Controllers.DTO;
using Microsoft.AspNetCore.Mvc;
using model;
using MongoDB.Bson;

namespace api_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeController(IEmployeeRepository employeeRepository) =>
        _employeeRepository = employeeRepository;


    [HttpGet("generate")]
    public async Task<bool> SetClientIdAsync()
    {
        await _employeeRepository.SetClientIdAsync();
        return true;
    }

    [HttpGet("search/{clientId}/{pageSize}/{page}")]
    public async Task<Page<EmployeeDto>> SearchAsync(string clientId, int pageSize, int page, string startWith)
    {
        var oidClientId = ObjectId.Parse(clientId);
        var employeePage = await _employeeRepository.SearchAsync(oidClientId, page, pageSize, startWith);
        var dto = new Page<EmployeeDto> 
        {
            PageSize = employeePage.PageSize,
            TotalPagesCount = employeePage.TotalPagesCount,
            TotalItemsCount = employeePage.TotalItemsCount,
            CurrentPage = employeePage.CurrentPage,
            CurrentPageSize = employeePage.CurrentPageSize,
            Items = employeePage.Items.Select(x => new EmployeeDto {
                Id = x.Id.ToString(),
                LastName = x.LastName,
                FirstName = x.FirstName,
                Email = x.Email
            })
        };

        return dto;
    }

     [HttpGet("search-autocomplete/{clientId}/{pageSize}/{page}")]
    public async Task<Page<EmployeeDto>> SearchAutocompleteAsync(string clientId, int pageSize, int page, string startWith)
    {
        var oidClientId = ObjectId.Parse(clientId);
        var employeePage = await _employeeRepository.SearchAutocompleteAsync(oidClientId, page, pageSize, startWith);
        var dto = new Page<EmployeeDto> 
        {
            PageSize = employeePage.PageSize,
            TotalPagesCount = employeePage.TotalPagesCount,
            TotalItemsCount = employeePage.TotalItemsCount,
            CurrentPage = employeePage.CurrentPage,
            CurrentPageSize = employeePage.CurrentPageSize,
            Items = employeePage.Items.Select(x => new EmployeeDto {
                Id = x.Id.ToString(),
                LastName = x.LastName,
                FirstName = x.FirstName,
                Email = x.Email
            })
        };

        return dto;
    }

    [HttpGet("{clientId}/{pageSize}/{page}")]
    public async Task<Page<EmployeeDto>> GetByClientIdAsync(string clientId, int pageSize, int page, string? startWith)
    {
        var oidClientId = ObjectId.Parse(clientId);
        var employeePage = await _employeeRepository.GetByClientIdAsync(oidClientId, page, pageSize, startWith);

        var dto = new Page<EmployeeDto> 
        {
            PageSize = employeePage.PageSize,
            TotalPagesCount = employeePage.TotalPagesCount,
            TotalItemsCount = employeePage.TotalItemsCount,
            CurrentPage = employeePage.CurrentPage,
            CurrentPageSize = employeePage.CurrentPageSize,
            Items = employeePage.Items.Select(x => new EmployeeDto {
                Id = x.Id.ToString(),
                LastName = x.LastName,
                FirstName = x.FirstName,
                Email = x.Email
            })
        };
    
        return dto;
    }
}