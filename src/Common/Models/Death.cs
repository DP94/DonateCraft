namespace Common.Models;

public class Death
{
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string Reason { get; set; }
    
    public string PlayerName { get; set; }
    
    public DateTime CreatedDate { get; set; }

    public Death()
    {
    }

    public Death(string id, string playerId, string reason, string playerName)
    {
        this.Id = id;
        this.PlayerId = playerId;
        this.Reason = reason;
        this.PlayerName = playerName;
    }
}