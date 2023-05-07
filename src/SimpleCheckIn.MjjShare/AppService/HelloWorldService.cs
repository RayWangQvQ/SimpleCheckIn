using Microsoft.Extensions.Logging;
using Ray.DDD;
using Ray.Infrastructure.AutoTask;

namespace SimpleCheckIn.MjjShare.AppService;

[AutoTask("Hello", "测试")]
public class HelloWorldService : IAppService, IAutoTaskService
{
    private readonly ILogger<HelloWorldService> _logger;

    public HelloWorldService(ILogger<HelloWorldService> logger)
    {
        _logger = logger;
    }

    public async Task DoAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        _logger.LogInformation("Hello World!");
    }
}
