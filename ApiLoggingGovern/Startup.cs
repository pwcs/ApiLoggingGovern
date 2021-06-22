using ApiLoggingGovern.SerilogEnrichers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiLoggingGovern
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
            services.AddHealthChecks().AddCheck<MemoryHealthCheck>("memory_check"); ; // Add health check services


            services.AddControllers();


            #region 注册Swagger服务
            services.AddSwaggerGen(options =>
            {
                typeof(ApiVersion).GetEnumNames().ToList().ForEach(version =>
                {
                    options.SwaggerDoc(version, new OpenApiInfo()
                    {
                        Version = version,
                        Title = $"webapi {version}",
                        Description = $"Asp.NetCore Web API {version}"
                    });
                });
                 

            });
            #endregion
            services.AddControllers(opts =>
            {
                opts.Filters.Add<SerilogLoggingPageFilter>();
                opts.Filters.Add<SerilogLoggingActionFilter>();
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    typeof(ApiVersion).GetEnumNames().ToList().ForEach(version =>
                    {
                        options.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
                    });
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest;
                //opts.GetLevel = LogHelper.ExcludeHealthChecks;
                opts.GetLevel = LogHelper.GetLevel(LogEventLevel.Debug, "Health checks");

            });
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/Ready").WithDisplayName("Not a health check");
                endpoints.MapHealthChecks("/healthy", new HealthCheckOptions()
                {
                    Predicate = s => s.Name.Equals("memory_check"),
                    ResponseWriter = WriteResponse
                });
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                }).AllowAnonymous();
            });
        }


        //指定返回格式
        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, p.Value))))))))));

            return context.Response.WriteAsync(
                json.ToString());
        }
    }
}
