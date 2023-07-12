using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using QPlay.Common.Settings;
using System.Threading.Tasks;

namespace QPlay.Common.Identity;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string AccessTokenParameter = "access_token";
    private const string MessageHubPath = "/messageHub";
    private readonly IConfiguration configuration;

    public ConfigureJwtBearerOptions(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void Configure(string name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            ServiceSettings serviceSettings = configuration
                .GetSection(nameof(ServiceSettings))
                .Get<ServiceSettings>();

            options.Authority = serviceSettings.Authority;
            options.Audience = serviceSettings.ServiceName;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role"
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    StringValues accessToken = context.Request.Query[AccessTokenParameter];
                    PathString path = context.HttpContext.Request.Path;

                    if (
                        !string.IsNullOrEmpty(accessToken)
                        && path.StartsWithSegments(MessageHubPath)
                    )
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        }
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}