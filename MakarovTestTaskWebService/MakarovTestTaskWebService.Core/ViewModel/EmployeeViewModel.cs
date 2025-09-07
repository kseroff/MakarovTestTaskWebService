using MakarovTestTaskWebService.Core.Models.DataBase;
using System.ComponentModel.DataAnnotations;
 
namespace MakarovTestTaskWebService.Core.ViewModel
{
    public class EmployeeViewModel
    {
        // Сотрудник
        [Required(ErrorMessage = "Не указано Имя")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указана Фамилия")]
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Не указан Телефон")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Не указано ID компании")]
        [Display(Name = "ID компании")]
        public int CompanyId { get; set; }

        // Отдел
        [Required(ErrorMessage = "Не выбран отдел")]
        [Display(Name = "Отдел")]
        public int DepartmentId { get; set; }

        // Паспорт

        [Required(ErrorMessage = "Не указан Тип Паспорта")]
        [Display(Name = "Тип Паспорта")]
        public string PassportType { get; set; }

        [Required(ErrorMessage = "Не указан Номер Паспорта")]
        [Display(Name = "Номер Паспорта")]
        public string PassportNumber { get; set; }

    }
}