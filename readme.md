# Mouser/EPM Tech assessment

The solution is written in VS2022 and .Net6

There are two controllers that need completing
 - WarehouseApi
    - There comments that document the required actions/endpoints
 - HomeController
    - Check the index view for the controller to see whats requested 

A DLL has been provided (EPM.Mouser.Interview.Data) which provides a data repo and some models
The data layer is 'faked' and initialized on startup so don't expect the same data between debug sessions (but it is persisted between requests).


Document any questions you'd ask and then also put your assumptions (in lieu of asking the questions).  Also highlight any improvements you've made to the orgional requirements.'
Feel free to also suggest what else could be done out side the scope of the requested changes.
Also document any enhancements you would apply.



## Definition for EPM.Mouser.Interview.Data.IWarehouseRepository
```
public interface IWarehouseRepository
{
    /// <summary>
    /// Gets the order for the given id
    /// </summary>
    /// <param name="id">The id of the product.</param>
    Task<Product?> Get(long id);

    /// <summary>
    /// Lists all products.
    /// </summary>
    Task<List<Product>> List();

    /// <summary>
    /// Queries the specified product data and returns a matching list.
    /// </summary>
    /// <param name="query">function to apply as a query against the data</param>
    Task<List<Product>> Query(Func<Product, bool> query);

    /// <summary>
    /// Updates the quantities for a product.
    /// </summary>
    /// <param name="model">The update model.</param>
    Task UpdateQuantities(Product model);

    /// <summary>
    /// Inserts a new product
    /// </summary>
    /// <param name="model">The model.</param>
    Task<Product> Insert(Product model);
}
```
## Definition for EPM.Mouser.Interview.Models
```
 public class Product
{
    /// <summary>
    /// Product Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Product Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Quantity currently in the warehouse
    /// </summary>
    public int InStockQuantity { get; set; }

    /// <summary>
    /// Quantity reserved for existing orders
    /// </summary>
    public int ReservedQuantity { get; set; }
}

/// <summary>
/// Reason for failed requests
/// </summary>
public enum ErrorReason
{
    QuantityInvalid,
    NotEnoughQuantity,
    InvalidRequest
}

/// <summary>
/// Standard Response for Update Requests
/// </summary>
public class UpdateResponse
{
    /// <summary>
    /// Gets or sets the error reason when Success is false.
    /// </summary>
    /// <value>
    /// The error reason.
    /// </value>
    public ErrorReason? ErrorReason { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the update was a success.
    /// </summary>
    /// <value>
    ///   <c>true</c> if success; otherwise, <c>false</c>.
    /// </value>
    public bool Success { get; set; }
}

/// <summary>
/// Standard response for Create Requests
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="UpdateResponse" />
public class CreateResponse<T> : UpdateResponse
{
    /// <summary>
    /// Gets or sets the model created.
    /// </summary>
    public T Model { get; set; }
}

public class UpdateQuantityRequest
{
    /// <summary>
    /// Gets or sets the Id of the product to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the quantity change being requested.
    /// </summary>
    public int Quantity { get; set; }
}
```
