using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure.Authentication;

internal sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly IConfiguration _configuration;

    public JwtBearerOptionsSetup(IOptions<AuthenticationOptions> authenticationOptions, IConfiguration configuration)
    {
        _authenticationOptions = authenticationOptions.Value;
        _configuration = configuration;
    }

    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _authenticationOptions.Audience;
        
        var useMockAuth = _configuration.GetValue<bool>("UseMockAuth");
        if (useMockAuth)
        {
            options.MetadataAddress = null;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _authenticationOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _authenticationOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretMockKeyForLocalDevelopmentOnlyDontUseInProduction123!")),
                ValidateLifetime = false
            };
        }
        else
        {
            options.MetadataAddress = _authenticationOptions.MetadataUrl;
            options.RequireHttpsMetadata = _authenticationOptions.RequireHttpsMetadata;
            options.TokenValidationParameters.ValidIssuer = _authenticationOptions.Issuer;
        }
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}