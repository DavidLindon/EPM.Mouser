using Microsoft.AspNetCore.Mvc;
using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace EPM.Mouser.Interview.Web.Controllers
{
    public class WarehouseApi : Controller
    {
        IWarehouseRepository _warehouseRepository;
        public WarehouseApi(IWarehouseRepository warehouseRepository) {
            _warehouseRepository = warehouseRepository;
		}

		/*
         *  Action: GET
         *  Url: api/warehouse/id
         *  This action should return a single product for an Id
         */
		[HttpGet]
        public async Task<JsonResult> GetProduct(long id)
        {
            // Get should be renamed GetAsync as per naming convensions as it is asynchronous
            Product product = await _warehouseRepository.Get(id);
            if (product == null)
            {
				return new JsonResult($"No product found for id {id}") { StatusCode = (int?)HttpStatusCode.NotFound };
			}
			return Json(product);
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        [HttpGet]
        public async Task<JsonResult> GetPublicInStockProducts()
        {
			// Query should be renamed QueryAsync as per naming convensions as it is asynchronous
			return Json(await _warehouseRepository.Query(x=> x.InStockQuantity > 0 && x.InStockQuantity > x.ReservedQuantity));
        }


        /*
         *  Action: GET
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        public async Task<JsonResult> OrderItem(UpdateQuantityRequest updateQuantityRequest)
        {
            if (updateQuantityRequest.Quantity <= 0)
            {
                return Json(NewUpdateResponse(ErrorReason.QuantityInvalid));
            }
			Product product = await _warehouseRepository.Get(updateQuantityRequest.Id);
			if (product == null)
			{
				return Json(NewUpdateResponse(ErrorReason.InvalidRequest));
			}
            if (product.InStockQuantity - product.ReservedQuantity - updateQuantityRequest.Quantity < 0)
            {
				return Json(NewUpdateResponse(ErrorReason.NotEnoughQuantity));
			}
            product.ReservedQuantity += updateQuantityRequest.Quantity;
            await _warehouseRepository.UpdateQuantities(product);
			return Json(new UpdateResponse() { Success = true });
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        public async Task<JsonResult> ShipItemAsync(UpdateQuantityRequest updateQuantityRequest)
        {
			if (updateQuantityRequest.Quantity <= 0)
			{
				return Json(NewUpdateResponse(ErrorReason.QuantityInvalid));
			}
			Product product = await _warehouseRepository.Get(updateQuantityRequest.Id);
			if (product == null)
			{
				return Json(NewUpdateResponse(ErrorReason.InvalidRequest));
			}
			if (product.InStockQuantity - product.ReservedQuantity - updateQuantityRequest.Quantity < 0)
			{
				return Json(NewUpdateResponse(ErrorReason.NotEnoughQuantity));
			}
			product.ReservedQuantity -= updateQuantityRequest.Quantity;
			product.InStockQuantity -= updateQuantityRequest.Quantity;
			await _warehouseRepository.UpdateQuantities(product);
			return Json(new UpdateResponse() { Success = true });
		}

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        public async Task<JsonResult> RestockItemAsync(UpdateQuantityRequest updateQuantityRequest)
        {
			if (updateQuantityRequest.Quantity <= 0)
			{
				return Json(NewUpdateResponse(ErrorReason.QuantityInvalid));
			}
			Product product = await _warehouseRepository.Get(updateQuantityRequest.Id);
			if (product == null)
			{
				return Json(NewUpdateResponse(ErrorReason.InvalidRequest));
			}
			product.InStockQuantity += updateQuantityRequest.Quantity;
			await _warehouseRepository.UpdateQuantities(product);
			return Json(new UpdateResponse() { Success = true });
		}

        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(x)" to the name [like windows file system does, where x is the next avaiable number]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        public async Task< JsonResult> AddNewProduct(Product product)
        {
			if (product.InStockQuantity < 0)
			{
				return Json(NewUpdateResponse(ErrorReason.QuantityInvalid));
			}
			if (string.IsNullOrEmpty(product.Name))
			{
				return Json(NewUpdateResponse( ErrorReason.InvalidRequest ));
			}
            List<Product> products = await _warehouseRepository.Query(x => x.Name.StartsWith(product.Name.Trim()));
            if (products.Count > 0)
            {
                int maxCount = 0;
                Regex rx = new Regex(@"\((\d+)\)$");
                foreach (string name in products.Select(x => x.Name))
                {
                    Match m = rx.Match(name);
                    if (m.Success)
                    {
                        int temp = Convert.ToInt16(m.Groups[1].Value);
                        if (temp > maxCount)
                        {
                            maxCount = temp;
                        }
                    }
                }
                product.Name += $" ({(maxCount + 1)})";
            }
            product.ReservedQuantity = 0;
			await _warehouseRepository.Insert(product);
            return Json(new CreateResponse<Product>() { Model = product, Success = true });
        }

        public static UpdateResponse NewUpdateResponse(ErrorReason errorReason, bool success = false)
        {
            return new UpdateResponse() { ErrorReason = errorReason, Success = success };
        }
    }
}
