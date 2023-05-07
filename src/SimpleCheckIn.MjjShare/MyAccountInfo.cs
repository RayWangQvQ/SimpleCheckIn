using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ray.Infrastructure.AutoTask;

namespace SimpleCheckIn.MjjShare
{
    public class MyAccountInfo : TargetAccountInfo
    {
        public MyAccountInfo() { }

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
            return this.UserName;
        }

        public string States { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MyAccountInfo other = (MyAccountInfo)obj;
            return this.UserName== other.UserName;
        }
    }
}
