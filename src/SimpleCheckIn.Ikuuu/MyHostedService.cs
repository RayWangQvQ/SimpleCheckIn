using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ray.Infrastructure.AutoTask;
using SimpleCheckIn.Ikuuu.Configs;

namespace SimpleCheckIn.Ikuuu;

public class MyHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<MyHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AutoTaskTypeFactory _autoTaskTypeFactory;

    public MyHostedService(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<MyHostedService> logger,
        IServiceProvider serviceProvider,
        AutoTaskTypeFactory autoTaskTypeFactory
    )
    {
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _autoTaskTypeFactory = autoTaskTypeFactory;
        //_accountManager.Init(_accountOptions.Select(x => new TargetAccountInfo(x.NickName, x.NickName)).ToList());
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await DoTaskAsync(cancellationToken);

        _logger.LogInformation("·开始推送·{task}", $"{_configuration["Run"]}任务");
        _hostApplicationLifetime.StopApplication();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

    }

    private async Task DoTaskAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var run = _configuration["Run"];

        var autoTaskInfo = _autoTaskTypeFactory.GetByCode(run);

        while (autoTaskInfo == null)
        {
            _logger.LogInformation("未指定目标任务，请选择要运行的任务：");
            _autoTaskTypeFactory.Show(_logger);
            _logger.LogInformation("请输入：");

            var index = Console.ReadLine();
            var suc = int.TryParse(index, out int num);
            if (!suc)
            {
                _logger.LogWarning("输入异常，请输入序号");
                continue;
            }

            autoTaskInfo = _autoTaskTypeFactory.GetByIndex(num);
        }

        _logger.LogInformation("目标任务：{run}", autoTaskInfo.ToString());

        var service = (IAutoTaskService)scope.ServiceProvider.GetRequiredService(autoTaskInfo.ImplementType);
        await service.DoAsync(cancellationToken);
    }
}
