using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Auth.Options;

namespace XYZ.Application.Services.Auth
{
    public sealed class JwtFactory : IJwtFactory
    {
        private readonly JwtOptions _opts;
        private readonly SigningCredentials _creds;

        public JwtFactory(IOptions<JwtOptions> options)
        {
            _opts = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
            _creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public Task<AccessTokenResult> CreateAccessTokenAsync(JwtSubject subject, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(_opts.AccessTokenMinutes);

            var claims = BuildClaims(subject, now);

            var token = new JwtSecurityToken(
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expires.UtcDateTime,
                signingCredentials: _creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult(new AccessTokenResult(tokenString, expires));
        }

        private static Claim[] BuildClaims(JwtSubject s, DateTimeOffset now)
        {
            var claims = new System.Collections.Generic.List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, s.UserId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

            if (!string.IsNullOrWhiteSpace(s.Email))
                claims.Add(new Claim(ClaimTypes.Email, s.Email!));
            if (!string.IsNullOrWhiteSpace(s.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, s.PhoneNumber!));
            if (!string.IsNullOrWhiteSpace(s.TenantId))
                claims.Add(new Claim("TenantId", s.TenantId!));
            if (!string.IsNullOrWhiteSpace(s.StudentId))
                claims.Add(new Claim("StudentId", s.StudentId!));
            if (!string.IsNullOrWhiteSpace(s.CoachId))
                claims.Add(new Claim("CoachId", s.CoachId!));
            if (!string.IsNullOrWhiteSpace(s.AdminId))
                claims.Add(new Claim("AdminId", s.AdminId!));

            if (s.Roles is not null)
            {
                foreach (var role in s.Roles.Where(r => !string.IsNullOrWhiteSpace(r)))
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (s.ExtraClaims is not null)
            {
                foreach (var kv in s.ExtraClaims)
                {
                    if (!string.IsNullOrEmpty(kv.Key) && kv.Value is not null)
                        claims.Add(new Claim(kv.Key, kv.Value));
                }
            }

            return claims.ToArray();
        }
    }
}
