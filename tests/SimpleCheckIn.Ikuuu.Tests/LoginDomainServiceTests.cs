using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Ray.Infrastructure.QingLong;
using SimpleCheckIn.Ikuuu.Configs;
using SimpleCheckIn.Ikuuu.DomainService;

namespace SimpleCheckIn.Ikuuu.Tests
{
    public class LoginDomainServiceTests
    {
        private const string StatesSample = @"
{
    ""cookies"":[
        {""name"":""lang"",""value"":""zh-cn"",""domain"":""ikuuu.eu"",""path"":""/"",""expires"":1.7169994E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""stel_ssid"",""value"":""67f6153b85bcdff123_6604636643520094934"",""domain"":""oauth.telegram.org"",""path"":""/"",""expires"":1.7142502E+09,""httpOnly"":true,""secure"":true,""sameSite"":""None""},
        {""name"":""csrftoken"",""value"":""QeMiOjY0pzNMa4imkf5S8SMhv0pzLxus3w4mU4H0pzZOlpvfmWk8GNmqfI07c4xw"",""domain"":""share.cjy.me"",""path"":""/"",""expires"":1.7139721E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""_ga"",""value"":""GA1.2.658723611.1682522468"",""domain"":"".ikuuu.eu"",""path"":""/"",""expires"":1.717689E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""uid"",""value"":""123456"",""domain"":""ikuuu.eu"",""path"":""/"",""expires"":1.6837338E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""email"",""value"":""test%40gmail.com"",""domain"":""ikuuu.eu"",""path"":""/"",""expires"":1.6837338E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""key"",""value"":""aea557bacd93333ab651933ab63a349b71583cf566cfd"",""domain"":""ikuuu.eu"",""path"":""/"",""expires"":1.6837338E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""ip"",""value"":""b5bc77c930d4456b8b330b4cfe6c826fb"",""domain"":""ikuuu.eu"",""path"":""/"",""expires"":1.6837338E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""expire_in"",""value"":""1683733746"",""domain"":""ikuuu.eu"",""path"":""/"",""expires"":1.6837338E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""_gid"",""value"":""GA1.2.16870228345.1683128950"",""domain"":"".ikuuu.eu"",""path"":""/"",""expires"":1.6832154E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""},
        {""name"":""_gat_gtag_UA_158704498_1"",""value"":""1"",""domain"":"".ikuuu.eu"",""path"":""/"",""expires"":1.683129E+09,""httpOnly"":false,""secure"":false,""sameSite"":""Lax""}
    ],
    ""origins"":[]
}";

        private LoginDomainService _target;

        private Mock<IHostEnvironment> _hostEnvironmentMock;
        private Mock<ILogger<LoginDomainService>> _loggerMock;
        private Mock<IOptions<IkuuuOptions>> _ikuuuOptionsMock;
        private Mock<IQingLongApi> _qinglongApiMock;

        public LoginDomainServiceTests()
        {
            _hostEnvironmentMock = new ();
            _loggerMock = new ();
            _ikuuuOptionsMock = new ();
            _qinglongApiMock = new ();

            _hostEnvironmentMock.Setup(x => x.ContentRootPath)
                .Returns(AppContext.BaseDirectory);

            _target = new LoginDomainService(_loggerMock.Object, _ikuuuOptionsMock.Object, _qinglongApiMock.Object, _hostEnvironmentMock.Object);
        }

        [Fact]
        public void SaveStatesToJson_Test()
        {
            var account = new MyAccountInfo()
            {
                States = StatesSample
            };
            _target.SaveStatesToJsonFile(account);
        }
    }
}