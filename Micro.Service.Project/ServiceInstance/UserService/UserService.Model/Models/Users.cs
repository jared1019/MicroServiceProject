using SqlSugar;

namespace UserService.Model.Models
{
    /// <summary>
    /// 用户表
    /// </summary>
    [SugarTable("tb_user", TableDescription = "用户表")]
    public class Users
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码;加密存储
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 注册手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// 密码加密的salt值
        /// </summary>
        public string Salt { get; set; }

    }
}
