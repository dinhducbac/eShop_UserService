using EmployeeManagerment.Entity;
using EmployeeManagerment.Models;
using Exercise2.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagerment.Respository.AppUserRepository
{
    public class AppUserRepository : GenericRepository<AppUser>, IAppUserRepository
    {
        private eShopDBContext _dbContext;
        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        public AppUserRepository(eShopDBContext context, UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager) : base(context)
        {
            _dbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> CreateAppUser(AppUser appUser, string passowrd)
        {
            return await _userManager.CreateAsync(appUser, passowrd);
        }

        public async Task<AppUser> FindByNameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public Task<List<UserModel>> GetModelAll()
        {
            return _dbContext.AppUsers.Select(user => new UserModel() { UserName = user.UserName, Email = user.Email })
                .ToListAsync();
        }

        public async Task<SignInResult> SignInAsync(AppUser appUser, string password, bool rememberme, bool lockWhenFailed)
        {
            return await _signInManager.PasswordSignInAsync(appUser, password,rememberme,lockWhenFailed);
        }
    }
}
