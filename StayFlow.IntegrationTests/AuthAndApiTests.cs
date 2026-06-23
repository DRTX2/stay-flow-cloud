using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StayFlow.Infrastructure.Identity;

namespace StayFlow.IntegrationTests;

public sealed class AuthAndApiTests(StayFlowApiFactory factory) : IClassFixture<StayFlowApiFactory>
{
    [Fact]
    public async Task TokenEndpoint_PasswordGrant_ReturnsAccessToken()
    {
        var client = factory.CreateClient();

        var token = await GetAdminTokenAsync(client);

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
    public async Task Me_WithToken_ReturnsTenantAndPermissions()
    {
        var client = await CreateAuthenticatedClientAsync();

        var me = await client.GetFromJsonAsync<JsonElement>("/api/v1/me");

        me.GetProperty("tenantId").GetString().Should().NotBeNullOrEmpty();
        me.GetProperty("permissions").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Reservation_CreateConfirmCheckIn_Succeeds()
    {
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
    public async Task Rooms_PostWithReadOnlyClient_IsForbidden()
    {
        var client = factory.CreateClient();
        var token = await GetClientCredentialsTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/rooms", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = DataSeeder.AdminEmail,
            ["password"] = DataSeeder.AdminPassword,
            ["client_id"] = "stayflow-spa",
            ["scope"] = "stayflow.api offline_access",
        }));
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("access_token").GetString()!;
    }

    private static async Task<string> GetClientCredentialsTokenAsync(HttpClient client)
    {
        var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "stayflow-service",
            ["client_secret"] = "stayflow-service-secret",
            ["scope"] = "stayflow.api",
        }));
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("access_token").GetString()!;
    }
}
