namespace Common.Models;

public class Lock : WithId
{
    public string DonationId { get; set; }
    
    public Donation Donation { get; set; }
    
    public LockStatus Status { get; set; }

    public Lock()
    {
    }

    public Lock(string id)
    {
        Id = id;
        Status = LockStatus.Created;
    }
}