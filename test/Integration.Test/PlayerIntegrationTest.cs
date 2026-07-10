using System.Net;
using System.Text.Json;
using Common.Models;

namespace IntegrationTest;

public class PlayerIntegrationTest : IntegrationTestBase
{

    [Test]
    public async Task CreatePlayer_Returns201()
    {
        var id = Guid.NewGuid().ToString();
        var player = new Player
        {
            Name = Guid.NewGuid().ToString(),
            Id = id
        };
        var response = await CreatePlayer(player);
        var location = response.Headers.Location?.ToString();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(location, Is.EqualTo($"{this._baseUrl}Player/{id}"));
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

        var response = await this._client.GetAsync(location);
        var str = await response.Content.ReadAsStringAsync();
        var responsePlayer = Deserialise<Player>(str);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responsePlayer, Is.Not.Null);
        Assert.That(responsePlayer.Name, Is.EqualTo(player.Name));
        Assert.That(responsePlayer.Id, Is.EqualTo(player.Id));
        Assert.That(responsePlayer.IsDead, Is.EqualTo(player.IsDead));
        Assert.That(responsePlayer.Credits, Is.EqualTo(0));
    }

    [Test]
    public async Task CreatePlayer_PersistsCredits()
    {
        var player = new Player
        {
            Name = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid().ToString(),
            Credits = 3
        };
        var createResponse = await CreatePlayer(player);
        var location = createResponse.Headers.Location?.ToString();

        var response = await this._client.GetAsync(location);
        var responsePlayer = Deserialise<Player>(await response.Content.ReadAsStringAsync());
        Assert.That(responsePlayer.Credits, Is.EqualTo(3));
    }
}