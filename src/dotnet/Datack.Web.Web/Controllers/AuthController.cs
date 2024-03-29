﻿using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers;

[Route("Api/Authentication")]
public class AuthController : Controller
{
    private readonly Authentication _authentication;

    public AuthController(Authentication authentication)
    {
        _authentication = authentication;
    }
        
    [AllowAnonymous]
    [Route("IsSetup")]
    [HttpGet]
    public async Task<ActionResult<Boolean>> IsLoggedIn()
    {
        var user = await _authentication.GetUser();

        if (user == null)
        {
            return Ok(false);
        }
            
        return Ok(true);
    }

    [AllowAnonymous]
    [Route("Login")]
    [HttpPost]
    public async Task<ActionResult> Login([FromBody] AuthControllerLoginRequest request)
    {
        if (String.IsNullOrWhiteSpace(request.UserName) || String.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Invalid credentials");
        }

        var user = await _authentication.GetUser();

        if (user == null)
        {
            var registerResult = await _authentication.Register(request.UserName, request.Password);

            if (!registerResult.Succeeded)
            {
                return BadRequest(registerResult.Errors.First().Description);
            }
        }

        var result = await _authentication.Login(request.UserName, request.Password, request.RememberMe);

        if (!result.Succeeded)
        {
            return BadRequest("Invalid credentials");
        }

        user = await _authentication.GetUser();

        if (user == null)
        {
            return BadRequest("User not found");
        }

        return Ok(user.UserName);
    }
        
    [AllowAnonymous]
    [Route("Logout")]
    [HttpPost]
    public async Task<ActionResult> Logout()
    {
        await _authentication.Logout();
        return Ok();
    }
}

public class AuthControllerLoginRequest
{
    public String? UserName { get; set; }
    public String? Password { get; set; }
    public Boolean RememberMe { get; set; }
}