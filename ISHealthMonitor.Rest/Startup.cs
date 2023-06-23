using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ISHealthMonitor.Core.Implementations;
using Serilog;
using Serilog.Events;
using System;


namespace ISHealthMonitor.Rest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var dateTimeNowString = DateTime.Today.ToString("MM-dd-yyyy");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Conditional(
                    evt => evt.Level == LogEventLevel.Information,
                        wt => wt.File(@"Logs\Info-.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Conditional(
                    evt => evt.Level == LogEventLevel.Error,
                        wt => wt.File(@"Logs\Error-.log", rollingInterval: RollingInterval.Day))
                .WriteTo.File(@"Logs\All-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddControllers();
            services.AddSwaggerGen();
          //  services.AddTokenAuthentication(Configuration);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/rest/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
