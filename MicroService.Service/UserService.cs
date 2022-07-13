using MicroService.Interface;
using MicroService.Model;

namespace MicroService.Service
{
    public class UserService: IUserService
    {
        #region DataInit
        private List<User> _UserList = new List<User>()
        {
            new User()
            {
                Id=1,
                Account="Administrator",
                Email="57265177@qq.com",
                Name="张三",
                Password="1234567890",
                LoginTime=DateTime.Now,
                Role="Admin"
            },
             new User()
            {
                Id=1,
                Account="LISI",
                Email="57265177@qq.com",
                Name="李四",
                Password="1234567890",
                LoginTime=DateTime.Now,
                Role="Admin"
            },
              new User()
            {
                Id=1,
                Account="WANGWU",
                Email="57265177@qq.com",
                Name="王五",
                Password="1234567890",
                LoginTime=DateTime.Now,
                Role="Admin"
            },
        };
        #endregion

        public User FindUser(int id)
        {
            return this._UserList.Find(u => u.Id == id);
        }

        public IEnumerable<User> UserAll()
        {
            return this._UserList;
        }
    }
}
