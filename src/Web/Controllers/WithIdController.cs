using Common.Models;
using Common.Models.Sort;
using Common.Util;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public abstract class WithIdController<T> : DonateCraftController<T> where T : WithId
{
    public abstract Task<IActionResult> GetAll();
    public abstract Task<IActionResult> GetById(string id);
    public abstract Task<IActionResult> Create(T t);
    public abstract Task<IActionResult> Update(T t);
    public abstract Task<IActionResult> Delete(string id);
    
}