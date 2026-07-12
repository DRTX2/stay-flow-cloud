using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace StayFlow.ContractTests;

/// <summary>
/// Pins the public, OAuth2-protected API contract: the OpenAPI surface, the token response, and the
/// response shapes external integrators depend on. Breaking any of these should fail CI.
/// </summary>
public sealed class PublicApiContractTests(ContractApiFactory factory) : IClassFixture<ContractApiFactory>
{
    private const string TestAdminClientId = "stayflow-test-admin";
    private const string TestAdminClientSecret = "dev-test-admin-secret";

    [Fact]
    public async Task OpenApiDocument_ExposesDocumentedPublicPaths()
    {
        var client = factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");

        var paths = document.GetProperty("paths")
            .EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        string[] expected =
        [
            "/api/v1/Rooms",
            "/api/v1/Reservations",
            "/api/v1/BookingEnquiries",
            "/api/v1/public/hotels",
            "/api/v1/public/bookings",
            "/api/v1/public/feedback",
            "/api/v1/Feedback",
            "/api/v1/Portal/reservations",
            "/api/v1/Guests",
            "/api/v1/Services",
            "/api/v1/Invoices",
            "/api/v1/Analytics/dashboard",
            "/api/v1/Analytics/front-desk/today",
            "/api/v1/Analytics/room-rack",
            "/api/v1/Analytics/setup-checklist",
            "/api/v1/Reports/revenue.csv",
            "/api/v1/Staff",
            "/api/v1/TenantFeatures",
            "/api/v1/Demo/sample-stay",
            "/connect/token",
        ];

        foreach (var path in expected)
        {
            paths.Should().Contain(
                actual => string.Equals(actual, path, StringComparison.OrdinalIgnoreCase),
                because: "{0} is part of the published API contract", path);
        }
    }

    [Fact]
    public async Task TokenEndpoint_ClientCredentials_HonoursOAuthTokenContract()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = TestAdminClientId,
            ["client_secret"] = TestAdminClientSecret,
            ["scope"] = "stayflow.api",
        }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("token_type").GetString().Should().Be("Bearer");
        payload.GetProperty("access_token").GetString().Should().NotBeNullOrWhiteSpace();
        payload.GetProperty("expires_in").GetInt32().Should().BeGreaterThan(0);
        payload.TryGetProperty("refresh_token", out _).Should().BeFalse("client credentials must not issue refresh tokens");
    }

    [Fact]
    public async Task ProtectedResource_WithoutToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/rooms");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Rooms_PagedResult_HasContractShape()
    {
        var client = await CreateAuthenticatedClientAsync();

        var page = await client.GetFromJsonAsync<JsonElement>("/api/v1/rooms");

        foreach (var property in new[] { "items", "page", "pageSize", "totalCount", "totalPages" })
        {
            page.TryGetProperty(property, out _).Should().BeTrue("paged results expose {0}", property);
        }

        page.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task Reservation_Resource_HasContractShape()
    {
        var client = await CreateAuthenticatedClientAsync();

        var roomId = (await client.GetFromJsonAsync<JsonElement>("/api/v1/rooms"))
            .GetProperty("items")[0].GetProperty("id").GetString();
        var guestId = (await client.GetFromJsonAsync<JsonElement>("/api/v1/guests"))
            .GetProperty("items")[0].GetProperty("id").GetString();

        var created = await client.PostAsJsonAsync("/api/v1/reservations", new
        {
            roomId,
            guestId,
            checkIn = "2026-10-01",
            checkOut = "2026-10-04",
            numberOfGuests = 2,
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var reservation = await created.Content.ReadFromJsonAsync<JsonElement>();
        foreach (var property in new[] { "id", "roomId", "guestId", "checkIn", "checkOut", "numberOfGuests", "totalPrice", "confirmationCode", "status", "nights" })
        {
            reservation.TryGetProperty(property, out _).Should().BeTrue("reservation exposes {0}", property);
        }
    }

    [Fact]
    public async Task FrontDeskToday_ExposesOperationalContractShape()
    {
        var client = await CreateAuthenticatedClientAsync();

        var board = await client.GetFromJsonAsync<JsonElement>("/api/v1/analytics/front-desk/today");

        foreach (var property in new[]
        {
            "date",
            "arrivals",
            "departures",
            "inHouse",
            "dirtyRooms",
            "outOfServiceRooms",
            "pendingHousekeepingTasks",
            "openMaintenanceWorkOrders",
            "pendingBookingEnquiries",
            "openOrders",
            "arrivalList",
            "departureList",
            "roomIssues",
        })
        {
            board.TryGetProperty(property, out _).Should().BeTrue("front desk board exposes {0}", property);
        }
    }

    [Fact]
    public async Task RoomRack_ExposesOperationalContractShape()
    {
        var client = await CreateAuthenticatedClientAsync();

        var rack = await client.GetFromJsonAsync<JsonElement>("/api/v1/analytics/room-rack");

        foreach (var property in new[] { "from", "to", "rooms" })
        {
            rack.TryGetProperty(property, out _).Should().BeTrue("room rack exposes {0}", property);
        }

        rack.GetProperty("rooms").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task SetupChecklist_ExposesGuidedSetupContractShape()
    {
        var client = await CreateAuthenticatedClientAsync();

        var checklist = await client.GetFromJsonAsync<JsonElement>("/api/v1/analytics/setup-checklist");

        foreach (var property in new[] { "completedSteps", "totalSteps", "percentComplete", "steps" })
        {
            checklist.TryGetProperty(property, out _).Should().BeTrue("setup checklist exposes {0}", property);
        }

        checklist.GetProperty("steps").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task TenantFeatures_ExposesPlanAndFeatureLockContractShape()
    {
        var client = await CreateAuthenticatedClientAsync();

        var features = await client.GetFromJsonAsync<JsonElement>("/api/v1/tenantfeatures");

        foreach (var property in new[] { "plan", "limits", "features", "featureDetails" })
        {
            features.TryGetProperty(property, out _).Should().BeTrue("tenant features exposes {0}", property);
        }

        features.GetProperty("featureDetails").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task Staff_ExposesAssignableRolesContractShape()
    {
        var client = await CreateAuthenticatedClientAsync();

        var staff = await client.GetFromJsonAsync<JsonElement>("/api/v1/staff");

        staff.TryGetProperty("assignableRoles", out _).Should().BeTrue();
        staff.TryGetProperty("users", out _).Should().BeTrue();
        var roles = staff.GetProperty("assignableRoles").EnumerateArray().Select(role => role.GetString()).ToList();
        roles.Should().Contain("FrontDesk");
        roles.Should().Contain("Housekeeping");
        roles.Should().Contain("Manager");
        roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task ReportsCsv_ReturnsCsvContent()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/reports/night-audit.csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
        (await response.Content.ReadAsStringAsync()).Should().Contain("Metric,Value");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = TestAdminClientId,
            ["client_secret"] = TestAdminClientSecret,
            ["scope"] = "stayflow.api",
        }));
        response.EnsureSuccessStatusCode();

        var token = (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("access_token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
