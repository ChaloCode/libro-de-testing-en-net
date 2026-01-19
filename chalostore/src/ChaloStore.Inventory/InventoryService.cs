namespace ChaloStore.Inventory;

public sealed record ProductDto(string Id, string Name, int Quantity, decimal Price);

public sealed class ServiceResult
{
    public bool Success { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(params string[] errors) => new() { Success = false, Errors = errors }; 
}

public interface IInventoryRepository
{
    void Add(ProductDto product);
    void UpdateQuantity(string id, int newQuantity);
    ProductDto? FindById(string id);
}

public sealed class InventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public ServiceResult AddProduct(ProductDto product)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(product.Id)) errors.Add("ID es obligatorio");
        if (string.IsNullOrWhiteSpace(product.Name)) errors.Add("Nombre es obligatorio");
        if (product.Name.Length > 100) errors.Add("Nombre excede 100 caracteres");
        if (product.Name.Any(c => "!@#$%".Contains(c))) errors.Add("Nombre contiene caracteres inválidos");
        if (product.Quantity < 0) errors.Add("Cantidad no puede ser negativa");
        if (product.Price < 0m) errors.Add("Precio no puede ser negativo");

        if (errors.Count > 0)
        {
            return ServiceResult.Fail(errors.ToArray());
        }

        _repository.Add(product);
        return ServiceResult.Ok();
    }

    public ServiceResult UpdateStock(string id, int delta)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(id)) errors.Add("ID es obligatorio");
        if (delta == 0) errors.Add("Delta no puede ser cero");

        var product = _repository.FindById(id);
        if (product is null)
        {
            errors.Add("Producto no encontrado");
        }
        else if (product.Quantity + delta < 0)
        {
            errors.Add("Stock resultante no puede ser negativo");
        }

        if (errors.Count > 0)
        {
            return ServiceResult.Fail(errors.ToArray());
        }

        _repository.UpdateQuantity(id, product!.Quantity + delta);
        return ServiceResult.Ok();
    }
}
