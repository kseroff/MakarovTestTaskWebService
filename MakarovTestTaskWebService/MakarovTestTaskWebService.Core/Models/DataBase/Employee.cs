namespace MakarovTestTaskWebService.Core.Models.DataBase
{
    public class Employee
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
        public int? CompanyId { get; set; }
        public Passport? Passport { get; set; }
        public Department? Department { get; set; } // Предпологаю что сотрудник может быть только в одном отделе
    }
}
