using System.Net.Http.Headers;
using System.Text.Json;
using Common.Models;

namespace IntegrationTest;

public abstract class IntegrationTestBase
{
    protected string _baseUrl;
    protected HttpClient _client;

    [OneTimeSetUp]
    public void Setup()
    {
        this._baseUrl = Environment.GetEnvironmentVariable("TEST_URL") ??  "http://localhost:5000/v1/";
        this._client = new HttpClient
        {
            BaseAddress = new Uri(this._baseUrl)
        };
    }
    
    protected async Task<HttpResponseMessage> CreatePlayer(Player player)
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = new Uri("Player", UriKind.Relative);
        httpRequestMessage.Method = HttpMethod.Post;
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(player));
        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return await this._client.SendAsync(httpRequestMessage);
    }
    
    protected async Task<HttpResponseMessage> CreateDeath(Player player, Death death)
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = new Uri($"Player/{player.Id}/Death", UriKind.Relative);
        httpRequestMessage.Method = HttpMethod.Post;
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(death));
        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return await this._client.SendAsync(httpRequestMessage);
    }

    protected static T Deserialise<T>(string json)
    {
        var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        return JsonSerializer.Deserialize<T>(json, jsonOptions);
    }

}