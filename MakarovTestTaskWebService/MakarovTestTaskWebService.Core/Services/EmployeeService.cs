using Dapper;
using MakarovTestTaskWebService.Core.Models.DataBase;
using MakarovTestTaskWebService.Core.ViewModel;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading.Tasks;

namespace MakarovTestTaskWebService.Core.Services
{

    public class EmployeeService : IEmployeeManager
    {

        private readonly ILogger<EmployeeService> _logger;
        private readonly IDbConnection _dbConnection;

        public EmployeeService(ILogger<EmployeeService> logger, IDbConnection dbConnection)
        {
            _logger = logger;
            _dbConnection = dbConnection;
        }

        // Добавить сотрудника и вернуть Id
        public async Task<int> AddEmployee(EmployeeViewModel model)
        {
            if (_dbConnection.State != ConnectionState.Open)
                await (_dbConnection as Npgsql.NpgsqlConnection)!.OpenAsync();

            await using var tran = await (_dbConnection as Npgsql.NpgsqlConnection)!.BeginTransactionAsync();
            try
            {
                bool hasPassNum = !string.IsNullOrWhiteSpace(model.PassportNumber);
                bool hasPassType = !string.IsNullOrWhiteSpace(model.PassportType);
                if (hasPassNum ^ hasPassType)
                    throw new InvalidOperationException("Для паспорта нужно указать и Number, и Type, либо ничего.");

                Department? department = null;
                if (model.DepartmentId.HasValue && model.DepartmentId.Value > 0)
                    department = await GetDepartmentByIdAsync(model.DepartmentId, tran);

                Passport? passport = null;
                if (hasPassNum && hasPassType)
                {
                    passport = new Passport { 
                        Number = model.PassportNumber!, 
                        Type = model.PassportType! 
                    };
                    await CreatePassportAsync(passport, tran);
                }

                var employee = new Employee
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Phone = model.Phone,
                    CompanyId = model.CompanyId,
                    Department = department,
                    Passport = passport
                };

                var employeeId = await AddEmployeeToDatabaseAsync(employee, tran);

                await tran.CommitAsync();
                return employeeId;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        // обнвить сотрудника
        public async Task<bool> UpdateEmployee(int id, TestUpdateEmployeeViewModel model)
        {
            if (_dbConnection.State != ConnectionState.Open)
                await (_dbConnection as Npgsql.NpgsqlConnection)!.OpenAsync();

            await using var tran = await (_dbConnection as Npgsql.NpgsqlConnection)!.BeginTransactionAsync();
            try
            {
                // Текущие данные
                var current = await GetEmployeeByIdAsync(id, tran);
                if (current is null)
                {
                    await tran.RollbackAsync();
                    return false;
                }

                var newName = string.IsNullOrWhiteSpace(model.Name) ? current.Name : model.Name;
                var newSurname = string.IsNullOrWhiteSpace(model.Surname) ? current.Surname : model.Surname;
                var newPhone = string.IsNullOrWhiteSpace(model.Phone) ? current.Phone : model.Phone;
                var newCompanyId = (model.CompanyId.HasValue && model.CompanyId.Value > 0) ? model.CompanyId.Value : current.CompanyId;

                // Отдел
                var currentDeptId = current.Department?.Id ?? 0;
                var targetDeptId = (model.DepartmentId.HasValue && model.DepartmentId.Value > 0) ? model.DepartmentId.Value : currentDeptId;

                Department? newDepartment = current.Department;
                if (targetDeptId != currentDeptId)
                {
                    newDepartment = await GetDepartmentByIdAsync(targetDeptId, tran);
                    if (newDepartment is null)
                        throw new InvalidOperationException($"Отдел с Id={targetDeptId} не найден.");
                }

                // Паспорт
                var curPassNum = current.Passport?.Number;
                var curPassType = current.Passport?.Type;

                var newPassNum = string.IsNullOrWhiteSpace(model.PassportNumber) ? curPassNum : model.PassportNumber;
                var newPassType = string.IsNullOrWhiteSpace(model.PassportType) ? curPassType : model.PassportType;

                bool numberChanged = !string.Equals(newPassNum, curPassNum, StringComparison.Ordinal);
                bool typeChanged = !string.Equals(newPassType, curPassType, StringComparison.Ordinal);

                // Если номер меняется
                if (numberChanged)
                {
                    if (string.IsNullOrWhiteSpace(newPassNum))
                        throw new InvalidOperationException("Новый номер паспорта пустой.");

                    await CreatePassportAsync(new Passport { 
                        Number = newPassNum!, 
                        Type = newPassType ?? curPassType ?? "" 
                    },tran);
                }
                else if (typeChanged && !string.IsNullOrWhiteSpace(newPassNum))
                    await UpdatePassportAsync(new Passport
                    {
                        Number = newPassNum!,
                        Type = newPassType!
                    }, tran);

                // обновление сотрудника
                var updated = new Employee
                {
                    Id = id,
                    Name = newName!,
                    Surname = newSurname!,
                    Phone = newPhone!,
                    CompanyId = newCompanyId,
                    Department = newDepartment ?? current.Department,
                    Passport = new Passport { 
                        Number = newPassNum ?? curPassNum!,
                        Type = newPassType ?? curPassType }
                };

                var ok = await UpdateEmployeeInDatabaseAsync(updated, tran);
                if (!ok)
                {
                    await tran.RollbackAsync();
                    return false;
                }

                // Если номер меняли то удаляем старый паспорт
                if (numberChanged && !string.IsNullOrWhiteSpace(curPassNum))
                {
                    await DeletePassportAsync(curPassNum!, tran);
                }

                await tran.CommitAsync();
                return true;

            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        private async Task<int> AddEmployeeToDatabaseAsync(Employee employee, IDbTransaction? tran = null)
        {
            const string sql = @"
                INSERT INTO Employee (Name, Surname, Phone, CompanyId, DepartmentId, PassportNumber)
                VALUES (@Name, @Surname, @Phone, @CompanyId, @DepartmentId, @PassportNumber)
                RETURNING Id;";

            return await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                employee.Name,
                employee.Surname,
                employee.Phone,
                employee.CompanyId,
                DepartmentId = employee.Department?.Id,
                PassportNumber = employee.Passport?.Number
            }, transaction: tran);

            // Note:
            // Вместо такой реализации, где мы раскладываем класс Employee
            // я бы использовал в дальнейшем DTO/DAO классы, который бы имел уже нужную структуру для взаимодействия с БД
            // т.к. Dapper не умеет работать с вложенными объектами
            // а добавлять в класс Employee доп новые плоские поля (под SQL)
            // и использовать навигационные/агрегатные как сейчас - не очень хорошая практика из-за дублирования
            // P.S. Возможно есть решение лучше или в самом Dapper, но я его не нашел
        }

        private async Task<bool> UpdateEmployeeInDatabaseAsync(Employee e, IDbTransaction tran)
        {
            const string sql = @"
                UPDATE Employee
                SET Name=@Name, Surname=@Surname, Phone=@Phone,
                    CompanyId=@CompanyId, DepartmentId=@DepartmentId, PassportNumber=@PassportNumber
                WHERE Id=@Id;";

            var affected = await _dbConnection.ExecuteAsync(sql, new
            {
                e.Id,
                e.Name,
                e.Surname,
                e.Phone,
                e.CompanyId,
                DepartmentId = e.Department?.Id,
                PassportNumber = e.Passport?.Number
            }, transaction: tran);

            return affected > 0;
        }

        private async Task<Department?> GetDepartmentByIdAsync(int? departmentId, IDbTransaction? tran = null)
        {
            if (!departmentId.HasValue || departmentId.Value <= 0)
                return null;

            const string sql = "SELECT Id, Name, Phone FROM Department WHERE Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<Department>(sql, new { Id = departmentId }, transaction: tran);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id, IDbTransaction? tran = null)
        {
            const string sql = @"
                SELECT
                    e.Id, e.Name, e.Surname, e.Phone, e.CompanyId, e.DepartmentId, e.PassportNumber,
                    d.Id, d.Name, d.Phone,
                    p.Type, p.Number
                FROM Employee e
                LEFT JOIN Department d ON e.DepartmentId = d.Id
                LEFT JOIN Passport   p ON e.PassportNumber = p.Number
                WHERE e.Id = @Id;";

            var rows = await _dbConnection.QueryAsync<Employee, Department, Passport, Employee>(
                sql,
                (emp, dept, pass) => { emp.Department = dept; emp.Passport = pass; return emp; },
                new { Id = id },
                transaction: tran,
                splitOn: "Id,Type"
            );

            return rows.FirstOrDefault();
        }

        // Удалить сотрудника по Id -- меньше привязок к БД, но больше запросов
        /*public async Task<bool> DeleteEmployee(int id)
        {
            // Открываем коннект вручную для транзакции
            var npg = (Npgsql.NpgsqlConnection)_dbConnection;
            if (npg.State != ConnectionState.Open)
                await npg.OpenAsync();

            await using var tran = await npg.BeginTransactionAsync();
            try
            {
                var employee = await GetEmployeeByIdAsync(id, tran);
                if (employee is null)
                {
                    await tran.RollbackAsync();
                    return false;
                }

                var passportNumber = employee.Passport?.Number;

                var sql = "DELETE FROM Employee WHERE Id = @Id";
                var deleted = await _dbConnection.ExecuteAsync(sql, new { Id = id }, transaction: tran);
                if (deleted > 0)
                {
                    await tran.RollbackAsync();
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(passportNumber))
                    await DeletePassportAsync(passportNumber!, tran);

                await tran.CommitAsync();
                return true;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }*/

        // Удалить сотрудника по Id -- оптимизированный вариант с CTE
        public async Task<bool> DeleteEmployee(int id)
        {
            // Готовим транзакцию (нужно открытое соединение)
            var npg = (Npgsql.NpgsqlConnection)_dbConnection;
            if (npg.State != ConnectionState.Open)
                await npg.OpenAsync();

            await using var tran = await npg.BeginTransactionAsync();

            try
            {
                const string sql = @"
                    WITH del_emp AS (
                        DELETE FROM Employee
                        WHERE Id = @Id
                        RETURNING PassportNumber
                    ),
                    del_pass AS (
                        DELETE FROM Passport p
                        USING del_emp
                        WHERE p.Number = del_emp.PassportNumber
                        RETURNING 1
                    )
                    SELECT (SELECT COUNT(*) FROM del_emp) AS employees_deleted;";

                var employeesDeleted = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }, transaction: tran);

                if (employeesDeleted == 0)
                {
                    await tran.RollbackAsync();
                    return false; // сотрудник с таким Id не найден
                }

                await tran.CommitAsync();
                return true;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        // Получить список сотрудников по Id компании
        public async Task<IEnumerable<Employee>> GetEmployesByCompany(int companyId)
        {
            const string sql = @"
                SELECT 
                    e.Id, e.Name, e.Surname, e.Phone, e.CompanyId, e.DepartmentId, e.PassportNumber,
                    d.Id, d.Name, d.Phone,
                    p.Type, p.Number
                FROM Employee e
                LEFT JOIN Department d ON e.DepartmentId = d.Id
                LEFT JOIN Passport   p ON e.PassportNumber = p.Number
                WHERE e.CompanyId = @CompanyId;";

            var result = await _dbConnection.QueryAsync<Employee, Department, Passport, Employee>(
                sql,
                (emp, dept, pass) => { emp.Department = dept; emp.Passport = pass; return emp; },
                new { CompanyId = companyId },
                splitOn: "Id,Type"
            );
            return result;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployees()
        {
            const string sql = @"
                SELECT
                    e.Id, e.Name, e.Surname, e.Phone, e.CompanyId, e.DepartmentId, e.PassportNumber,
                    d.Id, d.Name, d.Phone,
                    p.Type, p.Number
                FROM Employee e
                LEFT JOIN Department d ON e.DepartmentId = d.Id
                LEFT JOIN Passport   p ON e.PassportNumber = p.Number
                ORDER BY e.Id;";

            var rows = await _dbConnection.QueryAsync<Employee, Department, Passport, Employee>(
                sql,
                (emp, dept, pass) =>
                {
                    emp.Department = dept;
                    emp.Passport = pass;
                    return emp;
                },
                splitOn: "Id,Type"
            );

            return rows;
        }

        private async Task<bool> CreatePassportAsync(Passport passport, IDbTransaction? tran = null)
        {
            if (passport is null)
                return false;

            const string sql = @"
                INSERT INTO Passport (Type, Number)
                VALUES (@Type, @Number) 
                ON CONFLICT (Number) DO NOTHING;";

            var affected = await _dbConnection.ExecuteAsync(sql, passport, transaction: tran);
            return affected > 0;
        }

        private async Task<bool> UpdatePassportAsync(Passport passport, IDbTransaction? tran = null)
        {
            const string sql = @"
                UPDATE Passport 
                SET Type = @Type 
                WHERE Number = @Number;";
            var affected = await _dbConnection.ExecuteAsync(sql, passport, transaction: tran);
            return affected > 0;
        }

        private async Task<bool> DeletePassportAsync(string number, IDbTransaction? tran = null)
        {
            const string sql = @"DELETE FROM Passport p WHERE p.Number = @Number;";

            var affected = await _dbConnection.ExecuteAsync(sql, new { Number = number }, transaction: tran);
            return affected > 0;
        }

        public async Task<Employee?> GetEmployeeById(int id)
        {
            var employee = await GetEmployeeByIdAsync(id);
            return employee;
        }
    }
}