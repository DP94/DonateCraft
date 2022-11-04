namespace Common.Models;

public class Player
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public bool IsDead { get; set; }
    
    public List<Death> Deaths { get; set; }
    
    public List<Donation> Donations { get; set; }

    public Player()
    {
        Deaths = new List<Death>();
        Donations = new List<Donation>();
    }
    
    public Player(string id, string name)
    {
        Id = id;
        Name = name;
        Deaths = new List<Death>();
        Donations = new List<Donation>();
        IsDead = false;
    }
}