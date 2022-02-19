using System;
using System.Text;
using Authentication.Api.Authentication;
using Authentication.Api.Handlers;
using Authentication.Core.Interfaces.Authentication;
using Authentication.Core.Models.Authentication;
using Authentication.Core.Utilities;
using Authentication.Services.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AppContext = Authentication.Data.AppContext;

namespace Authentication.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Authentication.Api", Version = "v1"});
            });
            services.AddDbContext<AppContext>(options => options.UseInMemoryDatabase(StringConstants.AuthenticationDb));
            services.AddDistributedMemoryCache();

            var authConfig = new AuthenticationConfiguration();
            var key = Encoding.ASCII.GetBytes(GetEnvironmentVariable("SIGNING_CREDENTIALS"));
            authConfig.SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            
            services.TryAddSingleton(authConfig);

            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = authConfig.SigningCredentials.Key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        CryptoProviderFactory = authConfig.SigningCredentials.CryptoProviderFactory
                    };
                });

            services.TryAddScoped<IApplicationUserResolver, WebApplicationUserResolver>();
            services.TryAddScoped<IAuthorizationHandler, IsValidUserHandler>();

            services.AddAuthorization(x =>
            {
                x.AddPolicy(nameof(IsValidUserRequirement), b => b.AddRequirements(new IsValidUserRequirement()));
            });
            
            services.TryAddScoped<UserAccountService>();
            services.TryAddScoped<AuthenticationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<ServiceExceptionHandler>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key) ??
                   throw new Exception($"Environment variable `{key}` not found");
        }
    }
}