﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ray.Infrastructure.Aop;
using Ray.Infrastructure.AutoTask;
using SimpleCheckIn.Ikuuu.Configs;
using Volo.Abp.DependencyInjection;
using SimpleCheckIn.Ikuuu.DomainService;

namespace SimpleCheckIn.Ikuuu.AppService;

[AutoTask("Hello", "测试")]
public class HelloWorldService : ITransientDependency, IAutoTaskService
{
    private readonly IConfiguration _configuration;
    private readonly TargetAccountManager<MyAccountInfo> _targetAccountManager;
    private readonly ILogger<HelloWorldService> _logger;
    private readonly LoginDomainService _loginDomainService;
    private readonly IkuuuOptions _ikuuuOptions;

    public HelloWorldService(
        IConfiguration configuration,
        TargetAccountManager<MyAccountInfo> targetAccountManager,
        ILogger<HelloWorldService> logger,
        IOptions<List<MyAccountInfo>> accountOptions,
        IOptions<IkuuuOptions> ikuuuOptions,
        LoginDomainService loginDomainService
        )
    {
        _configuration = configuration;
        _targetAccountManager = targetAccountManager;
        _logger = logger;
        _loginDomainService = loginDomainService;
        _ikuuuOptions = ikuuuOptions.Value;
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
        _logger.LogInformation("访问{url}", _ikuuuOptions.EntranceUrl);
        await page.GotoAsync(_ikuuuOptions.EntranceUrl);

        var loginLocator = page.GetByRole(AriaRole.Button, new() { Name = "登录", Exact = true });
        if (await loginLocator.CountAsync() > 0)
        {
            _logger.LogInformation("检测到未登录，开始登录");
            await _loginDomainService.LoginAsync(account, page, cancellationToken);
        }

        _logger.LogInformation("检测到已登录");

        var readLocator = page.GetByRole(AriaRole.Button, new() { Name = "Read" });
        if (await readLocator.CountAsync() > 0)
        {
            _logger.LogInformation("阅读公告");
            await readLocator.ClickAsync();
        }

        var checkInLocator = page.GetByRole(AriaRole.Link, new() { Name = "每日签到" });
        if (await checkInLocator.CountAsync() > 0)
        {
            _logger.LogInformation("开始签到");
            await checkInLocator.ClickAsync();

            var getLocator = page.GetByText("获得");
            var list = await getLocator.AllTextContentsAsync();
            foreach (var item in list.ToList())
            {
                _logger.LogInformation(item);
            }

            //await page.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        }
        else if (await page.GetByRole(AriaRole.Link, new() { Name = "明日再来" }).CountAsync() > 0)
        {
            _logger.LogInformation("已签到，明日再来");
        }
        else
        {
            _logger.LogWarning("异常，请自行检查签到状态");
        }
    }
}