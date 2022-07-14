using System.ComponentModel;

namespace UserService.Model.CustomEnums
{
    public enum SexEnum
    {
        /// <summary>
        /// 男
        /// </summary>
        [Description("男")]
        Male = 0,
        /// <summary>
        /// 女
        /// </summary>
        [Description("女")]
        Female = 1
    }
}
