using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using ProniaTask.DataAccesLayer;
using ProniaTask.Extensions;
using ProniaTask.Models;
using ProniaTask.ViewModels.Products;
using System.Text;
namespace ProniaTask.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController(ProniaContext _context, IWebHostEnvironment _env):Controller
    {
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products
                .Select(p => new GetProductAdminVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    CostPrice = p.CostPrice,
                    SellPrice = p.SellPrice,
                    StockCount = p.StockCount,
                    Raiting = p.Raiting,
                    Discount = p.Discount,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync());
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM data)
        {
            if (data.ImageFile != null)
            {
                if (!data.ImageFile.IsValidType("image"))
                    ModelState.AddModelError("ImageFile", "Fayl şəkil formatında olmalıdır.");
                if (data.ImageFile.IsValidLength(300))
                    ModelState.AddModelError("ImageFile", "Faylın ölçüsü 200 kb-dan çox olmamalıdır.");
                //if (!ModelState.IsValid)
                //    return View(data);
            }
            bool isImageValid = true;
            StringBuilder sb = new StringBuilder();
            foreach (var img in data.ImageFiles)
            {
                if (!img.IsValidType("image"))
                {
                    sb.Append("-" + img.FileName + "faylı şəkil formatında deyil.");
                    //ModelState.AddModelError("ImageFiles", img.FileName + "faylı şəkil formatında deyil.");
                    isImageValid = false;
                }

                if (!img.IsValidLength(300))
                {
                    sb.Append("-" + img.FileName + "faylın ölçüsü 200kb-dan çox olmamalıdır.");
                    //ModelState.AddModelError("ImageFiles", img.FileName + "faylın ölçüsü 200kb-dan çox olmamalıdır.");
                    isImageValid = false;
                }

            }
            if (!isImageValid)
            {
                sb.Insert(0, "<ol>");
                sb.Append("</ol>");
                ModelState.AddModelError("ImageFiles", sb.ToString());
            }
            if (!ModelState.IsValid)
            {
                return View(data);
            }



            string fileName = await data.ImageFile.SaveFileAsync(Path.Combine(_env.WebRootPath, "imgs", "products"));

            Product prod = new Product
            {
                Name = data.Name,
                CostPrice = data.CostPrice,
                CreateTime= DateTime.Now,
                SellPrice = data.SellPrice,
                StockCount = data.StockCount,
                Raiting = data.Raiting,
                Discount = data.Discount,
                ImageUrl = Path.Combine("imgs", "products", fileName),
                isDeleted = false,
                ProductImages = new List<ProductImage>()
            };
            foreach(var img in data.ImageFiles)
            {
                string imgName = await img.SaveFileAsync(Path.Combine(_env.WebRootPath, "imgs", "products"));
                prod.ProductImages.Add(new ProductImage
                {
                    ImageUrl = Path.Combine(_env.WebRootPath, "imgs", "products"),
                    CreateTime = DateTime.Now,
                    isDeleted = false,
                });
                ;
            }
            await _context.Products.AddAsync(prod);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
