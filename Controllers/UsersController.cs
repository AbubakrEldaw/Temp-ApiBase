using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using APIBase.Services;
using APIBase.Models;
using APIBase.Utils.Encryption;
using Newtonsoft.Json;
using APIBase.Helpers;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    private IUserService _userService;
    private IEncMaster _encMaster;

    public UsersController(IUserService userService, IEncMaster encMaster)
    {
        _encMaster = encMaster;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public IActionResult Authenticate([FromBody] AuthenticateRequest model)
    {
        var response = _userService.Authenticate(model, ipAddress());

        if (response == null)
            return BadRequest("Username or password is incorrect");

        setTokenCookie(response.RefreshToken);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(AuthenticateResponse authResponse)
    {
        var refreshToken = authResponse.RefreshToken;
        var response = await _userService.RefreshToken(refreshToken, ipAddress());

        if (response == null)
            return Unauthorized(ErrorHelper.InvalidRefreshToken);

        setTokenCookie(  response.RefreshToken);

        return Ok(response);
    }

    [HttpPost("revoke-token")]
    public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
    {
        // accept token from request body or cookie
        var token = model.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token is required" });

        var response = _userService.RevokeToken(token, ipAddress());

        if (!response)
            return NotFound(new { message = "Token not found" });

        return Ok(new { message = "Token revoked" });
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var users = _userService.GetAll();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpGet("{id}/refresh-tokens")]
    public IActionResult GetRefreshTokens(int id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();

        return Ok(user.RefreshTokens);
    }

    // helper methods

    private void setTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string ipAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }


    //CLIENT METHODS

    [AllowAnonymous]
    [HttpPost("cauthenticate")]
    public IActionResult cAuthenticate([FromBody] string model)
    {
        var CIV = _encMaster.GetHeaderValue(HttpContext, "CIV", false);
        if (CIV == null)
        {
            return BadRequest();
        }

        var serializedModel = EncryptProvider.AESDecrypt(model, _encMaster.AppSettings.Encryptkey, CIV);

        var deserializedModel = JsonConvert.DeserializeObject<AuthenticateRequest>(serializedModel);
        var response = _userService.Authenticate(deserializedModel, ipAddress());

        if (response == null)
            return BadRequest("Username or password is incorrect");

        setTokenCookie(response.RefreshToken);
        var encResponse = EncryptProvider.AESEncrypt(JsonConvert.SerializeObject(response), _encMaster.AppSettings.Encryptkey, CIV);
        return Ok(encResponse);
    }

    [AllowAnonymous]
    [HttpPost("ccauthenticate")]
    public IActionResult ccAuthenticate()
    {
        return Ok();
    }


    [AllowAnonymous]
    [HttpGet("crefresh-token")]
    public async Task<IActionResult> cRefreshToken()
    {
        var refreshToken = _encMaster.GetHeaderValue(HttpContext, "RT", false);
        var CIV = _encMaster.GetHeaderValue(HttpContext, "CIV", false);
        if (CIV == null || refreshToken == null)
        {
            return BadRequest();
        }

        var response = await _userService.RefreshToken(refreshToken, ipAddress());

        if (response == null)
            return Unauthorized(new { message = "Invalid token" });

        setTokenCookie(response.RefreshToken);
        var encResponse = EncryptProvider.AESEncrypt(JsonConvert.SerializeObject(response), _encMaster.AppSettings.Encryptkey, CIV);
        return Ok(encResponse);
    }
}
