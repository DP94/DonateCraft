namespace Common.Models.Sort;

public class SortColumn
{
    public int Id { get; set; }

    public string Name { get; set; }

    public SortColumn(int id, string name)
    {
        Id = id;
        Name = name;
    }
}