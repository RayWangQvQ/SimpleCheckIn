using Refit;

namespace SimpleCheckIn.Ikuuu.Agents
{
    public interface IIkuuuApi
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/auth/login")]
        Task<ApiResponse<GenericResponse>> LoginAsync([Body(BodySerializationMethod.UrlEncoded)] LoginRequest request);

        /// <summary>
        /// 签到
        /// </summary>
        /// <returns></returns>
        [Post("/user/checkin")]
        Task<ApiResponse<GenericResponse>> CheckinAsync();
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
