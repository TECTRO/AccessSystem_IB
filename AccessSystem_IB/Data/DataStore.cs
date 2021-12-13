using AccessSystem_IB.Data.database;

namespace AccessSystem_IB.Data
{
    public class DataStore
    {
        private DataStore() { }

        private static DataStore _instance;
        private User _currentUser;

        public static DataStore Instance()
        {
            _instance ??= new DataStore();
            return _instance;
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                UserChangedEvent?.Invoke(_currentUser, value);
                _currentUser = value;
            }
        }

        public delegate void UserChanged(User oldUser, User newUser);

        public event UserChanged UserChangedEvent;
    }
}