namespace Common.Models;

public class Charity : WithId
{

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