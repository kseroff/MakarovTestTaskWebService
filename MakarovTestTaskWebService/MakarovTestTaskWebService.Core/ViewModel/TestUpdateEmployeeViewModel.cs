using MakarovTestTaskWebService.Core.Models.DataBase;
using System.ComponentModel.DataAnnotations;

namespace MakarovTestTaskWebService.Core.ViewModel
{
    // Упрощенная модель для обновления сотрудника (без валидации)
    // Чтобы была возможнеость послать не все поля для обновления
    public class TestUpdateEmployeeViewModel
    {
        // Сотрудник
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
        public int? CompanyId { get; set; }

        // Отдел
        public int? DepartmentId { get; set; }

        // Паспорт
        public string? PassportType { get; set; }
        public string? PassportNumber { get; set; }
    }
}