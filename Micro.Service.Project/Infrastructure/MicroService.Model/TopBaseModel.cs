using SqlSugar;

namespace MicroService.Model
{
    public class TopBaseModel
    {
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
    }
}
