using EmployeeManagerment.Entity;
using EmployeeManagerment.Models;
using EmployeeManagerment.Respository.AppUserRepository;
using eShopUserService.Models;
using Exercise2.EF;
using Exercise2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace EmployeeManagerment.Services
{
    public class UserService : IUserService
    {
        public readonly UserManager<AppUser> UserManager;
        public readonly SignInManager<AppUser> SignInManager;
        public readonly IConfiguration Configuration;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        public UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
            , IConfiguration configuration, IAppUserRepository appUserRepository,
            IHttpClientFactory httpClientFactory)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            Configuration = configuration;
            _appUserRepository = appUserRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<APIResult<OrderModel>> CreateOrder(string token, int productId, int Amount)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            var id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
            var createRequest = new OrderCreateRequest() 
            { 
                UserId = int.Parse(id),
                ProductId = productId,
                Amount = Amount
            };
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Configuration["OrderAddress"]);
            var json = JsonConvert.SerializeObject(createRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/Order/create",httpContent);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<APIResult<OrderModel>>(body);
        }

        public async Task<bool> DeleteOrder(int orderId)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Configuration["OrderAddress"]);
            var response = await client.DeleteAsync($"/api/Order/delete?id={orderId}");
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<bool>(body);
        }

        public async Task<APIResult<List<UserModel>>> GetAll()
        {
            var user = await _appUserRepository.GetModelAll();
            return new APIResult<List<UserModel>>() { Success = true, Message ="Successful", ResultObject = user};
        }

        public async Task<APIResult<List<ProductModel>>> GetListProduct(int pageIndex, int pageSize)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Configuration["ProductAddress"]);
            var response = await client.GetAsync($"/api/Product/products?pageIndex={pageIndex}&pageSize={pageSize}");
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<APIResult<List<ProductModel>>>(body);
        }

        public async Task<APIResult<List<OrderModel>>> GetOrder(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            var id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
            var check = this.ValidateToken(token);
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Configuration["OrderAddress"]);
            var response = await client.GetAsync($"/api/Order/get-order?userId={id}");
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<APIResult<List<OrderModel>>>(body);
        }

        public async Task<APIResult<UserModel>> Login(LoginRequest request)
        {
            var user = await _appUserRepository.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return new APIResult<UserModel>() { Success = false, Message = "Username or password invalid!" };
            }
            var result = await _appUserRepository.SignInAsync(user, request.Password,
                request.RememberMe == null ? false : (bool)request.RememberMe, true);
            if (!result.Succeeded)
            {
                return new APIResult<UserModel>() { Success = false, Message = "Username or password invalid!" }; ;
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim("UserId",user.Id.ToString()),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(Configuration["Token:Issuer"],
                Configuration["Token:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );


            return new APIResult<UserModel>()
            {
                Success = true,
                Message = "Login successful!",
                ResultObject = new UserModel()
                {
                    UserName = request.UserName,
                    Email = user.Email,
                    Token = new JwtSecurityTokenHandler().WriteToken(token)
                }
            };

        }
        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;
            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();
            validationParameters.ValidateLifetime = true;

            validationParameters.ValidAudience = Configuration["Token:Issuer"];
            validationParameters.ValidIssuer = Configuration["Token:Issuer"];
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:Key"]));
            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);
            return principal;
        }

        public async Task<APIResult<string>> Register(RegisterRequest request)
        {
            var checkUser = await _appUserRepository.FindByNameAsync(request.UserName);
            if(checkUser != null)
                return new APIResult<string>() { Success = false, Message = "Register failed!", ResultObject = $"Already has username '{request.UserName}'" };
            var user = new AppUser()
            {
                UserName = request.UserName,
                Email = request.Email
            };
            var result = await _appUserRepository.CreateAppUser(user,request.Password);
            if (!result.Succeeded)
                return new APIResult<string>() { Success = false, Message = "Register failed!", ResultObject = result.ToString() };
            return new APIResult<string>() { Success = true, Message = "Register successful!", ResultObject = "Register successful!" };
        }
    }
}
