using SimpleCheckIn.MjjShare.Agents;
using SimpleCheckIn.MjjShare.AppService;
using SimpleCheckIn.MjjShare.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ray.DDD;
using Ray.Infrastructure;
using Ray.Infrastructure.AutoTask;
using Ray.Infrastructure.Http;
using Ray.Infrastructure.QingLong;
using Ray.Serilog.Sinks.PushPlusBatched;
using Ray.Serilog.Sinks.ServerChanBatched;
using Ray.Serilog.Sinks.TelegramBatched;
using Ray.Serilog.Sinks.WorkWeiXinBatched;
using Refit;
using Serilog;
using Serilog.Events;

namespace SimpleCheckIn.MjjShare;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var exitCode = Microsoft.Playwright.Program.Main(new string[] { "install", "--with-deps", "chromium" });
        if (exitCode != 0)
        {
            throw new Exception($"Playwright exited with code {exitCode}");
        }

        Log.Logger = CreateLogger(args);
        try
        {
            Log.Logger.Information("Installing browser.");
            InstallBrowser();

            Log.Logger.Information("Starting console host.");
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    IList<IConfigurationSource> list = configurationBuilder.Sources;
                    list.ReplaceWhile(
                        configurationSource => configurationSource is EnvironmentVariablesConfigurationSource,
                        new EnvironmentVariablesConfigurationSource()
                        {
                            Prefix = MyConst.EnvPrefix
                        }
                    );

                    configurationBuilder.AddJsonFile("accounts.json", true, true);
                })
                .ConfigureServices(RegisterServices)
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    var list = services.Where(x => x.ServiceType == typeof(IAutoTaskService))
                        .Select(x => x.ImplementationType)
                        .ToList();
                    var autoTaskTypeFactory = new AutoTaskTypeFactory(list);
                    services.AddSingleton(autoTaskTypeFactory);
                })
                .UseSerilog().UseConsoleLifetime().Build();

            RayGlobal.ServiceProviderRoot = host.Services;

            await host.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            Log.Logger.Fatal("·开始推送·{task}·{user}", "任务异常", "");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static ILogger CreateLogger(string[] args)
    {
        var hb = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                IList<IConfigurationSource> list = configurationBuilder.Sources;
                list.ReplaceWhile(
                    configurationSource => configurationSource is EnvironmentVariablesConfigurationSource,
                    new EnvironmentVariablesConfigurationSource()
                    {
                        Prefix = MyConst.EnvPrefix
                    }
                );
            });
        var tempHost = hb.Build();
        var config = tempHost.Services.GetRequiredService<IConfiguration>();

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File($"Logs/{DateTime.Now:yyyy-MM-dd}/{DateTime.Now:HH-mm-ss}.txt",
                restrictedToMinimumLevel: LogEventLevel.Debug
            )
            //.WriteTo.Async(c =>
            //{
            //    c.File($"Logs/{DateTime.Now:yyyy-MM-dd}/{DateTime.Now:HH-mm-ss}.txt",
            //        restrictedToMinimumLevel: LogEventLevel.Debug);
            //})
            .WriteTo.Console()
            .WriteTo.PushPlusBatched(
                config["Notify:PushPlus:Token"],
                config["Notify:PushPlus:Channel"],
                config["Notify:PushPlus:Topic"],
                config["Notify:PushPlus:Webhook"],
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            .WriteTo.TelegramBatched(
                config["Notify:Telegram:BotToken"],
                config["Notify:Telegram:ChatId"],
                config["Notify:Telegram:Proxy"],
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            .WriteTo.ServerChanBatched(
                "",
                turboScKey: config["Notify:ServerChan:TurboScKey"],
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            .WriteTo.WorkWeiXinBatched(
                config["Notify:WorkWeiXin:WebHookUrl"],
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            .CreateLogger();
    }

    private static void InstallBrowser()
    {
        var exitCode = Microsoft.Playwright.Program.Main(new string[] { "install", "--with-deps", "chromium" });
        if (exitCode != 0)
        {
            throw new Exception($"Playwright exited with code {exitCode}");
        }
    }

    private static void RegisterServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var config = (IConfigurationRoot)hostBuilderContext.Configuration;

        services.AddHostedService<MyHostedService>();

        #region Accounts

        services.Configure<List<MyAccountInfo>>(config.GetSection("Accounts"));
        services.Configure<List<TargetAccountInfo>>(config.GetSection("Accounts"));
        services.AddSingleton(typeof(TargetAccountManager<>));

        #endregion

        #region config
        services.Configure<HttpClientCustomOptions>(config.GetSection("HttpCustomConfig"));
        services.Configure<SystemConfig>(config.GetSection("SystemConfig"));
        #endregion

        #region Api
        services.AddTransient<DelayHttpMessageHandler>();
        services.AddTransient<LogHttpMessageHandler>();
        services.AddTransient<ProxyHttpClientHandler>();
        services.AddTransient<CookieHttpClientHandler<MyAccountInfo>>();
        services
            .AddRefitClient<IMyApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://api.bilibili.com");

                var ua = config["UserAgent"];
                if (!string.IsNullOrWhiteSpace(ua))
                    c.DefaultRequestHeaders.UserAgent.ParseAdd(ua);
            })
            .AddHttpMessageHandler<DelayHttpMessageHandler>()
            .AddHttpMessageHandler<LogHttpMessageHandler>()
            .ConfigurePrimaryHttpMessageHandler<ProxyHttpClientHandler>()
            .ConfigurePrimaryHttpMessageHandler<CookieHttpClientHandler<MyAccountInfo>>()
            ;
        services.AddQingLongRefitApi();
        #endregion

        #region AppService
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.AssignableTo<IAutoTaskService>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithTransientLifetime()
        );
        #endregion

        #region DomainService
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.AssignableTo<IDomainService>())
            //.AsImplementedInterfaces()
            .AsSelf()
            .WithTransientLifetime()
        );
        #endregion
    }
}