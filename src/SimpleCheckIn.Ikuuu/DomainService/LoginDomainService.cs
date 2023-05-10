using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Ray.Infrastructure.AutoTask;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ray.DDD;
using Ray.Infrastructure.QingLong;
using SimpleCheckIn.Ikuuu.Configs;

namespace SimpleCheckIn.Ikuuu.DomainService
{
    public class LoginDomainService : IDomainService
    {
        private readonly IkuuuOptions _ikuuuOptions;
        private readonly ILogger<LoginDomainService> _logger;
        private readonly IQingLongApi _qingLongApi;
        private readonly IHostEnvironment _hostEnvironment;

        public LoginDomainService(
            ILogger<LoginDomainService> logger,
            IOptions<IkuuuOptions> ikuuuOptions,
            IQingLongApi qingLongApi,
            IHostEnvironment hostEnvironment
            )
        {
            _logger = logger;
            _qingLongApi = qingLongApi;
            _hostEnvironment = hostEnvironment;
            _ikuuuOptions = ikuuuOptions.Value;
        }

        public async Task LoginAsync(MyAccountInfo myAccount, IBrowserContext context, IPage page, CancellationToken cancellationToken)
        {
            _logger.LogInformation("填入邮箱：{email}",myAccount.UserName);
            var emailLocator = page.Locator("#email");
            await emailLocator.ClickAsync();
            await emailLocator.FillAsync(myAccount.UserName);

            _logger.LogInformation("填入密码：{pwd}", new string('*',myAccount.Pwd.Length));
            var pwdLocator = page.Locator("#password");
            await pwdLocator.ClickAsync();
            await pwdLocator.FillAsync(myAccount.Pwd);

            //await page.Locator("#remember-me").First.ClickAsync();
            await page.GetByText(new Regex("记住我|Remember Me")).ClickAsync();

            _logger.LogInformation("点击登录");
            var loginLocator = page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("登录|Login"), Exact = true });
            await loginLocator.First.ClickAsync();//todo:根据type过滤

            //todo:判断是否登录成功
            //_logger.LogInformation("等待重定向");
            //await Task.Delay(3 * 60 * 1000, cancellationToken);

            _logger.LogInformation("持久化账号状态");
            await SaveStatesAsync(myAccount, context, cancellationToken);
            _logger.LogInformation("持久化成功");
        }

        private async Task SaveStatesAsync(MyAccountInfo myAccount, IBrowserContext context, CancellationToken cancellationToken)
        {
            myAccount.States = await context.StorageStateAsync();
            _logger.LogDebug("States: {states}", myAccount.States);

            if (_ikuuuOptions.Platform.ToLower() == "qinglong")
            {
                _logger.LogInformation("尝试存储到青龙环境变量");
                await QingLongHelper.SaveStatesByUserNameAsync(_qingLongApi,
                    myAccount.UserName,$"{MyConst.EnvPrefix}Accounts", myAccount.States,  "States","UserName",
                    _logger, cancellationToken);
            }
            else
            {
                _logger.LogInformation("尝试存储到本地配置文件");
                SaveStatesToJsonFile(myAccount);
            }
        }

        public void SaveStatesToJsonFile(MyAccountInfo myAccount)
        {
            var pl = _hostEnvironment.ContentRootPath.Split("bin").ToList();
            pl.RemoveAt(pl.Count - 1);
            var path = Path.Combine(string.Join("bin", pl), "accounts.json");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{\"Accounts\":[]}");
            }

            var jsonStr = File.ReadAllText(path);

            dynamic jsonObj = JsonConvert.DeserializeObject(jsonStr);
            var accounts = (JArray)jsonObj["Accounts"];

            int index = accounts.IndexOf(accounts.FirstOrDefault(x =>
                x["States"].ToString().Contains(myAccount.GetUid()))
            );

            if (index >= 0)
            {
                accounts[index]["States"] = myAccount.States;
            }
            else
            {
                var n = new
                {
                    UserName = myAccount.UserName ?? "",
                    Pwd = myAccount.Pwd ?? "",
                    States = myAccount.States ?? ""
                };
                accounts.Add(JObject.FromObject(n));
            }

            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(path, output);
        }
    }
}
