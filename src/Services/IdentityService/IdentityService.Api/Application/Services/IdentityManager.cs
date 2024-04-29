using IdentityService.Api.Application.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Api.Application.Services;

public class IdentityManager : IIdentityService
{
    public Task<LoginResponseModel> LoginAsync(LoginRequestModel requestModel)
    {
        var claims = new Claim[]
        {
            new (ClaimTypes.NameIdentifier, requestModel.UserName),
            new (ClaimTypes.Name, "Sezer Ayran")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperStrongAndLongSecurityKeyMustBeGreaterThan512BitsForHmacSha512"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var expiry = DateTime.UtcNow.AddDays(2);

        var token = new JwtSecurityToken(claims: claims, expires: expiry, signingCredentials: credentials, notBefore: DateTime.Now);

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

        LoginResponseModel responseModel = new()
        {
            UserName = requestModel.UserName,
            UserToken = encodedJwt
        };

        return Task.FromResult(responseModel);
    }
}
