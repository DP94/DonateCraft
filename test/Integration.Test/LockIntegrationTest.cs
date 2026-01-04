using Common.Models;
using Lock = Common.Models.Lock;

namespace IntegrationTest;

public class LockIntegrationTest : IntegrationTestBase
{
    [Test]
    public async Task DeadPlayer_IsLocked()
    {
        var id = Guid.NewGuid().ToString();
        var player = new Player
        {
            Name = Guid.NewGuid().ToString(),
            Id = id
        };
        await CreatePlayer(player);
        
        var death = new Death
        {
            PlayerId = id,
            Reason = "Slain by a zombie",
            CreatedDate = DateTime.Today
        };
        await CreateDeath(player, death);

        var locks = await this._client.GetAsync($"Lock?playerIds={id}");
        var lockResponse = await locks.Content.ReadAsStringAsync();
        var locksObject = Deserialise<List<Lock>>(lockResponse);
        var playerLock = locksObject.First();
        Assert.That(playerLock.Unlocked, Is.False);
    }
}