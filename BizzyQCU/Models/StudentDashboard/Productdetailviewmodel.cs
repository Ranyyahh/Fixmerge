namespace BizzyQCU.Models
{
    public class ProductDetailViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int PreparationTime { get; set; }
        public byte[] ProductImage { get; set; }

        // Enterprise
        public int EnterpriseId { get; set; }
        public string EnterpriseName { get; set; }
        public byte[] EnterpriseLogo { get; set; }

        // Delivery options joined for display e.g. "Pickup or Room to Room"
        public string DeliveryOptions { get; set; }
    }
}