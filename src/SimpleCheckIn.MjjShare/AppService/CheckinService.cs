using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ray.DDD;
using Ray.Infrastructure.Aop;
using Ray.Infrastructure.AutoTask;
using SimpleCheckIn.MjjShare.Configs;
using SimpleCheckIn.MjjShare.DomainService;

namespace SimpleCheckIn.MjjShare.AppService;

[AutoTask("Checkin", "签到")]
public class CheckinService : IAppService, IAutoTaskService
{
    private readonly TargetAccountManager<MyAccountInfo> _targetAccountManager;
    private readonly ILogger<LoginService> _logger;
    private readonly LoginDomainService _loginDomainService;
    private readonly SystemConfig _systemOptions;

    public CheckinService(
        TargetAccountManager<MyAccountInfo> targetAccountManager,
        ILogger<LoginService> logger,
        IOptions<SystemConfig> systemOptions,
        LoginDomainService loginDomainService
    )
    {
        _targetAccountManager = targetAccountManager;
        _logger = logger;
        _systemOptions = systemOptions.Value;
        _loginDomainService = loginDomainService;
    }

    public async Task DoAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("共{count}个账号", _targetAccountManager.Count);

        for (int i = 0; i < _targetAccountManager.Count; i++)
        {
            MyAccountInfo myAccount = _targetAccountManager.CurrentTargetAccount;

            try
            {
                await DoForAccountAsync(myAccount, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "签到异常");
            }

            if (_targetAccountManager.HasNext())
            {
                var sec = 30;
                _logger.LogInformation("睡个{sec}秒", sec);
                await Task.Delay(30 * 1000, cancellationToken);
                _targetAccountManager.MoveToNext();
            }
        }
    }

    [DelimiterInterceptor("账号签到", DelimiterScale.L)]
    private async Task DoForAccountAsync(MyAccountInfo myAccount, CancellationToken cancellationToken)
    {
        _logger.LogInformation("账号：{account}", myAccount.NickName);

        _logger.LogInformation("打开浏览器");
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
#if DEBUG
            Headless = false,
#else
            Headless = true,
#endif
        });
        var context = await browser.NewContextAsync();

        //加载状态
        _logger.LogInformation("加载历史状态");
        if (!string.IsNullOrWhiteSpace(myAccount.States))
        {
            var cookies = (JArray)JsonConvert.DeserializeObject<JObject>(myAccount.States)["cookies"];
            await context.AddCookiesAsync(cookies.ToObject<List<Cookie>>());
        }

        //新增tab页
        IPage page = await context.NewPageAsync();

        //访问并签到
        await CheckInAsync(myAccount, page, cancellationToken);
    }

    private async Task CheckInAsync(MyAccountInfo account, IPage page, CancellationToken cancellationToken)
    {
        await page.GotoAsync(_systemOptions.EntranceUrl);

        var loginLocator = page.GetByRole(AriaRole.Button, new() { Name = "登录！" });
        if (await loginLocator.CountAsync() > 0)
        {
            await _loginDomainService.LoginAsync(account, page, cancellationToken);
        }
        else
        {
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "进入" }).ClearAsync();

            var okLocator2 = page.GetByRole(AriaRole.Button, new() { Name = "OK" });
            if (await okLocator2.CountAsync() > 0)
            {
                await okLocator2.ClickAsync();
            }
        }

        _logger.LogInformation("开始签到");
        //var checkInLocator = page.GetByRole(AriaRole.Button, new() { Name = "签到" });
        var checkInLocator = page.Locator("#id-checkin");
        if (await checkInLocator.CountAsync() > 0)
        {
            await checkInLocator.ClickAsync();
            _logger.LogInformation("签到成功！");

            var getLocator = page.GetByText("获得");
            var list = await getLocator.AllTextContentsAsync();
            foreach (var item in list.ToList())
            {
                _logger.LogInformation(item);
            }
        }
        else
        {
            _logger.LogWarning("重复签到");
        }
    }

}
