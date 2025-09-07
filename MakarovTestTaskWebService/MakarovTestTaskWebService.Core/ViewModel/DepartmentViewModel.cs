using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakarovTestTaskWebService.Core.ViewModel
{
    public class DepartmentViewModel
    {
        [Required(ErrorMessage = "Не указано Имя Отдела")]
        [Display(Name = "Имя Отдела")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указан Номер Отдела")]
        [Display(Name = "Номер Отдела")]
        public string Phone { get; set; }
    }
}
