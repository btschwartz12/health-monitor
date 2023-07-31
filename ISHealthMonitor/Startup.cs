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

namespace ISHealthMonitor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public ConfluenceAPI ConfluenceAPISettingsConfig { get; private set; } = new ConfluenceAPI(); 
        public IConfiguration Configuration { get; }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSwaggerGen();


			//         services.AddAuthentication(sharedOptions =>
			//         {
			//             sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			//             sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			//             sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
			//         })
			//         .AddWsFederation(wsFedOptions =>
			//         {
			//             //RP realm – normally this is the client FQDN unless a realm is given to the RP by the STS
			//             wsFedOptions.Wtrealm = "spn:_b0144165-412d-46e4-87ad-c62f32c0a7c0";
			//             //url to sts metadata
			//             wsFedOptions.MetadataAddress = "https://login.microsoftonline.com/8ca5db88-a5ab-48f7-a5e0-4ce50935f807/federationmetadata/2007-06/federationmetadata.xml?appid=_b0144165-412d-46e4-87ad-c62f32c0a7c0";
			//	wsFedOptions.CallbackPath = "/Federation";

			//})
			//         .AddCookie(cookieOptions =>
			//         {
			//             cookieOptions.Cookie.Name = "FedAuth"; //the name of the cookie you wish to use
			//             cookieOptions.Cookie.HttpOnly = true; //indicates the cookie can not be accessed by client scripts

			//         });

			services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

			services.AddTokenAuthentication(Configuration);

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
