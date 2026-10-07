using System.Text;
using Mealthy.Api.Security.Application.Contracts;
using Mealthy.Api.Security.Application.Services;
using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Domain.Repositories;
using Mealthy.Api.Security.Infraestructure.Auth;
using Mealthy.Api.Security.Infraestructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Mealthy.Api.Security;

public static class SecurityModule
{
    public static IServiceCollection AddSecurityModule(this IServiceCollection services, IConfiguration config)
    {
        // Falla al arrancar si falta el secreto o es corto (mejor que un 500 en el primer login).
        services.AddOptions<JwtOptions>()
            .Bind(config.GetSection(JwtOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwt = config.GetSection(JwtOptions.Section).Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;   // conserva "sub", "role"... tal cual vienen en el token
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key ?? string.Empty)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.UniqueName,
                    RoleClaimType = SecurityClaims.Role
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.CustomerOnly, p => p.RequireRole(nameof(Role.Customer)))
            .AddPolicy(AuthPolicies.StoreOwnerOnly, p => p.RequireRole(nameof(Role.StoreOwner)))
            .AddPolicy(AuthPolicies.AdminOnly, p => p.RequireRole(nameof(Role.Admin)));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();

        return services;
    }
}
