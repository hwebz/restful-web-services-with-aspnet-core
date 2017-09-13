using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityService.UserServices
{
    public class UserRepository : IUserRepository
    {
        private readonly List<CustomUser> _users = new List<CustomUser>
        {
            new CustomUser
            {
                SubjectId = "123",
                UserName = "postmanAdmin",
                Password = "postmanAdmin",
                Email = "postmanAdmin@email.ch"
            },
            new CustomUser
            {
                SubjectId = "456",
                UserName = "postmanUser",
                Password = "postmanUser",
                Email = "postmanUser@email.ch"
            }
        };

        public bool ValidateCredentials(string username, string password)
        {
            var user = FindByUsername(username);
            return user != null && user.Password.Equals(password);
        }

        public CustomUser FindBySubjectId(string subjectId)
        {
            return _users.FirstOrDefault(x => x.SubjectId == subjectId);
        }

        public CustomUser FindByUsername(string username)
        {
            return _users.FirstOrDefault(x => x.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }
}
