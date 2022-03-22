using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity.Setup
{
    public static class JwtConfig
    {
        public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // JWT
            AppConfig appSettings = GetAppSettings(services, configuration);
            // App
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            });

            services.AddAuthentication(x =>
            {

                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = TokenValidationParametersConfiguration(services, configuration);
            });

            return services;
        }

        public static TokenValidationParameters TokenValidationParametersConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("AppConfig");
            services.Configure<AppConfig>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppConfig>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, 
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidAudience = appSettings.ValidIn,
                ValidIssuer = appSettings.Issuer, 
                RequireExpirationTime = true,
            };
        }

        public static AppConfig GetAppSettings(IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("AppConfig");
            services.Configure<AppConfig>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppConfig>();
            return appSettings;
        }

        public static IApplicationBuilder UseAuthConfiguration(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }

}

