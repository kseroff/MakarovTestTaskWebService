using MakarovTestTaskWebService.Core.Models.DataBase;
using MakarovTestTaskWebService.Core.ViewModel;

namespace MakarovTestTaskWebService.Core.Services
{
    public interface IEmployeeManager
    {
        Task<int> AddEmployee(EmployeeViewModel employee);
        Task<bool> DeleteEmployee(int id);
        Task<IEnumerable<Employee>> GetEmployesByCompany(int companyId);
        Task<bool> UpdateEmployee(int id, TestUpdateEmployeeViewModel model);
        Task<Employee?> GetEmployeeById(int id);
        Task<IEnumerable<Employee>> GetAllEmployees();
    }
}