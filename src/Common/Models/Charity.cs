namespace Common.Models;

public class Charity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }

    public Charity()
    {
    }

    public Charity(string id, string name, string description, string url)
    {
        Id = id;
        Name = name;
        Description = description;
        Url = url;
    }
}