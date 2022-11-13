namespace Common.Models.Sort;

public class LockSortCriteria : SortCriteriaBase<Lock>
{

    private readonly static SortColumn IdSortColumn = new(1, "id");
    private readonly static SortColumn UnlockedSortColumn = new(2, "unlocked");

    public LockSortCriteria()
    {
        this.columns = new[]
        {
            IdSortColumn, UnlockedSortColumn
        };
    }

    public override void DoSort(SortColumn sortColumn, List<Lock> charities)
    {
        if (sortColumn.Id == IdSortColumn.Id)
        {
            charities.Sort((lock1, lock2) => string.Compare(lock1.Id, lock2.Id, StringComparison.Ordinal));
        }
        else if (sortColumn.Id == UnlockedSortColumn.Id)
        {
            charities.Sort((lock1, lock2) => lock1.Unlocked.CompareTo(lock2.Unlocked));
        }
    }
}