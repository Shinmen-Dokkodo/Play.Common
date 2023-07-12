using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace QPlay.Common.Identity;

public static class Extensions
{
    public static AuthenticationBuilder ConfigureAuthentication(this IServiceCollection services)
    {
        return services
            .ConfigureOptions<ConfigureJwtBearerOptions>()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
    }
}