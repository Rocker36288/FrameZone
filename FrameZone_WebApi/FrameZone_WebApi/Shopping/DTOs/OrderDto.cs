namespace FrameZone_WebApi.Shopping.DTOs
{
    public class OrderDto
    {
        public List<OrderItem>? OrderItems { get; set; }
        public int TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "ALL";
        public string ReturnURL { get; set; } = string.Empty;
        public Dictionary<string, object>? OptionParams { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
    }
}
