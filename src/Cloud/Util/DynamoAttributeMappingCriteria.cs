namespace Cloud.Util;

public class DynamoAttributeMappingCriteria
{
    public bool WithDeaths { get; set; }
    public bool WithDonations { get; set; }

    public DynamoAttributeMappingCriteria()
    {
        this.WithDeaths = true;
        this.WithDonations = true;
    }

    public DynamoAttributeMappingCriteria(bool withDeaths, bool withDonations)
    {
        WithDeaths = withDeaths;
        WithDonations = withDonations;
    }
}