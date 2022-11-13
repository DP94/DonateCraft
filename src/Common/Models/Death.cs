namespace Common.Models;

public class Death : WithId
{
    public string Reason { get; set; }
    
    public string PlayerId { get; set; }
    
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