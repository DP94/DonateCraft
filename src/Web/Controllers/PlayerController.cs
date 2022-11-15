using Cloud.Services;
using Common.Exceptions;
using Common.Models;
using Common.Models.Sort;
using Core.Services;
using Core.Services.Lock;
using Core.Services.Player;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class PlayerController : WithIdController<Player>
{
    private readonly IPlayerService _playerService;
    private readonly ILockService _lockService;

    public PlayerController(IPlayerService playerService, ILockService lockService)
    {
        this._playerService = playerService;
        this._lockService = lockService;
    }

    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Player))]
    [SwaggerOperation("Gets all players")]
    public async override Task<IActionResult> GetAll()
    {

        var players = await this._playerService.GetAll();
        var locks = await this._lockService.GetAll();
        foreach (var player in locks.SelectMany(aLock => players.Where(player => aLock.Id == player.Id)))
        {
            player.IsDead = true;
        }
        ProcessSorting(players);

        return Ok(players);
    }

    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Player))]
    [SwaggerResponse(404, "Player not found")]
    [SwaggerOperation("Gets all player by id")]
    public async override Task<IActionResult> GetById(string id)
    {
        var player = await this._playerService.GetById(id);
        await this.SetPlayersDeathStatus(player);
        return Ok(player);
    }

    [HttpPost]
    [SwaggerOperation("Creates a new player")]
    [SwaggerResponse(201, "Player created successfully", typeof(Player))]
    public async override Task<IActionResult> Create([FromBody] [SwaggerParameter("The book to create")] Player player)
    {
        if (string.IsNullOrWhiteSpace(player.Id))
        {
            return BadRequest("Player id must be supplied when creating a player");
        }
        
        var createdPlayer = await this._playerService.Create(player);
        return Created($"{this.HttpContext.Request.GetEncodedUrl()}/{player.Id}", createdPlayer);
    }

    [SwaggerOperation("Updates a player")]
    [SwaggerResponse(200, "Player updated", typeof(Player))]
    [HttpPut]
    public async override Task<IActionResult> Update([FromBody] Player player)
    {
        var updatedPlayer = await this._playerService.Update(player);
        return Ok(updatedPlayer);
    }

    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Player deleted")]
    [SwaggerOperation("Deletes a player")]
    public async override Task<IActionResult> Delete([SwaggerParameter("ID of the book to player")] string id)
    {
        await this._playerService.Delete(id);
        return NoContent();
    }

    private async Task SetPlayersDeathStatus(Player player)
    {
        try
        {
            await this._lockService.GetById(player.Id);
            player.IsDead = true;
        }
        catch (ResourceNotFoundException)
        {
            player.IsDead = false;
        }
    }

    public override SortCriteriaBase<Player> CreateSortCriteria()
    {
        return new PlayerSortCriteria();
    }
}