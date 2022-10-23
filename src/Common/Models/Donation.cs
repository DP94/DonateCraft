namespace Common.Models;

public class Donation
{
    public string Id { get; set; }
    public double Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CharityId { get; set; }
    public string CharityName { get; set; }

    public Donation()
    {
    }

    public Donation(string id, double amount, DateTime created, int charityId, string charityName)
    {
        Id = id;
        Amount = amount;
        CreatedDate = created;
        CharityId = charityId;
        CharityName = charityName;
    }
}