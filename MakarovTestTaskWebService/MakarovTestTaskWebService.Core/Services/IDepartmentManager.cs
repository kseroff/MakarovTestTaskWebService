using MakarovTestTaskWebService.Core.Models.DataBase;
using MakarovTestTaskWebService.Core.ViewModel;

namespace MakarovTestTaskWebService.Core.Services
{
    public interface IDepartmentManager
    {
        Task<int> AddDepartment(DepartmentViewModel department);
        Task<bool> UpdateDepartment(int id, DepartmentViewModel department);
        Task<bool> DeleteDepartment(int id);
        Task<Department> GetDepartmentById(int departmentId);
        Task<IEnumerable<Employee>> GetEmployesByDepartmentId(int departmentId);
    }
}
