namespace Common.Models;

public class Lock
{
    public string Id { get; set; }
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