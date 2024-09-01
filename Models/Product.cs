using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarSell.Models
{
    [Table("Products")] // Menyesuaikan nama tabel
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal CashPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string City { get; set; }
        public int Mileage { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string Thumbnail { get; set; }

        // Menambahkan relasi dengan Image
        public ICollection<Image> Images { get; set; }
    }

    [Table("Images")] // Menyesuaikan nama tabel
    public class Image
    {
        [Key]
        public int Id { get; set; }
        public string Path { get; set; }

        [ForeignKey("Product")] // Menambahkan kunci asing
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
