namespace Common.Models.Sort;

public class CharitySortCriteria : SortCriteriaBase<Charity>
{

    private readonly static SortColumn IdSortColumn = new(1, "id");
    private readonly static SortColumn DonationCountSortColumn = new(2, "donationCount");

    public CharitySortCriteria()
    {
        this.columns = new[]
        {
            IdSortColumn, DonationCountSortColumn
        };
    }

    public override void DoSort(SortColumn sortColumn, List<Charity> charities)
    {
        if (sortColumn.Id == IdSortColumn.Id)
        {
            charities.Sort((player, player1) => string.Compare(player.Id, player1.Id, StringComparison.Ordinal));
        }
        else if (sortColumn.Id == DonationCountSortColumn.Id)
        {
            charities.Sort((player, player1) => player.DonationCount.CompareTo(player1.DonationCount));
        }
    }
}