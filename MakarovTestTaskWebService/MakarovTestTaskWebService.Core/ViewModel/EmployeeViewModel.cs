using MakarovTestTaskWebService.Core.Models.DataBase;
using System.ComponentModel.DataAnnotations;
 
namespace MakarovTestTaskWebService.Core.ViewModel
{
    public class EmployeeViewModel
    {
        // ���������
        [Required(ErrorMessage = "�� ������� ���")]
        [Display(Name = "���")]
        public string Name { get; set; }

        [Required(ErrorMessage = "�� ������� �������")]
        [Display(Name = "�������")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "�� ������ �������")]
        [Display(Name = "�������")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "�� ������� ID ��������")]
        [Display(Name = "ID ��������")]
        public int CompanyId { get; set; }

        // �����
        [Required(ErrorMessage = "�� ������ �����")]
        [Display(Name = "�����")]
        public int DepartmentId { get; set; }

        // �������

        [Required(ErrorMessage = "�� ������ ��� ��������")]
        [Display(Name = "��� ��������")]
        public string PassportType { get; set; }

        [Required(ErrorMessage = "�� ������ ����� ��������")]
        [Display(Name = "����� ��������")]
        public string PassportNumber { get; set; }

    }
}