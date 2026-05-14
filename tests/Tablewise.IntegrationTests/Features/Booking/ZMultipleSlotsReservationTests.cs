using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tablewise.Application.DTOs.Booking;
using Tablewise.IntegrationTests.Fixtures;

namespace Tablewise.IntegrationTests.Features.Booking;

/// <summary>
/// Çoklu slot eşzamanlı rezervasyon senaryosu; sınıf adı "Z" ile alfabetik sonda koşturulur,
/// böylece diğer entegrasyonların bıraktığı Redis/ölçüm durumundan sonra Restrict ile temiz başlanır.
/// </summary>
[Collection("Database")]
public sealed class ZMultipleSlotsReservationTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly CustomWebApplicationFactory _factory;

    /// <summary>
    /// Test constructor.
    /// </summary>
    public ZMultipleSlotsReservationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// 10 farklı slot; her slotta 10 eş zamanlı istek → slot başına 1 başarılı (toplam 100 paralel istek).
    /// </summary>
    [Fact]
    public async Task MultipleSlots_ConcurrentRequests_OneSuccessPerSlot()
    {
        await _factory.RestrictTestVenueToSingleTableAsync();
        const int numberOfSlots = 10;
        const int requestsPerSlot = 10;
        var baseDate = DateTime.UtcNow.Date.AddDays(10);
        var successCounts = new int[numberOfSlots];
        var lockObject = new object();

        var tasks = new List<Task>(numberOfSlots * requestsPerSlot);

        for (var slotIndex = 0; slotIndex < numberOfSlots; slotIndex++)
        {
            var slot = slotIndex;
            // Venue Europe/Istanbul 10:00–23:00; 10–19 UTC ≈ 13:00–22:00 local (UTC+3), 90 dk slotlar için güvenli aralık.
            var reservedFor = baseDate.AddHours(10 + slot);

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
                        GuestPhone = $"+9055512345{(slot * requestsPerSlot + requestIndex):D2}",
                        PartySize = 2,
                        ReservedFor = reservedFor,
                        PrivacyPolicyAccepted = true,
                    };

                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/book/{_factory.TestSlug}/reserve")
                    {
                        Content = JsonContent.Create(request, options: JsonOptions),
                    };
                    httpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

                    var response = await client.SendAsync(httpRequest);

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

        for (var i = 0; i < numberOfSlots; i++)
        {
            Assert.Equal(1, successCounts[i]);
        }
    }
}
