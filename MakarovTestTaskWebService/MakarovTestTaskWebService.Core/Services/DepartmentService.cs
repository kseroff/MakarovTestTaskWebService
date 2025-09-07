using Dapper;
using MakarovTestTaskWebService.Core.Models.DataBase;
using MakarovTestTaskWebService.Core.ViewModel;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading.Tasks;

namespace MakarovTestTaskWebService.Core.Services
{
    public class DepartmentService : IDepartmentManager
    {
        private readonly ILogger<DepartmentService> _logger;
        private readonly IDbConnection _dbConnection;

        public DepartmentService(ILogger<DepartmentService> logger, IDbConnection dbConnection)
        {
            _logger = logger;
            _dbConnection = dbConnection;
        }

        // Добавить отдел
        public async Task<int> AddDepartment(DepartmentViewModel department)
        {
            var departmentEntity = new Department
            {
                Name = department.Name,
                Phone = department.Phone
            };
            var result = await AddDepartmentToDatabase(departmentEntity);
            return result;
        }

        // Обновить отдел
        public async Task<bool> UpdateDepartment(int id, DepartmentViewModel department)
        {
            var departmentEntity = new Department
            {
                Id = id,
                Name = department.Name,
                Phone = department.Phone
            };
            var result = await UpdateDepartmentInDatabase(departmentEntity);
            return result;
        }

        private async Task<int> AddDepartmentToDatabase(Department department)
        {
            var query = @"
            INSERT INTO Department (Name, Phone)
            VALUES (@Name, @Phone)
            RETURNING Id;";
            var result = await _dbConnection.ExecuteScalarAsync<int>(query, department);
            return result;
        }

        private async Task<bool> UpdateDepartmentInDatabase(Department department)
        {
            var query = @"
            UPDATE Department
            SET Name = @Name, Phone = @Phone
            WHERE Id = @Id;";
            var result = await _dbConnection.ExecuteAsync(query, department);
            return result > 0;
        }

        // Удалить отдел
        public async Task<bool> DeleteDepartment(int id)
        {
            var query = "DELETE FROM Department WHERE Id = @Id";
            var result = await _dbConnection.ExecuteAsync(query, new { Id = id });
            return result > 0;
        }

        // Получить сотрудников по ID отдела
        public async Task<IEnumerable<Employee>> GetEmployesByDepartmentId(int departmentId)
        {
            var query = @"
            SELECT e.Id, e.Name, e.Surname, e.Phone, e.CompanyId, e.DepartmentId, e.PassportNumber, d.Name AS DepartmentName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.Id
            WHERE e.DepartmentId = @DepartmentId;";

            var employees = await _dbConnection.QueryAsync<Employee>(query, new { DepartmentId = departmentId });
            return employees;
        }

        public async Task<Department> GetDepartmentById(int departmentId)
        {
            var query = "SELECT * FROM Department WHERE Id = @Id";
            var department = await _dbConnection.QueryFirstOrDefaultAsync<Department>(query, new { Id = departmentId });
            return department;
        }
    }
}
