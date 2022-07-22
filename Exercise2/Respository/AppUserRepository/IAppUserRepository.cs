using EmployeeManagerment.Entity;
using EmployeeManagerment.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagerment.Respository.AppUserRepository
{
    public interface IAppUserRepository : IGenericRepository<AppUser>
    {
        public Task<List<UserModel>> GetModelAll();
        public Task<AppUser> FindByNameAsync(string username);
        public Task<IdentityResult> CreateAppUser(AppUser appUser, string passowrd);
        public Task<SignInResult> SignInAsync(AppUser appUser, string password, bool rememberme, bool lockWhenFailed);
    }
}
