namespace Common.Models;

public class Charity : WithId
{

    public bool IsFundRaiser { get; set; }
    public int DonationCount { get; set; }

    public Charity()
    {
    }

    public Charity(string id, int donationCount, bool isFundRaiser)
    {
        Id = id;
        DonationCount = donationCount;
        IsFundRaiser = isFundRaiser;
    }
}