using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarSell.Models;
using CarSell.DataContext;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CarSell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly CarDbContext _context;

        public CarController(CarDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductDto>> GetProducts(int skip = 0, int limit = 30)
        {
            var products = _context.Products.Include(p => p.Images).Skip(skip).Take(limit).ToList();
            var total = _context.Products.Count();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                CashPrice = p.CashPrice,
                DiscountPercentage = p.DiscountPercentage,
                City = p.City,
                Mileage = p.Mileage,
                Brand = p.Brand,
                Category = p.Category,
                Thumbnail = p.Thumbnail,
                Images = p.Images.Select(i => i.Path).ToList()
            }).ToList();

            return Ok(new { Products = productDtos, Total = total, Skip = skip, Limit = limit });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                CashPrice = product.CashPrice,
                DiscountPercentage = product.DiscountPercentage,
                City = product.City,
                Mileage = product.Mileage,
                Brand = product.Brand,
                Category = product.Category,
                Thumbnail = product.Thumbnail,
                Images = product.Images.Select(i => i.Path).ToList()
            };

            return Ok(productDto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] ProductCreateDTO productDTO)
        {
            try
            {
                var product = new Product
                {
                    Title = productDTO.Title,
                    Description = productDTO.Description,
                    Price = productDTO.Price,
                    CashPrice = productDTO.CashPrice,
                    DiscountPercentage = productDTO.DiscountPercentage,
                    City = productDTO.City,
                    Mileage = productDTO.Mileage,
                    Brand = productDTO.Brand,
                    Category = productDTO.Category,
                    Thumbnail = productDTO.Thumbnail,
                    Images = new List<Image>() // Inisialisasi koleksi gambar untuk mencegah NullReferenceException
                };

                if (productDTO.Images != null && productDTO.Images.Count > 0)
                {
                    foreach (var formFile in productDTO.Images)
                    {
                        if (formFile.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(formFile.FileName);
                            var filePath = System.IO.Path.Combine("uploads", fileName);

                            // Simpan file gambar ke server
                            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                            {
                                await formFile.CopyToAsync(stream);
                            }

                            // Simpan path gambar ke basis data
                            var image = new Image { Path = filePath };
                            product.Images.Add(image);
                        }
                    }
                }

                // Tambahkan produk baru ke basis data
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price,
                    CashPrice = product.CashPrice,
                    DiscountPercentage = product.DiscountPercentage,
                    City = product.City,
                    Mileage = product.Mileage,
                    Brand = product.Brand,
                    Category = product.Category,
                    Thumbnail = product.Thumbnail,
                    Images = product.Images.Select(i => i.Path).ToList()
                };

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDTO productDTO)
        {
            if (id != productDTO.Id)
            {
                return BadRequest();
            }

            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Update properties of product
            product.Title = productDTO.Title;
            product.Description = productDTO.Description;
            product.Price = productDTO.Price;
            product.CashPrice = productDTO.CashPrice;
            product.DiscountPercentage = productDTO.DiscountPercentage;
            product.City = productDTO.City;
            product.Mileage = productDTO.Mileage;
            product.Brand = productDTO.Brand;
            product.Category = productDTO.Category;
            product.Thumbnail = productDTO.Thumbnail;

            // Handle images
            if (productDTO.Images != null && productDTO.Images.Count > 0)
            {
                // Remove existing images
                foreach (var existingImage in product.Images.ToList())
                {
                    _context.Images.Remove(existingImage);
                }

                // Add new images
                foreach (var formFile in productDTO.Images)
                {
                    if (formFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(formFile.FileName);
                        var filePath = System.IO.Path.Combine("uploads", fileName);

                        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }

                        // Save image path to database
                        var image = new Image { Path = filePath };
                        product.Images.Add(image);
                    }
                }
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                CashPrice = product.CashPrice,
                DiscountPercentage = product.DiscountPercentage,
                City = product.City,
                Mileage = product.Mileage,
                Brand = product.Brand,
                Category = product.Category,
                Thumbnail = product.Thumbnail,
                Images = product.Images.Select(i => i.Path).ToList()
            };

            return Ok(productDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Menghapus gambar dari folder uploads
            foreach (var image in product.Images.ToList())
            {
                var imagePath = Path.Combine("uploads", image.Path);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // Menghapus gambar dari basis data
                _context.Images.Remove(image);
            }

            // Menghapus produk dari basis data
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                CashPrice = product.CashPrice,
                DiscountPercentage = product.DiscountPercentage,
                City = product.City,
                Mileage = product.Mileage,
                Brand = product.Brand,
                Category = product.Category,
                Thumbnail = product.Thumbnail,
                Images = product.Images.Select(i => i.Path).ToList()
            };

            return Ok(productDto);
        }
    }

    // DTO untuk menampilkan data produk tanpa properti referensi
    public class ProductDto
    {
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
        public List<string> Images { get; set; }
    }

    // DTO untuk menerima data produk dari permintaan pengguna
    public class ProductCreateDTO
    {
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
        public List<IFormFile> Images { get; set; } // Menambah properti untuk menerima file gambar
    }

    // DTO untuk menerima data pembaruan produk dari permintaan pengguna
    public class ProductUpdateDTO : ProductCreateDTO
    {
        public int Id { get; set; }
    }
}
