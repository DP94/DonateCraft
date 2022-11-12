namespace Common.Models;

public class Charity
{
    public string Id { get; set; }
    
    public int DonationCount { get; set; }

    public Charity()
    {
    }

    public Charity(string id, int donationCount)
    {
        Id = id;
        DonationCount = donationCount;
    }
}