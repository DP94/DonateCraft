using Common.Models;
using Core.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PlayerController(IPlayerService playerService, IHttpContextAccessor httpContextAccessor)
    {
        this._playerService = playerService;
        this._httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [SwaggerResponse(200, "Success", typeof(Player))]
    [SwaggerOperation("Gets all players")]
    public async Task<IActionResult> Get()
    {
        var players = await this._playerService.GetPlayers();
        return Ok(players);
    }

    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Player))]
    [SwaggerOperation("Gets all player by id")]
    public async Task<IActionResult> Get(string id)
    {
        var player = await this._playerService.GetPlayerById(id);
        if (player == null)
        {
            return NotFound();
        }
        return Ok(player);
    }

    [HttpPost]
    [SwaggerOperation("Creates a new player")]
    [SwaggerResponse(201, "Player created successfully", typeof(Player))]
    public async Task<ActionResult> Post([FromBody] [SwaggerParameter("The book to create")] Player player)
    {
        var createdPlayer = await this._playerService.CreatePlayer(player);
        return Created($"{this._httpContextAccessor.HttpContext?.Request.GetEncodedUrl()}/{player.Id}", createdPlayer);
    }

    [SwaggerOperation("Updates a player")]
    [SwaggerResponse(200, "Player updated", typeof(Player))]
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Player player)
    {
        var updatedPlayer = await this._playerService.UpdatePlayer(player);
        return Ok(updatedPlayer);
    }

    [HttpDelete("{id}")]
    [SwaggerResponse(204, "Player deleted")]
    [SwaggerOperation("Deletes a player")]
    public async Task<IActionResult> Delete([SwaggerParameter("ID of the book to player")] string id)
    {
        await this._playerService.DeletePlayer(id);
        return NoContent();
    }
}