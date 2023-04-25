namespace Common.Models;

public class JustGivingDonation
{
    public string Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string DonationDate { get; set; }
    public string DonationRef { get; set; }
    public string DonorDisplayName { get; set; }
    public string DonorLocalAmount { get; set; }
    public string DonorLocalCurrencyCode { get; set; }
    public int Id { get; set; }
    public string Image { get; set; }
    public string Message { get; set; }
    public string Source { get; set; }
    public string Status { get; set; }
    public int CharityId { get; set; }
    public string PageShortName { get; set; }
}