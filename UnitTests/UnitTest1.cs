using EPM.Mouser.Interview.Data;
using NSubstitute;
using EPM.Mouser.Interview.Web.Controllers;
using EPM.Mouser.Interview.Models;
using System.Security.Cryptography.X509Certificates;

namespace UnitTests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public async Task WarehouseApi_GetProduct_WithParameter_5()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Get(5).Returns(Task.FromResult(new Product()));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			Product product = (Product)(await warehouseApi.GetProduct(5)).Value;

			// Assert
			await warehouseRepository.Received(1).Get(5);
			Assert.IsNotNull(product);
		}

		[TestMethod]
		public async Task WarehouseApi_GetProduct_WithParameter_9999999_HandleProductNotFound()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			string result = (string)(await warehouseApi.GetProduct(9999999)).Value;

			// Assert
			Assert.AreEqual("No product found for id 9999999", result);
		}

		[TestMethod]
		public async Task WarehouseApi_OrderItem_QtyZero_Returns_QuantityInvalid()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.OrderItem(new UpdateQuantityRequest() { Quantity = 0 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.QuantityInvalid, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().UpdateQuantities(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_OrderItem_NonExistantProduct_Returns_InvalidRequest()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.OrderItem(new UpdateQuantityRequest() { Id = 999999, Quantity = 1 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().UpdateQuantities(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_OrderItem_NotEnoughQuantity_Returns_QuantityInvalid()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Get(5).Returns(Task.FromResult(new Product() { InStockQuantity = 5, ReservedQuantity = 4 }));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.OrderItem(new UpdateQuantityRequest() { Id = 5, Quantity = 2 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.NotEnoughQuantity, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().UpdateQuantities(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_OrderItem_Returns_Success()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Get(5).Returns(Task.FromResult(new Product() { InStockQuantity = 5, ReservedQuantity = 4 }));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.OrderItem(new UpdateQuantityRequest() { Id = 5, Quantity = 1 })).Value;

			// Assert
			await warehouseRepository.Received(1).UpdateQuantities(Arg.Is<Product>(x => x.ReservedQuantity == 5));
			Assert.IsTrue(updateResponse.Success);
		}

		[TestMethod]
		public async Task WarehouseApi_ShipItem_QtyZero_Returns_QuantityInvalid()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.ShipItemAsync(new UpdateQuantityRequest() { Quantity = 0 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.QuantityInvalid, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().UpdateQuantities(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_ShipItem_NonExistantProduct_Returns_InvalidRequest()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.ShipItemAsync(new UpdateQuantityRequest() { Id = 999999, Quantity = 1 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().UpdateQuantities(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_ShipItem_NotEnoughQuantity_Returns_QuantityInvalid()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Get(5).Returns(Task.FromResult(new Product() { InStockQuantity = 5, ReservedQuantity = 4 }));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.ShipItemAsync(new UpdateQuantityRequest() { Id = 5, Quantity = 2 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.NotEnoughQuantity, updateResponse.ErrorReason);
		}

		[TestMethod]
		public async Task WarehouseApi_ShipItem_Returns_Success()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Get(5).Returns(Task.FromResult(new Product() { InStockQuantity = 5, ReservedQuantity = 4 }));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.ShipItemAsync(new UpdateQuantityRequest() { Id = 5, Quantity = 1 })).Value;

			// Assert
			await warehouseRepository.Received(1).UpdateQuantities(Arg.Is<Product>(x => x.InStockQuantity == 4 && x.ReservedQuantity == 3));
			Assert.IsTrue(updateResponse.Success);
		}

		[TestMethod]
		public async Task WarehouseApi_RestockItem_QtyZero_Returns_QuantityInvalid()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.RestockItemAsync(new UpdateQuantityRequest() { Quantity = 0 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.QuantityInvalid, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().UpdateQuantities(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_RestockItem_NonExistantProduct_Returns_InvalidRequest()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.RestockItemAsync(new UpdateQuantityRequest() { Id = 999999, Quantity = 1 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().UpdateQuantities(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_RestockItem_Returns_Success()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Get(5).Returns(Task.FromResult(new Product() { InStockQuantity = 5, ReservedQuantity = 4 }));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.RestockItemAsync(new UpdateQuantityRequest() { Id = 5, Quantity = 1 })).Value;

			// Assert
			await warehouseRepository.Received(1).UpdateQuantities(Arg.Is<Product>(x => x.InStockQuantity == 6));
			Assert.IsTrue(updateResponse.Success);
		}

		[TestMethod]
		public async Task WarehouseApi_AddNewProduct_QtyZero_Returns_QuantityInvalid()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.AddNewProduct(new Product() { InStockQuantity = -1 })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.QuantityInvalid, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().Insert(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_AddNewProduct_BlankName_Returns_InvalidRequest()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			UpdateResponse updateResponse = (UpdateResponse)(await warehouseApi.AddNewProduct(new Product() { InStockQuantity = 1, Name = string.Empty })).Value;

			// Assert
			Assert.IsFalse(updateResponse.Success);
			Assert.AreEqual(ErrorReason.InvalidRequest, updateResponse.ErrorReason);
			await warehouseRepository.DidNotReceive().Insert(Arg.Any<Product>());
		}

		[TestMethod]
		public async Task WarehouseApi_AddNewProduct()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Query(default).ReturnsForAnyArgs(Task.FromResult(new List<Product>()));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			CreateResponse<Product> createResponse = (CreateResponse<Product>)(await warehouseApi.AddNewProduct(new Product() { InStockQuantity = 1, Name = "Great Product" })).Value;

			// Assert
			Assert.IsTrue(createResponse.Success);
			Assert.AreEqual(0, createResponse.Model.ReservedQuantity);
			Assert.AreEqual("Great Product", createResponse.Model.Name);
			await warehouseRepository.Received(1).Insert(Arg.Is<Product>(x => x.Name == "Great Product"));
		}

		[TestMethod]
		public async Task WarehouseApi_AddNewProduct_NameAlreadExists()
		{
			IWarehouseRepository warehouseRepository = Substitute.For<IWarehouseRepository>();
			warehouseRepository.Query(default).ReturnsForAnyArgs(Task.FromResult(new List<Product>() { new Product() { Name = "Great Product" } }));
			WarehouseApi warehouseApi = new WarehouseApi(warehouseRepository);
			CreateResponse<Product> createResponse = (CreateResponse<Product>)(await warehouseApi.AddNewProduct(new Product() { InStockQuantity = 1, Name = "Great Product" })).Value;

			// Assert
			Assert.IsTrue(createResponse.Success);
			Assert.AreEqual(0, createResponse.Model.ReservedQuantity);
			Assert.AreEqual("Great Product (1)", createResponse.Model.Name);
			await warehouseRepository.Received(1).Insert(Arg.Is<Product>(x => x.Name == "Great Product (1)"));
		}
	}
}