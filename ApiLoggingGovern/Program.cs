using ApiLoggingGovern.SerilogEnrichers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiLoggingGovern
{
    public class Program
    {

        private static string LogFilePath(string LogEvent) => $@"Logs\{LogEvent}\log-.log";
        private static string SerilogOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({Application}/{MachineName}/{ThreadId}) {Message}{NewLine}{Exception}";

        private static IConfiguration ConfigurationSerilog = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
         .Build();

        private static string connectionString =
      @"User Id=username;Password=password;Data Source==(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL = TCP)(HOST = 172.100.2.42)(PORT = 1521)))(CONNECT_DATA=(SERVICE_NAME = orcl)))";

        public static void Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                     .ReadFrom.Configuration(ConfigurationSerilog)

                     .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Oracle(cfg =>
                            cfg.WithSettings(connectionString, "SerilogLOG", tableSpaceAndFunctionName: "USERS")
                            .UseBurstBatch(batchLimit: 1)
                            .CreateSink()))
                     
                     .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Async(a => a.File(LogFilePath("Debug"), outputTemplate: SerilogOutputTemplate, restrictedToMinimumLevel: LogEventLevel.Debug, rollingInterval: RollingInterval.Hour, retainedFileCountLimit: null, shared: true)))
                     
                     .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Async(a => a.File(LogFilePath("Information"), outputTemplate: SerilogOutputTemplate, restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour, retainedFileCountLimit: null, shared: true)))

                     .CreateLogger();

                Log.Information("No one listens to me!");
                Log.Warning("No one listens to me! Warning");
                Log.Debug("No one listens to me! Debug");
                Log.Error("No one listens to me! Error");
                Log.Fatal("No one listens to me! Fatal");

                Log.ForContext(Constants.SourceContextPropertyName, "Microsoft").Warning("Hello, world!");
                Log.ForContext(Constants.SourceContextPropertyName, "Microsoft").Error("Hello, world!");
                Log.ForContext(Constants.SourceContextPropertyName, "MyApp.Something.Tricky").Verbose("Hello, world!");


                Log.Information("Destructure with max object nesting depth:\n{@NestedObject}",
              new { FiveDeep = new { Two = new { Three = new { Four = new { Five = "the end" } } } } });

                Log.Information("Destructure with max string length:\n{@LongString}",
                    new { TwentyChars = "0123456789abcdefghij" });

                Log.Information("Destructure with max collection count:\n{@BigData}",
                    new { TenItems = new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" } });

                Log.Information("Destructure with policy to strip password:\n{@LoginData}",
                    new LoginData { Username = "BGates", Password = "isityearoflinuxyet" });

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "主机意外终止");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args)
               .UseSerilog((context, services, configuration) => configuration
                       .ReadFrom.Configuration(context.Configuration)
                       .ReadFrom.Services(services)
                   .Enrich.FromLogContext()
               .Enrich.WithRequestInfo()
               
               .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly((e) => { var context = e.Properties.ContainsKey("RequestPath") ? e.Properties["RequestPath"].ToString().ToLower() : e.Properties["SourceContext"].ToString().ToLower(); return context.Contains("health"); }).WriteTo.Async(a => a.File(LogFilePath("health"), outputTemplate: SerilogOutputTemplate, restrictedToMinimumLevel: LogEventLevel.Debug, rollingInterval: RollingInterval.Hour, retainedFileCountLimit: null, shared: true)))
               
               .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Async(a => a.File(LogFilePath("Debug"), outputTemplate: SerilogOutputTemplate, restrictedToMinimumLevel: LogEventLevel.Debug, rollingInterval: RollingInterval.Hour, retainedFileCountLimit: null, shared: true)))
               
               .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Async(a => a.File(LogFilePath("Information"), outputTemplate: SerilogOutputTemplate, restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour, retainedFileCountLimit: null, shared: true)))
               
               .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Oracle(cfg =>
                       cfg.WithSettings(connectionString, "SerilogLOG", tableSpaceAndFunctionName: "USERS")
                       .UseBurstBatch(batchLimit: 1)
                       .CreateSink()))
                        )

                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseStartup<Startup>();
                  })
              ;
    }
    public class CustomFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return true;
        }
    }

    public class LoginData
    {
        public string Username;
        public string Password;
    }

    public class CustomPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            result = null;

            if (value is LoginData)
            {
                result = new StructureValue(
                    new List<LogEventProperty>
                    {
                        new LogEventProperty("Username", new ScalarValue(((LoginData)value).Username)),
                        new LogEventProperty("Password", new ScalarValue(((LoginData)value).Password+"---"))
                    }
                    );
            }

            return (result != null);
        }
    }
}
