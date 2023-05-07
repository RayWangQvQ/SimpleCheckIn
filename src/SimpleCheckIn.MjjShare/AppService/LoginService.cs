using SimpleCheckIn.MjjShare.Configs;
using SimpleCheckIn.MjjShare.DomainService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ray.DDD;
using Ray.Infrastructure.Aop;
using Ray.Infrastructure.AutoTask;
using Volo.Abp.DependencyInjection;

namespace SimpleCheckIn.MjjShare.AppService;

//[AutoTask("Login", "登录")]
public class LoginService : IAppService, IAutoTaskService
{
    private readonly ILogger<LoginService> _logger;
    private readonly LoginDomainService _loginDomainService;
    private readonly SystemConfig _systemOptions;

    public LoginService(
        IConfiguration configuration,
        ILogger<LoginService> logger,
        IOptions<SystemConfig> systemOptions,
        LoginDomainService loginDomainService
        )
    {
        _logger = logger;
        _loginDomainService = loginDomainService;
        _systemOptions = systemOptions.Value;
    }

    public async Task DoAsync(CancellationToken cancellationToken)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();

        await page.GotoAsync(_systemOptions.EntranceUrl);

        await _loginDomainService.LoginAsync(new MyAccountInfo(), page, cancellationToken);
    }


}
