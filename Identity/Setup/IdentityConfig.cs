using Identity.Data;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Setup
{
    public static class IdentityConfig
    {
        public static void AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // User Identity
            services.AddDefaultIdentity<ApplicationUser>()
              .AddRoles<IdentityRole>()
              .AddEntityFrameworkStores<DataContext>()
              .AddDefaultTokenProviders();

            services.AddJwtConfiguration(configuration);
        }

    }
}
