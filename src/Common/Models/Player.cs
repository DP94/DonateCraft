namespace Common.Models;

public class Player
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public List<Death> Deaths { get; set; }

    public Player()
    {
        Deaths = new List<Death>();
    }
    
    public Player(string id, string name)
    {
        Id = id;
        Name = name;
        Deaths = new List<Death>();
    }
}