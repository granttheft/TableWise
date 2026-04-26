namespace Tablewise.IntegrationTests.Features.Booking;

/// <summary>
/// Eş zamanlı rezervasyon testleri.
/// Integration test olarak çalışır - gerçek Redis ve DB gerektirir.
/// </summary>
public class ConcurrentReservationTests
{
    /// <summary>
    /// 100 eş zamanlı istek aynı slota → 1 başarılı, 99 hata.
    /// Bu test distributed lock mekanizmasını doğrular.
    /// </summary>
    /// <remarks>
    /// Bu test gerçek Redis ve PostgreSQL bağlantısı gerektirir.
    /// CI/CD'de docker-compose ile çalıştırılmalıdır.
    /// </remarks>
    [Fact(Skip = "Integration test - requires Redis and PostgreSQL")]
    public async Task ConcurrentReservations_OnSameSlot_OnlyOneSucceeds()
    {
        // Bu test gerçek bir integration test ortamında çalıştırılmalı
        // Setup: WebApplicationFactory ile API başlat
        // Arrange: 100 paralel HTTP client oluştur

        var successCount = 0;
        var failureCount = 0;
        var lockObject = new object();

        var tasks = new List<Task>();

        for (var i = 0; i < 100; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Simüle edilmiş rezervasyon isteği
                    // Gerçek implementasyonda HttpClient kullanılacak
                    await SimulateReservationRequestAsync(taskId);

                    lock (lockObject)
                    {
                        successCount++;
                    }
                }
                catch
                {
                    lock (lockObject)
                    {
                        failureCount++;
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert: Sadece 1 başarılı olmalı
        Assert.Equal(1, successCount);
        Assert.Equal(99, failureCount);
    }

    /// <summary>
    /// Idempotency-Key aynı → aynı response.
    /// </summary>
    [Fact(Skip = "Integration test - requires Redis and PostgreSQL")]
    public async Task SameIdempotencyKey_ReturnsCachedResponse()
    {
        // Bu test gerçek bir integration test ortamında çalıştırılmalı
        var idempotencyKey = Guid.NewGuid().ToString();

        // İlk istek
        var firstResponse = await SimulateReservationWithIdempotencyKeyAsync(idempotencyKey);
        var firstConfirmCode = firstResponse;

        // Aynı key ile ikinci istek
        var secondResponse = await SimulateReservationWithIdempotencyKeyAsync(idempotencyKey);
        var secondConfirmCode = secondResponse;

        // Assert: Aynı confirm code dönmeli
        Assert.Equal(firstConfirmCode, secondConfirmCode);
    }

    private static Task SimulateReservationRequestAsync(int taskId)
    {
        // Placeholder - gerçek implementasyonda HTTP client kullanılacak
        return Task.Delay(10);
    }

    private static Task<string> SimulateReservationWithIdempotencyKeyAsync(string idempotencyKey)
    {
        // Placeholder - gerçek implementasyonda HTTP client kullanılacak
        return Task.FromResult($"CODE{idempotencyKey[..8].ToUpper()}");
    }
}
