namespace Common.Models.Sort;

public class PlayerSortCriteria : SortCriteriaBase<Player>
{
    private readonly static SortColumn IdSortColumn = new(1, "id");
    private readonly static SortColumn NameSortColumn = new(2, "name");
    private readonly static SortColumn DeathSortColumn = new(3, "deaths");
    private readonly static SortColumn DonationsSortColumn = new(4, "donations");

    public PlayerSortCriteria()
    {
        this.columns = new[]
        {
            IdSortColumn, NameSortColumn, DeathSortColumn, DonationsSortColumn
        };
    }


    public override void DoSort(SortColumn sortColumn, List<Player> players)
    {
        if (sortColumn.Id == IdSortColumn.Id)
        {
            players.Sort((player, player1) => string.Compare(player.Id, player1.Id, StringComparison.Ordinal));
        } 
        else if (sortColumn.Id == NameSortColumn.Id)
        {
            players.Sort((player, player1) => string.Compare(player.Name, player1.Name, StringComparison.OrdinalIgnoreCase));
        } 
        else if (sortColumn.Id == DeathSortColumn.Id)
        {
            players.Sort((player, player1) => player.Deaths.Count.CompareTo(player1.Deaths.Count));
        } 
        else if (sortColumn.Id == DonationsSortColumn.Id)
        {
            players.Sort((player, player1) => player.Donations.Count.CompareTo(player1.Deaths.Count));
        }
        if (!AscendingSort)
        {
            players.Reverse();
        }
    }
}