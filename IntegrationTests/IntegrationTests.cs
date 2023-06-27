using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using EPM.Mouser.Interview.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationTests
{
	[TestClass]
	public class IntegrationTests
	{
		[TestMethod]
		public async Task WarehouseApi_GetProduct_WithParameter_5()
		{
			IWarehouseRepository warehouseRepository = new WarehouseRepository();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			JsonResult jsonResult = await warehouseApi.GetProduct(5);

			// Assert
			Assert.IsNotNull(jsonResult.Value);
		}

		[TestMethod]
		public async Task WarehouseApi_GetPublicInStockProducts()
		{
			IWarehouseRepository warehouseRepository = new WarehouseRepository();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			JsonResult jsonResult = await warehouseApi.GetPublicInStockProducts();

			// Assert
			List<Product> products = (List<Product>) jsonResult.Value;
			Assert.AreEqual(0, products.Count(x => x.InStockQuantity == 0), "x.InStockQuantity == 0");
			Assert.AreEqual(0, products.Count(x => x.InStockQuantity <= x.ReservedQuantity), "x.InStockQuantity >= x.ReservedQuantity");
		}
	}
}