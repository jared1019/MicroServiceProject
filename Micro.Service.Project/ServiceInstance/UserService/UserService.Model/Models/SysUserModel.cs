using MicroService.Model;
using SqlSugar;
using UserService.Model.CustomEnums;

namespace UserService.Model.Models
{
    [SugarTable("Sys_User")]
    public class SysUserModel : BaseModel
    {
        /// <summary>
        /// 登录账号
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? LoginName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? Pwd { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? RealName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string? Remark { get; set; }

        // 性别
        [SugarColumn(IsNullable = true)]
        public SexEnum Sex { get; set; } = SexEnum.Male;

        // 年龄
        [SugarColumn(IsNullable = true)]
        public int Age { get; set; }

        // 生日
        [SugarColumn(IsNullable = true)]
        public DateTime Birth { get; set; } = DateTime.Now;

        // 地址
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? Address { get; set; }

        [SugarColumn(IsNullable = true)]
        public bool IsDeleted { get; set; } = false;
    }
}
