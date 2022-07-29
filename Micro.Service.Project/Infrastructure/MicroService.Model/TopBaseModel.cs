using SqlSugar;

namespace MicroService.Model
{
    public class TopBaseModel
    {
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "主键")]
        public int Id { get; set; }
    }
}
