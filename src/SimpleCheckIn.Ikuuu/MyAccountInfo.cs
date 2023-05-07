using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ray.Infrastructure.AutoTask;

namespace SimpleCheckIn.Ikuuu
{
    public class MyAccountInfo : TargetAccountInfo
    {
        public MyAccountInfo()
        {

        }

        public MyAccountInfo(string userName, string pwd) : base(userName, pwd)
        {
        }

        private string _nickName;

        public string NickName
        {
            get => string.IsNullOrWhiteSpace(_nickName)
                ? GetNickName(this.UserName)
                : _nickName;
            set => _nickName = value;
        }

        private string GetNickName(string userName)
        {
            return userName.Split("@").ToList().First();
        }

        public string States { get; set; }

        public string GetUid()
        {
            dynamic stateObj = JsonConvert.DeserializeObject(States);
            var ckList = (JArray)stateObj["cookies"];
            var ck = ckList.FirstOrDefault(x => x["name"].ToString() == "uid");
            var uid = ck["value"].ToString();
            return uid;
        }
    }
}
