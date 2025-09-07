using Microsoft.AspNetCore.Identity;

namespace MakarovTestTaskWebService.Models
{
    public interface ICustomPasswordValidator<T> where T : class
    {
        Task<IdentityResult> ValidateAsync(UserManager<T> manager, T user, string password);
    };

    public interface ICustomEmployeeValidator<TUser> where TUser : class
    {
        Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user);
    };
}