using MicroService.Model;

namespace UserService.Model.ViewModel
{
    public class SysUserDto : BaseModel
    {
        public string uLoginName { get; set; }
        public string uLoginPWD { get; set; }
        public string uRealName { get; set; }
        public int uStatus { get; set; }
        public int DepartmentId { get; set; }
        public string uRemark { get; set; }
        public DateTime uCreateTime { get; set; } = DateTime.Now;
        public DateTime uUpdateTime { get; set; } = DateTime.Now;
        public DateTime uLastErrTime { get; set; } = DateTime.Now;
        public int uErrorCount { get; set; }
        public string name { get; set; }
        public int sex { get; set; } = 0;
        public int age { get; set; }
        public DateTime birth { get; set; } = DateTime.Now;
        public string addr { get; set; }
        public bool tdIsDelete { get; set; }
        public List<string> RoleNames { get; set; }
        public List<int> Dids { get; set; }
        public string DepartmentName { get; set; }
    }
}
