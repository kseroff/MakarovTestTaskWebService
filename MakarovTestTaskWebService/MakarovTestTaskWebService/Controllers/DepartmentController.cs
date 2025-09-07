using MakarovTestTaskWebService.Core.Services;
using MakarovTestTaskWebService.Core.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MakarovTestTaskWebService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly ILogger<DepartmentService> _logger;
        private readonly IDepartmentManager _departmentManager;

        public DepartmentController(ILogger<DepartmentService> logger, IDepartmentManager departmentManager)
        {
            _logger = logger;
            _departmentManager = departmentManager;
        }

        [HttpPost]
        //[SwaggerOperation(Summary = "Добавить Отдел", Description = "Добавить Отдел")]
        public async Task<IActionResult> AddDepartment([FromBody] DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var departmentId = await _departmentManager.AddDepartment(model);

            return CreatedAtAction(
                nameof(GetDepartmentById), 
                new { id = departmentId }, 
                new { DepartmentId = departmentId }
                );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _departmentManager.GetDepartmentById(id);
            if (department == null)
                return NotFound(new { Message = "Отделы не найдены" });

            return Ok(department);
        }

        [HttpPut("{id}")]
        //[SwaggerOperation(Summary = "Обновить данные отдела", Description = "Обновить данные отдела")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _departmentManager.UpdateDepartment(id, model);

            if (!result)
                return NotFound(new { Message = "Отделы не найдены" });
            return NoContent();
        }

        [HttpGet("department/{departmentId}")]
        //[SwaggerOperation(Summary = "Получить список сотрудников по ID отдела", Description = "Получить список сотрудников по ID отдела")]
        public async Task<IActionResult> GetEmployeesByDepartment(int departmentId)
        {
            var employees = await _departmentManager.GetEmployesByDepartmentId(departmentId);
            return Ok(employees);
        }

    }

}
