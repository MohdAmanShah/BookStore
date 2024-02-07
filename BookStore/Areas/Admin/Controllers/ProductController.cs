using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using DataModels;
using DataModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticData.Admin_Role)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductViewModel model = new ProductViewModel();
            model.Categories = unitOfWork.CategoryRepository.GetAll();
            if (id == null || id == 0)
            {
                model.Product = new ProductModel();
                model.Product.ImageUrl = ImagePlaceHolders.ProductImage;
                return View(model);
            }
            model.Product = unitOfWork.ProductRepository.Get(u => u.ProductId == id);

            if (model.Product == null)
            {
                TempData["error"] = "Could not find the data";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel model, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (model.Product.ProductId == 0)
                {
                    unitOfWork.ProductRepository.Add(model.Product);
                    TempData["success"] = "Data Added successfully";
                }
                else
                {

                    unitOfWork.ProductRepository.Update(model.Product);
                    TempData["success"] = "Data Updated successfully";
                }
                unitOfWork.Commit();
                String WebRootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    String FilePath = ImagePlaceHolders.ProductImagePath + model.Product.ProductId.ToString();
                    String FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    String finalPath = Path.Combine(WebRootPath, FilePath);
                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }
                    if (!String.IsNullOrEmpty(model.Product.ImageUrl) && model.Product.ImageUrl != ImagePlaceHolders.ProductImage)
                    {
                        String temp = Path.Combine(WebRootPath, model.Product.ImageUrl);
                        if (System.IO.File.Exists(temp))
                        {
                            System.IO.File.Delete(temp);
                        }
                    }
                    using (var _file = new FileStream(Path.Combine(finalPath, FileName), FileMode.Create))
                    {
                        file.CopyTo(_file);
                    }
                    model.Product.ImageUrl = @"\" + Path.Combine(FilePath, FileName);

                }
                else
                {
                    model.Product.ImageUrl = ImagePlaceHolders.ProductImage;
                }

                unitOfWork.ProductImagesRepository.Add(new ProductImagesModel
                {
                    ProductId = model.Product.ProductId,
                    Url = model.Product.ImageUrl,
                });
                unitOfWork.ProductRepository.Update(model.Product);
                unitOfWork.Commit();
                return RedirectToAction(nameof(AddImages), new
                {
                    Id = model.Product.ProductId,
                });

            }
            TempData["error"] = "Failed: Invalid Data";
            model.Categories = unitOfWork.CategoryRepository.GetAll();
            return View(model);
        }
        [HttpGet]
        public IActionResult AddImages(int Id)
        {
            ProductModel model = unitOfWork.ProductRepository.Get(i => i.ProductId == Id, includeProp: "Images");
            return View(model);
        }
        [HttpPost]
        public IActionResult AddImages(ProductModel model, List<IFormFile>? files)
        {
            model = unitOfWork.ProductRepository.Get(i => i.ProductId == model.ProductId, includeProp: "Images");
            String webRootPath = webHostEnvironment.WebRootPath;
            if (files != null)
            {
                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = ImagePlaceHolders.ProductImagePath + model.ProductId.ToString();
                    string finalPath = Path.Combine(webRootPath, productPath);
                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }
                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    ProductImagesModel productImage = new ProductImagesModel
                    {
                        Url = @"\" + Path.Combine(productPath, fileName),
                        ProductId = model.ProductId,
                    };
                    if (model.Images == null)
                    {
                        model.Images = new List<ProductImagesModel>();
                    }
                    model.Images.Add(productImage);
                }
                if (model.ImageUrl == null || model.ImageUrl == ImagePlaceHolders.ProductImage)
                {
                    model.ImageUrl = model.Images[0].Url.TrimStart('\\');
                }
                unitOfWork.ProductRepository.Update(model);
                unitOfWork.Commit();
                TempData["success"] = "Data Updated successfully";
                return RedirectToAction(nameof(AddImages), new
                {
                    Id = model.ProductId,
                });
            }
            TempData["success"] = "Operation failed";
            return View(model);
        }

        public IActionResult DeleteImage(int Id)
        {
            ProductImagesModel model = unitOfWork.ProductImagesRepository.Get(i => i.Id == Id);
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.Url))
                {
                    var imagepath = Path.Combine(webHostEnvironment.WebRootPath, model.Url.TrimStart('\\'));
                    if (System.IO.Path.Exists(imagepath))
                    {
                        System.IO.File.Delete(imagepath);
                    }
                }
                unitOfWork.ProductImagesRepository.Remove(model);
                unitOfWork.Commit();
                TempData["success"] = "Images Deleted Successfully";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveAllImages(int Id)
        {
            List<ProductImagesModel> model = unitOfWork.ProductImagesRepository.GetAll(i => i.ProductId == Id).ToList();
            if (model != null)
            {
                var webRootPath = webHostEnvironment.WebRootPath;
                foreach (var image in model)
                {
                    var imagePath = Path.Combine(webRootPath, image.Url.TrimStart('\\'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }
            var product = unitOfWork.ProductRepository.Get(i => i.ProductId == Id);
            product.ImageUrl = ImagePlaceHolders.ProductImage;
            unitOfWork.ProductImagesRepository.RemoveRange(model);
            unitOfWork.Commit();
            TempData["success"] = "Images Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<ProductModel> products = unitOfWork.ProductRepository.GetAll(includeProp: "Category");
            return Json(new
            {
                data = products
            });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            ProductModel Product = unitOfWork.ProductRepository.Get(u => u.ProductId == id, includeProp: "Images");
            if (Product != null)
            {
                if (Product.ImageUrl != null && Product.ImageUrl != ImagePlaceHolders.ProductImage)
                {
                    var ImagesPath = Path.Combine(webHostEnvironment.WebRootPath, Product.ImageUrl.TrimStart('\\'));
                    ImagesPath = Path.GetDirectoryName(ImagesPath);
                    if (Path.Exists(ImagesPath))
                    {
                        Directory.Delete(ImagesPath, true);
                    }

                }
                unitOfWork.ProductImagesRepository.RemoveRange(Product.Images);
                unitOfWork.ProductRepository.Remove(Product);
                unitOfWork.Commit();
                return Json(new
                {
                    success = true,
                    message = "Data deleted successfully"
                });
            }
            return Json(new
            {
                success = false,
                message = "Could'nt delete - file not found"
            });

        }
        #endregion
    }
}
