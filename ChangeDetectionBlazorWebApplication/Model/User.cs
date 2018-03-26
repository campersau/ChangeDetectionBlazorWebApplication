using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChangeDetectionBlazorWebApplication.Model
{
    public class User : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void FirePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public User(string id)
        {
            Id = id;
        }

        public string Id { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; FirePropertyChanged(); }
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
