using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Common.Models;

namespace IntegrationTest;

public class DeathIntegrationTest : IntegrationTestBase
{
    [Test]
    public async Task CreateDeath_SuccessfullyMarksPlayer_AsDead()
    {
        var id = Guid.NewGuid().ToString();
        var player = new Player
        {
            Name = Guid.NewGuid().ToString(),
            Id = id
        };
        var response = await CreatePlayer(player);
        
        var playerLocation = response.Headers.Location?.ToString();
        var responsePlayer = Deserialise<Player>(response.Content.ReadAsStringAsync().Result);
        Assert.That(responsePlayer.IsDead, Is.False);

        var death = new Death
        {
            PlayerId = id,
            Reason = "Slain by a zombie",
            CreatedDate = DateTime.Today
        };
        var deathResponse = await this.CreateDeath(player, death);
        var location = deathResponse.Headers.Location?.ToString();
        var deathResponseContent = await deathResponse.Content.ReadAsStringAsync();
        var deathResponseObject = Deserialise<Death>(deathResponseContent);
        response = await this._client.GetAsync(playerLocation);
        player = Deserialise<Player>(response.Content.ReadAsStringAsync().Result);
        
        Assert.That(deathResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(location, Is.EqualTo($"{this._baseUrl}Player/{id}/Death/{deathResponseObject.Id}"));
        Assert.That(player.IsDead,  Is.True);
        Assert.That(player.Deaths, Has.Count.EqualTo(1));
        Assert.That(player.Deaths[0].Id, Is.EqualTo(deathResponseObject.Id));
        Assert.That(player.Deaths[0].Reason, Is.EqualTo(deathResponseObject.Reason));
        Assert.That(deathResponseObject.PlayerId, Is.EqualTo(id));
        Assert.That(deathResponseObject.AutoRevived, Is.False);
    }

    [Test]
    public async Task CreateDeath_AutoRevives_AndSpendsCredit_WhenPlayerHasCredits()
    {
        var id = Guid.NewGuid().ToString();
        var player = new Player
        {
            Name = Guid.NewGuid().ToString(),
            Id = id,
            Credits = 3
        };
        var createResponse = await CreatePlayer(player);
        var playerLocation = createResponse.Headers.Location?.ToString();

        var death = new Death
        {
            PlayerId = id,
            Reason = "Slain by a zombie",
            CreatedDate = DateTime.Today
        };
        var deathResponse = await this.CreateDeath(player, death);
        var deathResponseObject = Deserialise<Death>(await deathResponse.Content.ReadAsStringAsync());

        Assert.That(deathResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(deathResponseObject.AutoRevived, Is.True);

        var reloadResponse = await this._client.GetAsync(playerLocation);
        var reloaded = Deserialise<Player>(await reloadResponse.Content.ReadAsStringAsync());
        Assert.That(reloaded.Credits, Is.EqualTo(2));

        var locks = await this._client.GetAsync($"Lock?playerIds={id}");
        var locksList = Deserialise<List<Common.Models.Lock>>(await locks.Content.ReadAsStringAsync());
        Assert.That(locksList, Is.Empty);
    }
}