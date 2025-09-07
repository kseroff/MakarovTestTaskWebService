using MakarovTestTaskWebService.Controllers;
using MakarovTestTaskWebService.Core.Models.DataBase;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace MakarovTestTaskWebService.Models
{
    public class CustomEmployeeValidator : ICustomEmployeeValidator<Employee>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<Employee> manager, Employee employee)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                errors.Add(new IdentityError
                {
                    Code = "NameRequired",
                    Description = "Имя сотрудника обязательно для заполнения"
                });
            }

            if (string.IsNullOrWhiteSpace(employee.Surname))
            {
                errors.Add(new IdentityError
                {
                    Code = "SurnameRequired",
                    Description = "Фамилия сотрудника обязательна для заполнения"
                });
            }

            if (!string.IsNullOrWhiteSpace(employee.Phone))
            {
                var phoneRegex = new Regex(@"^\+?[0-9]{10,15}$");
                if (!phoneRegex.IsMatch(employee.Phone))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "InvalidPhoneFormat",
                        Description = "Неверный формат номера телефона"
                    });
                }
            }

            if (employee.CompanyId <= 0)
            {
                errors.Add(new IdentityError
                {
                    Code = "InvalidCompany",
                    Description = "Компания должна быть указана корректно"
                });
            }

            return Task.FromResult(errors.Count == 0 ?
                IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
        }
    }
}
