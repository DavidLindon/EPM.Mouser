using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {

		IWarehouseRepository _warehouseRepository;
		public HomeController(IWarehouseRepository warehouseRepository)
		{
			_warehouseRepository = warehouseRepository;
		}

		[HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _warehouseRepository.List());
        }

        public static string GetFontColour(Product product)
        {
            int availableStock = product.InStockQuantity - product.ReservedQuantity;
            if (availableStock >= 10)
            {
                return "FFFFFF";
            }
            else if (availableStock > 0 && availableStock < 10)
            {
                return "#ff6600";
            } 
            else 
            {
                return "#FF0000";
            }
        }
    }
}
