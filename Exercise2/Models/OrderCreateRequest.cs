namespace eShopUserService.Models
{
    public class OrderCreateRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Amount { get; set; }
    }
}
