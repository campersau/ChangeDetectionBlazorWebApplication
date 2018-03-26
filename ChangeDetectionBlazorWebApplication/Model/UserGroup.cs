namespace ChangeDetectionBlazorWebApplication.Model
{
    public class UserGroup : BaseModel
    {
        public UserGroup(string id)
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
            return Name;
        }
    }
}
