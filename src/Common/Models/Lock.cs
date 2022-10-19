namespace Common.Models;

public class Lock
{
    public string Id { get; set; }
    public string Key { get; set; }
    public bool Unlocked { get; set; }

    public Lock()
    {
    }

    public Lock(string id, string key, bool unlocked)
    {
        Id = id;
        Key = key;
        Unlocked = unlocked;
    }
}