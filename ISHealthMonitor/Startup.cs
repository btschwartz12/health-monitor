using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using ISHealthMonitor.Core.Data.Contexts;
using ISHealthMonitor.Core.DataAccess;
using Microsoft.AspNetCore.Server.IISIntegration;
using ISHealthMonitor.Core.Implementations;
using ISHealthMonitor.UI.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.Logging;
using ISHealthMonitor.Core.Helpers.Cache;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;

namespace ISHealthMonitor
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }
        public ConfluenceAPI ConfluenceAPISettingsConfig { get; private set; } = new ConfluenceAPI();
        public IConfiguration Configuration { get; }
        private IHostEnvironment _env { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            if (_env.IsEnvironment("Local")) 
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "local";
                    options.DefaultChallengeScheme = "local";
                    options.DefaultScheme = "local";
                })
                .AddScheme<AuthenticationSchemeOptions, LocalAuthenticationHandler>("local", options => { })
                .AddJwtBearer("JWT", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["TokenConfig:Secret"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

                services.AddControllersWithViews();
            }
            else
            {

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    options.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddJwtBearer("JWT", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["TokenConfig:Secret"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                })
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));


                services.AddControllersWithViews(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                });

                services.AddRazorPages()
                    .AddMicrosoftIdentityUI();

            }

            services.AddTransient<IAuthorizationHandler, AdminRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.Requirements.Add(new AdminRequirement()));
            });


            // Cache
            services.AddSingleton<LogCache>();

            // Confluence
            Configuration.Bind("ConfluenceCloudApp", ConfluenceAPISettingsConfig);
            services.AddSingleton(ConfluenceAPISettingsConfig);

            // Db
            services.AddDbContext<IACMSEntityContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ISHealthCheckDatabase")));
            services.AddDbContext<DatawarehouseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DatawarehouseConnection")));

            // Interfaces
            services.AddTransient<IEmployee, Employee>();
            services.AddTransient<IHealthModel, HealthModel>();
            services.AddTransient<IRest, Rest>();
            services.AddTransient<ISplunkModel, SplunkModel>();


            services.AddSwaggerGen();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("Logs/{Date}.txt");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IS HealthCheck");
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}