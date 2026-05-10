using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tablewise.Application.DTOs.Booking;
using Tablewise.IntegrationTests.Fixtures;

namespace Tablewise.IntegrationTests.Features.Booking;

/// <summary>
/// Eş zamanlı rezervasyon testleri.
/// Testcontainers ile gerçek Redis ve PostgreSQL kullanır.
/// </summary>
[Collection("Database")]
public class ConcurrentReservationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Test constructor.
    /// </summary>
    public ConcurrentReservationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 100 eş zamanlı istek aynı slota → 1 başarılı, 99 hata.
    /// Bu test distributed lock mekanizmasını doğrular.
    /// </summary>
    [Fact]
    public async Task ConcurrentReservations_OnSameSlot_OnlyOneSucceeds()
    {
        // Arrange
        var reservedFor = DateTime.UtcNow.Date.AddDays(7).AddHours(19); // 1 hafta sonra, 19:00
        var successCount = 0;
        var conflictCount = 0;
        var lockObject = new object();

        var tasks = new List<Task>();
        const int concurrentRequests = 100;

        // Act: 100 paralel istek gönder
        for (var i = 0; i < concurrentRequests; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                using var client = _factory.CreateClient();

                var request = new ReserveRequestDto
                {
                    GuestName = $"Test Guest {taskId}",
                    GuestEmail = $"guest{taskId}@test.com",
                    GuestPhone = $"+9055512345{taskId:D2}",
                    PartySize = 4,
                    ReservedFor = reservedFor,
                    PrivacyPolicyAccepted = true
                };

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/book/{_factory.TestSlug}/reserve")
                {
                    Content = JsonContent.Create(request, options: JsonOptions)
                };

                // Her istek farklı Idempotency-Key kullanmalı
                httpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

                var response = await client.SendAsync(httpRequest).ConfigureAwait(false);

                lock (lockObject)
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        successCount++;
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        conflictCount++;
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert: Sadece 1 başarılı olmalı, geri kalanı conflict
        Assert.Equal(1, successCount);
        Assert.Equal(concurrentRequests - 1, conflictCount);
    }

    /// <summary>
    /// Idempotency-Key aynı → aynı response döner.
    /// X-Idempotency-Replay header'ı ikinci istekte mevcut olmalı.
    /// </summary>
    [Fact]
    public async Task SameIdempotencyKey_ReturnsCachedResponse()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var reservedFor = DateTime.UtcNow.Date.AddDays(8).AddHours(20); // Farklı slot

        var request = new ReserveRequestDto
        {
            GuestName = "Idempotency Test Guest",
            GuestEmail = "idempotency@test.com",
            GuestPhone = "+905559876543",
            PartySize = 2,
            ReservedFor = reservedFor,
            PrivacyPolicyAccepted = true
        };

        // Act: İlk istek
        var firstRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/book/{_factory.TestSlug}/reserve")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        firstRequest.Headers.Add("Idempotency-Key", idempotencyKey);

        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var firstResult = await firstResponse.Content.ReadFromJsonAsync<ReserveResponseDto>(JsonOptions);
        Assert.NotNull(firstResult);
        Assert.False(firstResponse.Headers.Contains("X-Idempotency-Replay"));

        // Act: Aynı key ile ikinci istek
        var secondRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/book/{_factory.TestSlug}/reserve")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        secondRequest.Headers.Add("Idempotency-Key", idempotencyKey);

        var secondResponse = await _client.SendAsync(secondRequest);

        // Assert: Aynı response dönmeli + replay header
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.True(secondResponse.Headers.Contains("X-Idempotency-Replay"));

        var secondResult = await secondResponse.Content.ReadFromJsonAsync<ReserveResponseDto>(JsonOptions);
        Assert.NotNull(secondResult);
        Assert.Equal(firstResult!.ConfirmCode, secondResult!.ConfirmCode);
        Assert.Equal(firstResult.ReservationId, secondResult.ReservationId);
    }

    /// <summary>
    /// Idempotency-Key header yoksa 400 BadRequest dönmeli.
    /// </summary>
    [Fact]
    public async Task MissingIdempotencyKey_Returns400BadRequest()
    {
        // Arrange
        var reservedFor = DateTime.UtcNow.Date.AddDays(9).AddHours(18);

        var request = new ReserveRequestDto
        {
            GuestName = "No Idempotency Key",
            GuestEmail = "no-key@test.com",
            GuestPhone = "+905551112233",
            PartySize = 3,
            ReservedFor = reservedFor,
            PrivacyPolicyAccepted = true
        };

        // Act: Idempotency-Key header olmadan istek
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/book/{_factory.TestSlug}/reserve")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        // Header eklenmedi!

        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Idempotency-Key", content);
    }

    /// <summary>
    /// Kısmi paralel istekler: 10 farklı slot, her slotta 10 istek → 10 başarılı.
    /// </summary>
    [Fact]
    public async Task MultipleSlots_ConcurrentRequests_OneSuccessPerSlot()
    {
        // Arrange
        const int numberOfSlots = 10;
        const int requestsPerSlot = 10;
        var baseDate = DateTime.UtcNow.Date.AddDays(10);
        var successCounts = new int[numberOfSlots];
        var lockObject = new object();

        var tasks = new List<Task>();

        for (var slotIndex = 0; slotIndex < numberOfSlots; slotIndex++)
        {
            var slot = slotIndex;
            var reservedFor = baseDate.AddHours(11 + slot); // Her slot farklı saat

            for (var reqIndex = 0; reqIndex < requestsPerSlot; reqIndex++)
            {
                var requestIndex = reqIndex;
                tasks.Add(Task.Run(async () =>
                {
                    using var client = _factory.CreateClient();

                    var request = new ReserveRequestDto
                    {
                        GuestName = $"Slot{slot} Guest{requestIndex}",
                        GuestEmail = $"slot{slot}guest{requestIndex}@test.com",
                        GuestPhone = $"+9055500{slot}{requestIndex:D2}",
                        PartySize = 2,
                        ReservedFor = reservedFor,
                        PrivacyPolicyAccepted = true
                    };

                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/book/{_factory.TestSlug}/reserve")
                    {
                        Content = JsonContent.Create(request, options: JsonOptions)
                    };
                    httpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

                    var response = await client.SendAsync(httpRequest).ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        lock (lockObject)
                        {
                            successCounts[slot]++;
                        }
                    }
                }));
            }
        }

        await Task.WhenAll(tasks);

        // Assert: Her slot için tam olarak 1 başarılı
        for (var i = 0; i < numberOfSlots; i++)
        {
            Assert.Equal(1, successCounts[i]);
        }
    }
}
