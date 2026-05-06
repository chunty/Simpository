namespace Simpository.Tests.Infrastructure;

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CompositeKeyEntity
{
    public int OrderId { get; set; }
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}
