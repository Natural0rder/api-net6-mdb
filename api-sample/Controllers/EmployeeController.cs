﻿using api_sample.Controllers.DTO;
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

    [HttpGet("{clientId}/{page}")]
    public async Task<Page<EmployeeDto>> GetByClientIdAsync(string clientId, int page, string? startWith)
    {
        var oidClientId = ObjectId.Parse(clientId);
        var employeePage = await _employeeRepository.GetByClientIdAsync(oidClientId, page, startWith);

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