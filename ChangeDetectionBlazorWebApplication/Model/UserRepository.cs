using System.Collections.ObjectModel;
using System.Linq;

namespace ChangeDetectionBlazorWebApplication.Model
{
    public class UserRepository
    {
        private readonly ObservableCollection<User> _users = new ObservableCollection<User>();
        private readonly ObservableCollection<UserGroup> _userGroups = new ObservableCollection<UserGroup>();

        public UserRepository()
        {
            _users.Add(new User() { Name = "I" });
            _users.Add(new User() { Name = "You" });

            _userGroups.Add(new UserGroup("1") { Name = "Administrator" });
            _userGroups.Add(new UserGroup("2") { Name = "User" });
        }

        public ObservableCollection<User> GetUsers() => _users;

        public ReadOnlyObservableCollection<UserGroup> GetUserGroups() => new ReadOnlyObservableCollection<UserGroup>(_userGroups);

        public User FindUserById(string id) => _users.FirstOrDefault(u => u.Id == id);

    }
}
