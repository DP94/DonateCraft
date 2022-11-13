namespace Common.Models;

public class Lock : WithId
{
    public bool Unlocked { get; set; }
    
    public string DonationId { get; set; }
    
    public Donation Donation { get; set; }

    public Lock()
    {
    }

    public Lock(string id, bool unlocked)
    {
        Id = id;
        Unlocked = unlocked;
    }
}