using Simpository.Exceptions;

namespace Simpository.Tests.Exceptions;

public class DataNotFoundExceptionTests
{
    [Fact]
    public void Message_WithNoKey_ReturnsBaseMessage()
    {
        var ex = new DataNotFoundException();

        Assert.Equal("Object not found", ex.Message);
    }

    [Fact]
    public void Message_WithSingleKey_IncludesKeyNameAndValue()
    {
        var ex = new DataNotFoundException(key: 42, keyName: "Id");

        Assert.Equal("Object not found with Id: 42", ex.Message);
    }

    [Fact]
    public void Message_WithCustomEntityType_IncludesEntityType()
    {
        var ex = new DataNotFoundException(key: 42, keyName: "Id", entityType: "Product");

        Assert.Equal("Product not found with Id: 42", ex.Message);
    }

    [Fact]
    public void Message_WithCompositeKeys_IncludesAllKeyNamesAndValues()
    {
        var ex = new DataNotFoundException(
            keys: [1, 2],
            keyNames: ["OrderId", "LineNumber"],
            entityType: "OrderLine");

        Assert.Equal("OrderLine not found with OrderId, LineNumber: 1, 2", ex.Message);
    }

    [Fact]
    public void Generic_Message_WithNoKey_UsesTypeName()
    {
        var ex = new DataNotFoundException<TestEntity>();

        Assert.Equal("TestEntity not found", ex.Message);
    }

    [Fact]
    public void Generic_Message_WithSingleKey_IncludesTypeNameAndKey()
    {
        var ex = new DataNotFoundException<TestEntity>(key: 99, keyName: "Id");

        Assert.Equal("TestEntity not found with Id: 99", ex.Message);
    }

    [Fact]
    public void Generic_Message_WithCompositeKeys_IncludesAllKeyNamesAndValues()
    {
        var ex = new DataNotFoundException<CompositeKeyEntity>(
            keys: [1, 2],
            keyNames: ["OrderId", "LineNumber"]);

        Assert.Equal("CompositeKeyEntity not found with OrderId, LineNumber: 1, 2", ex.Message);
    }

    [Fact]
    public void IsException_CanBeCaughtAsBaseException()
    {
        var ex = new DataNotFoundException<TestEntity>(42);
        Assert.IsAssignableFrom<DataNotFoundException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
