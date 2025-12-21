using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduSystem.UI.Web.Client.Services.Auth;

public static class JwtClaimParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(jwt);
            
            var json = JsonSerializer.Serialize(jwtToken.Payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (keyValuePairs == null)
                return claims;

            // Mapping keys to standard Role claim
            var roleKeys = new[] { "role", "Role", "roles", "Roles", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" };
            foreach (var roleKey in roleKeys)
            {
                if (keyValuePairs.TryGetValue(roleKey, out var roles))
                {
                    if (roles is JsonElement element)
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, element.GetString()!));
                        }
                        else if (element.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var role in element.EnumerateArray())
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role.GetString()!));
                            }
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                    }
                }
            }

            // Map standard claims
            var claimMappings = new Dictionary<string, string>
            {
                { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", ClaimTypes.NameIdentifier },
                { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", ClaimTypes.Email },
                { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", ClaimTypes.Name },
                { "sub", ClaimTypes.NameIdentifier },
                { "email", ClaimTypes.Email },
                { "name", ClaimTypes.Name },
                { "Name", ClaimTypes.Name },
                { "unique_name", ClaimTypes.Name }
            };

            foreach (var kvp in keyValuePairs)
            {
                if (claimMappings.TryGetValue(kvp.Key, out var claimType))
                {
                    if (!claims.Any(c => c.Type == claimType))
                    {
                        claims.Add(new Claim(claimType, kvp.Value?.ToString() ?? string.Empty));
                    }
                }
                else if (!roleKeys.Contains(kvp.Key))
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error parsing JWT: {ex.Message}");
        }

        return claims;
    }
}
