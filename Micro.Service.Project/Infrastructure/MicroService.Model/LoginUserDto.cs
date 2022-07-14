namespace MicroService.Model
{
    public class LoginUserDto
    {
        public int UserId { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string? LoginName { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string? RealName { get; set; }

        // 性别
        public string? RoleName { get; set; }
    }
}
