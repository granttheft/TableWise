using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Tablewise.Infrastructure.Services;

namespace Tablewise.UnitTests.Services;

/// <summary>
/// DistributedLockService unit testleri.
/// </summary>
public class DistributedLockServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _redisDbMock;
    private readonly Mock<ILogger<DistributedLockService>> _loggerMock;

    public DistributedLockServiceTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _redisDbMock = new Mock<IDatabase>();
        _loggerMock = new Mock<ILogger<DistributedLockService>>();

        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDbMock.Object);
    }

    /// <summary>
    /// Lock alınabildiğinde handle dönmeli.
    /// </summary>
    [Fact]
    public async Task TryAcquireAsync_WhenLockAvailable_ReturnsHandle()
    {
        // Arrange
        _redisDbMock.Setup(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.Is<When>(w => w == When.NotExists),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var service = new DistributedLockService(_redisMock.Object, _loggerMock.Object);

        // Act
        var handle = await service.TryAcquireAsync("test-lock", TimeSpan.FromSeconds(30));

        // Assert
        Assert.NotNull(handle);
        Assert.True(handle.IsAcquired);
    }

    /// <summary>
    /// Lock alınamadığında null dönmeli.
    /// </summary>
    [Fact]
    public async Task TryAcquireAsync_WhenLockTaken_ReturnsNull()
    {
        // Arrange
        _redisDbMock.Setup(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.Is<When>(w => w == When.NotExists),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(false); // Lock zaten alınmış

        var service = new DistributedLockService(_redisMock.Object, _loggerMock.Object);

        // Act
        var handle = await service.TryAcquireAsync("test-lock", TimeSpan.FromSeconds(30));

        // Assert
        Assert.Null(handle);
    }

    /// <summary>
    /// WaitForLockAsync timeout olduğunda null dönmeli.
    /// </summary>
    [Fact]
    public async Task WaitForLockAsync_WhenTimeout_ReturnsNull()
    {
        // Arrange - Lock her zaman alınamıyor
        _redisDbMock.Setup(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.Is<When>(w => w == When.NotExists),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var service = new DistributedLockService(_redisMock.Object, _loggerMock.Object);

        // Act - Kısa timeout ile dene
        var handle = await service.WaitForLockAsync(
            "test-lock",
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(100)); // 100ms timeout

        // Assert
        Assert.Null(handle);
    }

    /// <summary>
    /// Handle dispose edildiğinde lock serbest bırakılmalı.
    /// </summary>
    [Fact]
    public async Task LockHandle_WhenDisposed_ReleasesLock()
    {
        // Arrange
        _redisDbMock.Setup(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.Is<When>(w => w == When.NotExists),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _redisDbMock.Setup(x => x.ScriptEvaluateAsync(
            It.IsAny<string>(),
            It.IsAny<RedisKey[]>(),
            It.IsAny<RedisValue[]>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisResult.Create(1));

        var service = new DistributedLockService(_redisMock.Object, _loggerMock.Object);

        // Act
        var handle = await service.TryAcquireAsync("test-lock", TimeSpan.FromSeconds(30));
        Assert.NotNull(handle);

        await handle.DisposeAsync();

        // Assert - ScriptEvaluateAsync (Lua script ile atomic delete) çağrılmalı
        _redisDbMock.Verify(x => x.ScriptEvaluateAsync(
            It.IsAny<string>(),
            It.IsAny<RedisKey[]>(),
            It.IsAny<RedisValue[]>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    /// <summary>
    /// Lock key doğru prefix ile oluşturulmalı.
    /// </summary>
    [Fact]
    public async Task TryAcquireAsync_UsesCorrectKeyPrefix()
    {
        // Arrange
        RedisKey capturedKey = default;

        _redisDbMock.Setup(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan?, When, CommandFlags>((key, _, _, _, _) => capturedKey = key)
            .ReturnsAsync(true);

        var service = new DistributedLockService(_redisMock.Object, _loggerMock.Object);

        // Act
        await service.TryAcquireAsync("my-resource", TimeSpan.FromSeconds(30));

        // Assert
        Assert.Contains("lock:", capturedKey.ToString());
        Assert.Contains("my-resource", capturedKey.ToString());
    }
}
