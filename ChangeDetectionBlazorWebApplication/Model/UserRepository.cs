using System.Collections.ObjectModel;
using System.Linq;

namespace ChangeDetectionBlazorWebApplication.Model
{
    public class UserRepository
    {
        private readonly ObservableCollection<User> _users = new ObservableCollection<User>();

        public UserRepository()
        {
            _users.Add(new User("1") { Name = "I" });
            _users.Add(new User("2") { Name = "You" });
        }

        public ObservableCollection<User> GetUsers() => _users;

        public User FindUserById(string id) => _users.FirstOrDefault(u => u.Id == id);

    }
}
