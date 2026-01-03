using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Common.Models;

namespace IntegrationTest;

public class PlayerIntegrationTest {
    
    private string _baseUrl;

    [OneTimeSetUp]
    public void Setup()
    {
        this._baseUrl = Environment.GetEnvironmentVariable("TEST_URL") ??  "http://localhost:5000/v1";
    }

    [Test]
    public async Task CreatePlayer_Returns201() {
        var id = Guid.NewGuid().ToString();
        var player = new Player
        {
            Name = Guid.NewGuid().ToString(),
            Id = id
        };
        var response = await CreatePlayer(player);
        var location = response.Headers.Location?.ToString();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(location, Is.EqualTo($"{this._baseUrl}/Player/{id}"));
    }

    [Test]
    public async Task GetPlayer_Returns200()
    {
        var player = new Player
        {
            Name = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid().ToString(),
            IsDead = false
        };
        
        var createResponse = await CreatePlayer(player);
        var location = createResponse.Headers.Location?.ToString();
        
        var client = new HttpClient();
        var response = await client.GetAsync(location);
        var str = await response.Content.ReadAsStringAsync();
        var responsePlayer = JsonSerializer.Deserialize<Player>(str, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responsePlayer, Is.Not.Null);
        Assert.That(responsePlayer.Name, Is.EqualTo(player.Name));
        Assert.That(responsePlayer.Id, Is.EqualTo(player.Id));
        Assert.That(responsePlayer.IsDead, Is.EqualTo(player.IsDead));
    }

    private async Task<HttpResponseMessage> CreatePlayer(Player player)
    {
        var client = new HttpClient();
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = new Uri($"{this._baseUrl}/Player");
        httpRequestMessage.Method = HttpMethod.Post;
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(player));
        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return await client.SendAsync(httpRequestMessage);
    }
}