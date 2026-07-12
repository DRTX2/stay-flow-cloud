using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace StayFlow.IntegrationTests;

/// <summary>
/// Integration tests for the authentication and API layer.
///
/// NOTE: Since the password grant is intentionally disabled (replaced by Authorization Code + PKCE),
/// tests that need an authenticated admin token now use Client Credentials via the seeded
/// stayflow-service machine client. This client is seeded with SuperAdmin-level read permissions
/// and is appropriate for automated testing.
///
/// For tests that require user-context permissions (tenant_id claim, role-based actions),
/// the integration test factory seeds a test admin user and the token is obtained via
/// client credentials with a test-scoped token injected via the factory.
/// </summary>
public sealed class AuthAndApiTests(StayFlowApiFactory factory) : IClassFixture<StayFlowApiFactory>
{
    // Dev fallback values — match DataSeeder dev defaults.
    // In CI, these come from environment variables set in the test factory.
    private const string TestServiceClientId = "stayflow-service";
    private const string TestServiceClientSecret = "dev-service-secret-change-in-prod";
    private const string TestAdminClientId = "stayflow-test-admin";
    private const string TestAdminClientSecret = "dev-test-admin-secret";

    [Fact]
    public async Task ClientCredentials_ReturnsAccessToken()
    {
        var client = factory.CreateClient();

        var token = await GetClientCredentialsTokenAsync(client, TestServiceClientId, TestServiceClientSecret);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/rooms");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Rooms_WithToken_ReturnsSeededInventory()
    {
        var client = await CreateAuthenticatedClientAsync();

        var page = await client.GetFromJsonAsync<JsonElement>("/api/v1/rooms");

        page.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsPermissions()
    {
        var client = await CreateAuthenticatedClientAsync();

        var me = await client.GetFromJsonAsync<JsonElement>("/api/v1/me");

        me.GetProperty("permissions").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Reservation_CreateConfirmCheckIn_Succeeds()
    {
        // This test requires a user-context token with full reservation permissions.
        // The test factory injects a SuperAdmin bearer token via a custom header.
        var client = await CreateAuthenticatedClientAsync();

        var room = (await client.GetFromJsonAsync<JsonElement>("/api/v1/rooms"))
            .GetProperty("items")[0].GetProperty("id").GetString();
        var guest = (await client.GetFromJsonAsync<JsonElement>("/api/v1/guests"))
            .GetProperty("items")[0].GetProperty("id").GetString();

        var create = await client.PostAsJsonAsync("/api/v1/reservations", new
        {
            roomId = room,
            guestId = guest,
            checkIn = "2026-09-10",
            checkOut = "2026-09-13",
            numberOfGuests = 2,
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        (await client.PostAsync($"/api/v1/reservations/{id}/confirm", null)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsync($"/api/v1/reservations/{id}/check-in", null)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FrontDeskToday_WithToken_ReturnsOperationalBoard()
    {
        var client = await CreateAuthenticatedClientAsync();

        var board = await client.GetFromJsonAsync<JsonElement>("/api/v1/analytics/front-desk/today");

        board.GetProperty("arrivals").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        board.GetProperty("departures").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        board.GetProperty("roomIssues").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task SampleStay_WithToken_RunsEndToEndAndLeavesRoomDirty()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/v1/demo/sample-stay", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sample = await response.Content.ReadFromJsonAsync<JsonElement>();
        var roomId = sample.GetProperty("roomId").GetGuid();
        sample.GetProperty("reservationId").GetGuid().Should().NotBeEmpty();
        sample.GetProperty("invoiceId").GetGuid().Should().NotBeEmpty();

        var room = await client.GetFromJsonAsync<JsonElement>($"/api/v1/rooms/{roomId}");
        room.GetProperty("status").GetString().Should().Be("Available");
        room.GetProperty("cleaningStatus").GetString().Should().Be("Dirty");
    }

    [Fact]
    public async Task MaintenanceWorkOrder_ControlsRoomOutOfServiceLifecycle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var roomTypeId = (await client.GetFromJsonAsync<JsonElement>("/api/v1/roomtypes"))
            [0].GetProperty("id").GetString();

        var createRoom = await client.PostAsJsonAsync("/api/v1/rooms", new
        {
            number = $"M-{Guid.NewGuid().ToString("N")[..6]}",
            roomTypeId,
            basePrice = 99m,
            capacity = 2,
            floor = 9,
        });
        createRoom.StatusCode.Should().Be(HttpStatusCode.Created);
        var roomId = (await createRoom.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var createWorkOrder = await client.PostAsJsonAsync("/api/v1/maintenance/work-orders", new
        {
            roomId,
            description = "Integration test urgent repair",
            priority = "Urgent",
        });
        createWorkOrder.StatusCode.Should().Be(HttpStatusCode.OK);
        var workOrderId = (await createWorkOrder.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var outOfServiceRoom = await client.GetFromJsonAsync<JsonElement>($"/api/v1/rooms/{roomId}");
        outOfServiceRoom.GetProperty("status").GetString().Should().Be("OutOfService");

        var resolve = await client.PostAsJsonAsync($"/api/v1/maintenance/work-orders/{workOrderId}/resolve", new
        {
            notes = "Fixed by integration test",
        });
        resolve.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var restoredRoom = await client.GetFromJsonAsync<JsonElement>($"/api/v1/rooms/{roomId}");
        restoredRoom.GetProperty("status").GetString().Should().Be("Available");
    }

    [Fact]
    public async Task Staff_CreateAndUpdateRole_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();
        var email = $"staff-{Guid.NewGuid():N}@stayflow.local";

        var create = await client.PostAsJsonAsync("/api/v1/staff", new
        {
            fullName = "Integration Staff",
            email,
            password = "Staff12345$",
            roles = new[] { "FrontDesk" },
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var userId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var update = await client.PutAsJsonAsync($"/api/v1/staff/{userId}/roles", new
        {
            roles = new[] { "Manager" },
        });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var staff = await client.GetFromJsonAsync<JsonElement>("/api/v1/staff");
        var user = staff.GetProperty("users").EnumerateArray()
            .Single(item => item.GetProperty("id").GetGuid() == userId);
        user.GetProperty("roles").EnumerateArray().Select(role => role.GetString()).Should().Contain("Manager");
    }

    [Fact]
    public async Task ReportsCsv_WithToken_ReturnsNightAudit()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/reports/night-audit.csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().Contain("Metric,Value");
        csv.Should().Contain("Occupied Rooms");
    }

    [Fact]
    public async Task PublicBookingEnquiry_PersistsAndCanBeConvertedOnce()
    {
        var anonymous = factory.CreateClient();
        var hotels = await anonymous.GetFromJsonAsync<JsonElement>("/api/v1/public/hotels");
        var hotel = hotels[0];
        var roomType = hotel.GetProperty("roomTypes")[0];
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120));
        var checkOut = checkIn.AddDays(2);

        var created = await anonymous.PostAsJsonAsync("/api/v1/public/bookings", new
        {
            hotelSlug = hotel.GetProperty("slug").GetString(),
            roomTypeId = roomType.GetProperty("id").GetGuid(),
            checkIn,
            checkOut,
            guests = 1,
            fullName = "Public Test Guest",
            email = $"public-{Guid.NewGuid():N}@example.com",
            phone = "+1 555 0100",
        });
        created.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var reference = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("reference").GetString();

        var client = await CreateAuthenticatedClientAsync();
        var enquiries = await client.GetFromJsonAsync<JsonElement>("/api/v1/bookingenquiries?status=Pending&pageSize=100");
        var enquiry = enquiries.GetProperty("items").EnumerateArray()
            .Single(item => item.GetProperty("reference").GetString() == reference);
        var enquiryId = enquiry.GetProperty("id").GetGuid();
        var requestedRoomTypeId = enquiry.GetProperty("roomTypeId").GetGuid();
        var rooms = await client.GetFromJsonAsync<JsonElement>("/api/v1/rooms?pageSize=100");
        var roomId = rooms.GetProperty("items").EnumerateArray()
            .First(room => room.GetProperty("roomTypeId").GetGuid() == requestedRoomTypeId)
            .GetProperty("id").GetGuid();

        var converted = await client.PostAsJsonAsync($"/api/v1/bookingenquiries/{enquiryId}/convert", new { roomId });
        converted.StatusCode.Should().Be(HttpStatusCode.OK);
        var duplicate = await client.PostAsJsonAsync($"/api/v1/bookingenquiries/{enquiryId}/convert", new { roomId });
        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckedOutStay_FeedbackInvitationAcceptsOneResponse()
    {
        var client = await CreateAuthenticatedClientAsync();
        var roomId = (await client.GetFromJsonAsync<JsonElement>("/api/v1/rooms"))
            .GetProperty("items").EnumerateArray()
            .First(room => room.GetProperty("status").GetString() == "Available")
            .GetProperty("id").GetGuid();
        var guestId = (await client.GetFromJsonAsync<JsonElement>("/api/v1/guests"))
            .GetProperty("items")[0].GetProperty("id").GetGuid();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(240));
        var created = await client.PostAsJsonAsync("/api/v1/reservations", new
        {
            roomId,
            guestId,
            checkIn,
            checkOut = checkIn.AddDays(2),
            numberOfGuests = 1,
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
        var reservationId = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        (await client.PostAsync($"/api/v1/reservations/{reservationId}/confirm", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/v1/reservations/{reservationId}/check-in", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/v1/reservations/{reservationId}/check-out", null)).EnsureSuccessStatusCode();

        var invitation = await client.PostAsync($"/api/v1/reservations/{reservationId}/feedback-invitation", null);
        invitation.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = (await invitation.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        var anonymous = factory.CreateClient();
        var submitted = await anonymous.PostAsJsonAsync("/api/v1/public/feedback", new
        {
            token,
            rating = 5,
            comment = "A verified integration-test stay.",
        });
        submitted.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var duplicate = await anonymous.PostAsJsonAsync("/api/v1/public/feedback", new
        {
            token,
            rating = 1,
        });
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var feedback = await client.GetFromJsonAsync<JsonElement>("/api/v1/feedback?pageSize=100");
        feedback.GetProperty("items").EnumerateArray()
            .Should().Contain(item => item.GetProperty("reservationId").GetGuid() == reservationId);
    }

    [Fact]
    public async Task Rooms_PostWithReadOnlyClient_IsForbidden()
    {
        var client = factory.CreateClient();
        var token = await GetClientCredentialsTokenAsync(client, TestServiceClientId, TestServiceClientSecret);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/rooms", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();
        var token = await GetClientCredentialsTokenAsync(client, TestAdminClientId, TestAdminClientSecret);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string> GetClientCredentialsTokenAsync(
        HttpClient client,
        string clientId,
        string clientSecret)
    {
        var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["scope"] = "stayflow.api",
        }));
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("access_token").GetString()!;
    }
}
