using SqlSugar;

namespace MicroService.Model
{
    public class BaseModel : TopBaseModel
    {
        /// <summary>
        /// 创建人id
        /// </summary>
        [SugarColumn(ColumnName = "")]
        public int? CreateUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新人Id
        /// </summary>
        public int? UpdateUserId { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
