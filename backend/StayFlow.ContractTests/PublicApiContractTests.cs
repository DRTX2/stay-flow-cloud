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
            "/api/v1/Guests",
            "/api/v1/Services",
            "/api/v1/Invoices",
            "/api/v1/Analytics/dashboard",
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
    public async Task TokenEndpoint_PasswordGrant_HonoursOAuthTokenContract()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "admin@stayflow.local",
            ["password"] = "Admin123$",
            ["client_id"] = "stayflow-spa",
            ["scope"] = "stayflow.api offline_access",
        }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("token_type").GetString().Should().Be("Bearer");
        payload.GetProperty("access_token").GetString().Should().NotBeNullOrWhiteSpace();
        payload.GetProperty("expires_in").GetInt32().Should().BeGreaterThan(0);
        payload.GetProperty("refresh_token").GetString().Should().NotBeNullOrWhiteSpace();
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

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "admin@stayflow.local",
            ["password"] = "Admin123$",
            ["client_id"] = "stayflow-spa",
            ["scope"] = "stayflow.api",
        }));
        response.EnsureSuccessStatusCode();

        var token = (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("access_token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
