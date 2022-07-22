using EmployeeManagerment.Models;
using eShopUserService.Models;
using Exercise2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagerment.Services
{
    public interface IUserService
    {
        public Task<APIResult<List<UserModel>>> GetAll();
        public Task<APIResult<UserModel>> Login(LoginRequest request);
        public Task<APIResult<string>> Register(RegisterRequest request);
        public Task<APIResult<List<ProductModel>>> GetListProduct(int pageIndex, int pageSize);
        public Task<APIResult<OrderModel>> CreateOrder(string token, int productId, int Amount);
        public Task<APIResult<List<OrderModel>>> GetOrder(string token);
        public Task<bool> DeleteOrder(int orderId);
    }
}
