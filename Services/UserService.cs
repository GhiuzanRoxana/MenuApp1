using MenuApp.Models;
using System;

namespace MenuApp.Services
{
    public enum UserType
    {
        Guest,
        Client,
        Employee
    }

    public class UserService
    {
        private static UserService? _instance;
        private User? _currentUser;

        public static UserService Instance => _instance ??= new UserService();

        private UserService()
        {
            UserChanged = delegate { };
            AuthenticationStatusChanged = delegate { };
        }

        public User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                bool wasLoggedIn = IsAuthenticated;
                _currentUser = value;
                UserChanged?.Invoke(this, EventArgs.Empty);
                if (wasLoggedIn != IsAuthenticated)
                {
                    AuthenticationStatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsAuthenticated => CurrentUser != null;
        public bool IsEmployee => CurrentUser?.IsEmployee ?? false;
        public bool IsClient => IsAuthenticated && !IsEmployee;

        public UserType CurrentUserType
        {
            get
            {
                if (!IsAuthenticated)
                    return UserType.Guest;
                else if (IsEmployee)
                    return UserType.Employee;
                else
                    return UserType.Client;
            }
        }

        public event EventHandler UserChanged;
        public event EventHandler AuthenticationStatusChanged;

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}