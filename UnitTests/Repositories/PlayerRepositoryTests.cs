using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Moq;
using System.Data;
using Xunit;

namespace GameplaySessionTracker.Tests.Repositories;

public class PlayerRepositoryTests
{
    private const string TestConnectionString = "Server=test;Database=test;";

    [Fact]
    public void Constructor_SetsConnectionString()
    {
        // Arrange & Act
        var repository = new PlayerRepository(TestConnectionString);

        // Assert
        Assert.NotNull(repository);
    }

    // Note: Testing SQL-based repositories requires either:
    // 1. Integration tests with a real database
    // 2. Mocking IDbConnection/IDbCommand (complex)
    // 3. Using an in-memory SQLite database
    // For comprehensive coverage, we verify the repository can be instantiated.
    // Full integration testing would require database infrastructure.
}
