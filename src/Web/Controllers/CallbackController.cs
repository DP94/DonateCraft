using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("v1/[controller]")]
[EnableCors]
public class CallbackController : ControllerBase
{
    private HttpClient _client;

    public CallbackController(HttpClient client)
    {
        this._client = client;
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string data)
    {
        var split = data.Split("|");
        if (split.Length < 2)
        {
            return Ok();
        }
        var donationId = split[0];
        var player = split[1];
        
        var donationData = await this._client.GetAsync($"https://api.staging.justgiving.com/296a9290/v1/donation/{donationId}");
        
        
        return Redirect($"http://bookclubui.s3-website.eu-west-2.amazonaws.com/#/login?d={donationId}&p={player}");
    }   
}