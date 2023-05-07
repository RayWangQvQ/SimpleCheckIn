using Refit;

namespace SimpleCheckIn.MjjShare.Agents
{
    public interface IMyApi
    {
        //[Post("/auth/login")]
        //Task<ApiResponse<GenericResponse>> LoginAsync([Body] LoginRequest request);
    }

    public class LoginRequest
    {
        public LoginRequest(string email, string pwd)
        {
            this.email = email;
            this.passwd = pwd;
        }

        public string email { get; set; }

        public string passwd { get; set; }

        public string code { get; set; }
    }

    public class GenericResponse
    {
        public int ret { get; set; }

        public string msg { get; set; }
    }
}
