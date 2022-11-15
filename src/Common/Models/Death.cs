namespace Common.Models;

public class Death : WithPlayerId
{
    public string Reason { get; set; }

    public DateTime CreatedDate { get; set; }

    public Death()
    {
    }

    public Death(string id, string reason)
    {
        this.Id = id;
        this.Reason = reason;
    }
}