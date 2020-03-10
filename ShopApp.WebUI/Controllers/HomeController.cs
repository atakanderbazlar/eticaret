using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShopApp.Business.Abstract;
using ShopApp.WebUI.Models;

namespace ShopApp.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private IProductService _productService;
        public HomeController(IProductService productService)
        {
            _productService = productService;
        }
        //Model ile ürünleri databaseden alıp yazdırdık.
        public IActionResult Index(string search = null)
        {
            if (!string.IsNullOrEmpty(search))
            {
             //  var foundProducts = _productService.SearchProducts(search);
             //   return View(foundProducts);

            return View(new ProductListModel() {

                Products = _productService.SearchProducts(search)
            });

            }
            return View(new ProductListModel() {
                Products = _productService.GetAll()
            });
        }
    }
}