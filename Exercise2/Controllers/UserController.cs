using EmployeeManagerment.Models;
using EmployeeManagerment.Services;
using eShopUserService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagerment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IUserService UserService;
        public readonly ILogger Logger;
        public readonly IConfiguration Configuration;

        public UserController(IUserService userService, ILogger<UserController> logger, IConfiguration configuration)
        {
            UserService = userService;
            Logger = logger;
            Configuration = configuration;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await UserService.Login(request);
            if (result.Success == false)
                return BadRequest(result.Message); 
            Console.WriteLine(result.ResultObject.Token);
            return Ok(result.ResultObject);
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await UserService.Register(request);
            if (!result.Success)
                return BadRequest(result.ResultObject);
            return Ok(result.ResultObject);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await UserService.GetAll();
            return Ok(result);
        }
        [HttpGet("getlistproduct")]
        public async Task<IActionResult> GetListProduct(int pageIndex, int pageSize)
        {
            var result = await UserService.GetListProduct(pageIndex, pageSize);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(int productId, int amount)
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            var result = await UserService.CreateOrder(token, productId, amount);
            return Ok(result);
        }
        [Authorize]
        [HttpGet("get-order")]
        public async Task<IActionResult> GetOrder()
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            var result = await UserService.GetOrder(token);
            return Ok(result);
        }
        [Authorize]
        [HttpDelete("delete-order")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            string token1 = await HttpContext.GetTokenAsync("access_token");
            var result = await UserService.DeleteOrder(orderId);
            return Ok(result);
        }
    }

}
