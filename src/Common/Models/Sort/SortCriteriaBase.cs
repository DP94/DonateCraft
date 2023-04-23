namespace Common.Models.Sort;

public abstract class SortCriteriaBase<T>
{
    protected SortColumn[] columns;
    public bool AscendingSort { get; set; }

    public SortColumn GetSortColumnByName(string name)
    {
        if (this.columns == null)
        {
            throw new InvalidOperationException("Sort columns is null!");
        }
        return this.columns.FirstOrDefault(column => column.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public abstract void DoSort(SortColumn sortColumn, List<T> sortItems);
}