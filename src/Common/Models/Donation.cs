namespace Common.Models;

public class Donation : WithId
{
    public double Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CharityId { get; set; }
    public string CharityName { get; set; }
    
    public string PaidForId { get; set; }
    
    public Player PaidForBy { get; set; }
    
    public bool Private { get; set; }

    public Donation()
    {
    }

    public Donation(string id, double amount, DateTime created, int charityId, string charityName, string paidForId, bool Private)
    {
        Id = id;
        Amount = amount;
        CreatedDate = created;
        CharityId = charityId;
        CharityName = charityName;
        PaidForId = paidForId;
        this.Private = Private;
    }
}