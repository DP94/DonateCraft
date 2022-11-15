using Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public abstract class WithPlayerIdController<T> : DonateCraftController<T> where T : WithPlayerId
{
    public abstract Task<IActionResult> GetAllForPlayer(string playerId);
    public abstract Task<IActionResult> GetByIdForPlayer(string playerId, string id);
    public abstract Task<IActionResult> Create(string playerId, T t);
    public abstract Task<IActionResult> Update(string playerId, T t);
    public abstract Task<IActionResult> Delete(string playerId, string id);
}