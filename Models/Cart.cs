namespace SmartShop.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public decimal TotalPrice => CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
    }
}