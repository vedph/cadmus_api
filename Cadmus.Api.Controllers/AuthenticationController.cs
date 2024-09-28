using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RegisteredClaims = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using Microsoft.Extensions.Configuration;
using Cadmus.Api.Models;
using Cadmus.Api.Services.Auth;

namespace Cadmus.Api.Controllers;

/// <summary>
/// Authentication controller.
/// </summary>
/// <seealso cref="Controller" />
[ApiController]
public sealed class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="roleManager">The role manager.</param>
    /// <param name="signInManager">The sign in manager.</param>
    /// <param name="configuration">The configuration.</param>
    public AuthenticationController(UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    // to use with DI see:
    // https://levelup.gitconnected.com/add-extra-user-claims-in-asp-net-core-web-applications-1f28c98c9ec6

    // https://stackoverflow.com/questions/42036810/asp-net-core-jwt-mapping-role-claims-to-claimsidentity
    private async Task<IList<Claim>> GetUserClaims(ApplicationUser user)
    {
        // https://tools.ietf.org/html/rfc7519#section-4

        DateTimeOffset now = new(DateTime.UtcNow);
        IdentityOptions options = new();
        List<Claim> claims = new()
        {
            // (SUB) the principal that is the subject of the JWT
            new Claim(RegisteredClaims.Sub, user.UserName!),

            // (JWT ID) provides a unique identifier for the JWT
            new Claim(RegisteredClaims.Jti, Guid.NewGuid().ToString()),

            // (NBF)
            //new Claim(RegisteredClaims.Nbf, (DateTime.UtcNow - new TimeSpan(0, 0, 10)).ToString()),
            new Claim(RegisteredClaims.Nbf,
                (now - new TimeSpan(0, 0, 10)).ToUnixTimeSeconds().ToString()),

            new Claim(options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
            new Claim(options.ClaimsIdentity.UserNameClaimType, user.UserName!),

            // (IAT) issued at
            new Claim(RegisteredClaims.Iat, now.ToUnixTimeSeconds().ToString()),

            // email and its confirmation
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                user.Email!),
            // this claim name is arbitrary
            new Claim("vfd", user.EmailConfirmed? "true" : "false")
        };

        // claims from user claims
        IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // claims from user roles
        IList<string> userRoles = await _userManager.GetRolesAsync(user);
        foreach (string userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            ApplicationRole? role = await _roleManager.FindByNameAsync(userRole);
            if (role != null && _roleManager.SupportsRoleClaims)
            {
                IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);
                claims.AddRange(roleClaims);
            }
        }

        // claims from additional user properties
        // http://docs.oasis-open.org/imi/identity/v1.0/os/identity-1.0-spec-os.html#_Toc229451870

        claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
            user.FirstName!));
        claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
            user.LastName!));

        return claims;
    }

    /// <summary>
    /// Logins the specified user.
    /// </summary>
    /// <param name="model">The login model.</param>
    [HttpPost]
    [Route("api/auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginBindingModel model)
    {
        ApplicationUser? user = await _userManager.FindByNameAsync(model.Username!);

        if (user != null
            && await _userManager.CheckPasswordAsync(user, model.Password!))
        {
            IList<Claim> claims = await GetUserClaims(user);

            // this is a fake key, the real one is in the environment
            // ensure that this key is at least 16 chars long
            IConfigurationSection jwtSection = _configuration.GetSection("Jwt");
            string key = jwtSection["SecureKey"]!;
            SymmetricSecurityKey authSigningKey = new(
                Encoding.UTF8.GetBytes(key));

            // Note: don't include "www" in your Audience!
            // it seems it gets stripped whence mismatch and 401

            JwtSecurityToken token = new(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: new SigningCredentials(
                    authSigningKey,
                    SecurityAlgorithms.HmacSha256)
                );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        return Unauthorized();
    }

    /// <summary>
    /// Logs the user out.
    /// </summary>
    [HttpGet]
    [Route("api/auth/logout")]
    public async Task<IActionResult> Logout()
    {
        // Ask ASP.NET Core Identity to delete the local and external cookies created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        await _signInManager.SignOutAsync();

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application.
        return SignOut();
    }
}