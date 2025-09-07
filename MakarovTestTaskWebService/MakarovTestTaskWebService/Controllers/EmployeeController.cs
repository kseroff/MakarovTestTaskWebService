using MakarovTestTaskWebService.Core.Models.DataBase;
using MakarovTestTaskWebService.Core.Services;
using MakarovTestTaskWebService.Core.ViewModel;
using MakarovTestTaskWebService.Models;
using Microsoft.AspNetCore.Mvc;

namespace MakarovTestTaskWebService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger<EmployeeService> _logger;
        private readonly IEmployeeManager _employeeManager;
        private readonly ICustomEmployeeValidator<Employee> _employeeValidator; // не используется (для удобства), но может пригодиться

        public EmployeeController(
            ILogger<EmployeeService> logger, 
            IEmployeeManager employeeManager, 
            ICustomEmployeeValidator<Employee> employeeValidator)
        {
            _logger = logger;
            _employeeManager = employeeManager;
            _employeeValidator = employeeValidator;
        }

        [HttpPost]
        //[SwaggerOperation(Summary = "Добавить сотрудника", Description = "Добавить сотрудника")]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employeeId = await _employeeManager.AddEmployee(model);

            return CreatedAtAction(
                nameof(GetEmployeeById), 
                new { id = employeeId }, 
                new { EmployeeId = employeeId }
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _employeeManager.GetEmployeeById(id);
            if (employee == null)
                return NotFound(new { Message = "Сотрудник не найден" });
            return Ok(employee);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeManager.GetAllEmployees();
            return Ok(employees);
        }

        [HttpDelete("{id}")]
        //[SwaggerOperation(Summary = "Удалить сотрудника по Id", Description = "Удалить сотрудника по Id")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeManager.DeleteEmployee(id);
            if (!result)
                return NotFound(new { Message = "Сотрудник не найден" });
            return NoContent();
        }

        [HttpGet("company/{companyId}")]
        //[SwaggerOperation(Summary = "Получить список сотрудников по компании", Description = "Получить список сотрудников по компании")]
        public async Task<IActionResult> GetEmployeesByCompany(int companyId)
        {
            var employees = await _employeeManager.GetEmployesByCompany(companyId);
            return Ok(employees);
        }

        [HttpPut("{id}")]
        //[SwaggerOperation(Summary = "Обновить данные сотрудника", Description = "Обновить данные сотрудника")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] TestUpdateEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeManager.UpdateEmployee(id, model);

            if (!result)
                return NotFound(new { Message = "Сотрудники не найдены" });
            return NoContent();
        }

    }

}
