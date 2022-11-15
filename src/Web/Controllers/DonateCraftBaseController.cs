using Common.Models;
using Common.Models.Sort;
using Common.Util;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public abstract class DonateCraftBaseController<T> : ControllerBase where T : WithId
{

    public abstract Task<IActionResult> GetById(string id);
    public abstract Task<IActionResult> GetAll();
    public abstract Task<IActionResult> Create(T t);
    public abstract Task<IActionResult> Update(T t);
    public abstract Task<IActionResult> Delete(string id);
    
    
    public void ProcessSorting(List<T> items)
    {
        var query = this.HttpContext.Request.Query;
        if (!query.TryGetValue(Constants.SORT_BY, out var sortBy))
        {
            return;
        }
        var defaultSortOrder = "asc";
        if (query.TryGetValue(Constants.SORT_ORDER, out var sortOrder) && !string.IsNullOrWhiteSpace(sortOrder))
        {
            defaultSortOrder = sortOrder.ToString();
        }
        var sortCriteria = CreateSortCriteria();
        sortCriteria.AscendingSort = defaultSortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);
        foreach (var sortColumn in sortBy.Select(sortQuery => sortCriteria.GetSortColumnByName(sortQuery)))
        {
            sortCriteria.DoSort(sortColumn, items);
        }
        if (!sortCriteria.AscendingSort)
        {
            items.Reverse();
        }
    }

    public abstract SortCriteriaBase<T> CreateSortCriteria();
}