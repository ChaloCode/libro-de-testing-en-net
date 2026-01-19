using ChaloStore.Inventory;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ChaloStore.UnitTests;

public class InventoryServiceTests
{
    private readonly IInventoryRepository _repo = Substitute.For<IInventoryRepository>();
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        _service = new InventoryService(_repo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddProduct_InvalidId_ShouldFail(string? id)
    {
        var dto = new ProductDto(id ?? string.Empty, "Laptop", 1, 1000m);

        var result = _service.AddProduct(dto);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("ID es obligatorio");
        _repo.DidNotReceive().Add(Arg.Any<ProductDto>());
    }

    [Fact]
    public void AddProduct_ValidData_ShouldPersist()
    {
        var dto = new ProductDto("SKU-001", "Laptop", 5, 1000m);

        var result = _service.AddProduct(dto);

        result.Success.Should().BeTrue();
        _repo.Received(1).Add(dto);
    }

    [Fact]
    public void UpdateStock_ShouldReserveQuantity()
    {
        _repo.FindById("SKU-001").Returns(new ProductDto("SKU-001", "Laptop", 5, 1000m));

        var result = _service.UpdateStock("SKU-001", -2);

        result.Success.Should().BeTrue();
        _repo.Received(1).UpdateQuantity("SKU-001", 3);
    }
}
