namespace Common.Models;

public class DonateCraftOptions
{
    public const string DonateCraft = "DonateCraft";
    
    public string JustGivingApiKey { get; set; }

    public string JustGivingApiUrl { get; set; }

    public string DonateCraftUiUrl { get; set; }
    
    public string PlayerTableName { get; set; }
    
    public string LockTableName { get; set; }
    
    public string CharityTableName { get; set; }
}