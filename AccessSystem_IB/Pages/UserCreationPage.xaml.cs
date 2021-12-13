using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AccessSystem_IB.Annotations;
using AccessSystem_IB.Data.database;
using Lab1_password_generator_ib;
using Microsoft.EntityFrameworkCore;
using DbContext = AccessSystem_IB.Data.database.DbContext;

namespace AccessSystem_IB.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserCreationPage.xaml
    /// </summary>
    public partial class UserCreationPage : Page, INotifyPropertyChanged
    {
        private string _newLogin = String.Empty;
        private string _newPassword = String.Empty;
        private Roles _newRole;

        public App Global { get; } = (Application.Current as App);
        //private ApplicationContext DataBase { get; } = (Application.Current as App)?.DataBase;
        private PasswordGenerator Generator { get; } = new PasswordGenerator(new SecureCharProvider());
        public String NewLogin
        {
            get => _newLogin;
            set
            {
                _newLogin = value;
                OnPropertyChanged(nameof(NewLogin));
            }
        }

        public String NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        public Roles NewRole
        {
            get => _newRole;
            set
            {
                _newRole = value;
                OnPropertyChanged(nameof(NewRole));
            }
        }

        public ObservableCollection<Roles> UserRoles { get; set; } = new ObservableCollection<Roles>(typeof(Roles).GetEnumValues().Cast<Roles>());

        public UserCreationPage()
        {
            InitializeComponent();

            DbContext.GetInstance().Send(async context =>
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                var users = context.Users.ToList();
                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var user in users)
                        Users.Add(user);
                });

            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NewPassword = Generator.Generate(NewLogin);
            }
            catch (Exception exception)
            {
                ShowErrorMessage(exception.Message);
            }
        }

        private void ShowErrorMessage(string message, int delay = 2500)
        {
            ErrorBlock.Text = message;
            ErrorBlock.Visibility = Visibility.Visible;

            Task.Run(() =>
            {
                Thread.Sleep(delay); Dispatcher.Invoke(() =>
                {
                    ErrorBlock.Visibility = Visibility.Collapsed;
                });
            });
        }

        private void ShowSuccessMessage(string message, int delay = 2500)
        {
            ErrorBlock.Visibility = Visibility.Collapsed;
            SuccessBlock.Text = message;
            SuccessBlock.Visibility = Visibility.Visible;

            Task.Run(() =>
            {
                Thread.Sleep(delay); Dispatcher.Invoke(() =>
                {
                    SuccessBlock.Visibility = Visibility.Collapsed;
                });
            });
        }


        private async void CreateUserButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (usersBox.SelectedItem == null)
                {
                    if (!Generator.CheckLogin(NewLogin) || !Generator.CheckPassword(NewLogin, NewPassword) ||
                        NewPassword.Length != Generator.PasswordCount)
                    {
                        ShowErrorMessage("Параметры пользователя введены некорректно");
                        return;
                    }

                    await DbContext.GetInstance().SendAsync(async context =>
                    {
                        await context.Users.AddAsync(new User
                        {
                            Login = NewLogin,
                            Password = NewPassword,
                            Role = NewRole
                        });
                        await context.SaveChangesAsync();

                        var users = context.Users.ToList();
                        await Dispatcher.InvokeAsync(() =>
                        {
                            Users.Clear();
                            foreach (var user in users)
                                Users.Add(user);
                        });
                    });


                    await Global.UpdateJournal($"Создан пользователь {NewLogin} ({NewRole})", JournalEvent.CreateUserAccount);
                    ShowSuccessMessage($"Пользователь {NewLogin} ({NewRole}) успешно добавлен!");
                }
                else
                {

                    var login = ((User)usersBox.SelectedItem).Login;
                    await DbContext.GetInstance().SendAsync(async context =>
                    {
                        var user =
                            context
                            .Users
                            .Where(user1 => user1.Login == login)
                            .Take(1).FirstOrDefault();

                        if (user != null)
                        {
                            user.Login = NewLogin;
                            user.Password = NewPassword;
                            user.Role = NewRole;
                        }

                        var users = context.Users.ToList();
                        await Dispatcher.InvokeAsync(() =>
                        {
                            Users.Clear();
                            foreach (var user in users)
                                Users.Add(user);
                        });

                        await context.SaveChangesAsync();
                    });

                    await Global.UpdateJournal($"Изменен пользователь {NewLogin} ({NewRole})", JournalEvent.UpdateUserAccount);
                    ShowSuccessMessage($"Пользователь {NewLogin} ({NewRole}) успешно Изменен!");
                }

                NewLogin = String.Empty;
                NewPassword = String.Empty;
                NewRole = Roles.User;
                usersBox.SelectedIndex = -1;

            }
            catch (Exception exception)
            {
                ShowErrorMessage(exception.Message, 5000);
            }
        }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        private void SelectNewButton_OnClick(object sender, RoutedEventArgs e)
        {
            usersBox.SelectedIndex = -1;
        }

        private void UsersBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (usersBox.SelectedItem != null)
            {
                var user = (User)usersBox.SelectedItem;
                NewLogin = user.Login;
                NewPassword = user.Password;
                NewRole = user.Role;

                CreateUserButton.Content = "Изменить";
                DeleteUserButton.Visibility = Visibility.Visible;
            }
            else
            {
                NewLogin = String.Empty;
                NewPassword = String.Empty;
                NewRole = Roles.User;

                CreateUserButton.Content = "Создать";
                DeleteUserButton.Visibility = Visibility.Collapsed;

            }
        }

        private async void DeleteUserButton_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteUserButton.IsEnabled = false;
            var login = ((User)usersBox.SelectedItem).Login;
            await DbContext.GetInstance().SendAsync(async context =>
            {
                var user =
                    context
                        .Users
                        .Include(user1 => user1.JournalEntries)
                        .Where(user1 => user1.Login == login)
                        .Take(1).FirstOrDefault();

                if (user != null)
                {
                    context.Users.Remove(user);
                    await context.SaveChangesAsync();

                    var users = context.Users.ToList();
                    await Dispatcher.InvokeAsync(() =>
                    {
                        Users.Clear();
                        foreach (var user in users)
                            Users.Add(user);
                        DeleteUserButton.IsEnabled = true;

                    });
                }
            });
        }
    }
}
