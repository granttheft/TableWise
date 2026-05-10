namespace Tablewise.IntegrationTests.Fixtures;

/// <summary>
/// Database collection tanımı.
/// Test sınıfları arasında WebApplicationFactory paylaşımı için.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
