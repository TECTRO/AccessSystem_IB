using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccessSystem_IB.Data;
using AccessSystem_IB.Data.database;
using Microsoft.EntityFrameworkCore;
using Thread = System.Threading.Thread;

namespace AccessSystem_IB.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public App Global { get; } = (Application.Current as App);
        //public ApplicationContext Db = (Application.Current as App)?.DataBase;
        Settings Settings { get; }
        public AuthPage()
        {
            InitializeComponent();
            Settings = Settings.GetSettings();
        }

        void ShowErrorMessage(string message, bool collapseBlock = false)
        {
            Dispatcher.Invoke(() =>
            {

                ErrorBlock.Text = message;
                SuccessBlock.Visibility = Visibility.Collapsed;
                ErrorBlock.Visibility = Visibility.Visible;

                if (collapseBlock)
                {
                    new Task(() =>
                    {
                        Thread.Sleep(2500);
                        Dispatcher.Invoke(() => { ErrorBlock.Visibility = Visibility.Collapsed; });
                    }).Start();
                }
            });
        }

        void ShowSuccessMessage(string message, bool collapseBlock = false)
        {
            Dispatcher.Invoke(() =>
            {
                SuccessBlock.Text = message;
                SuccessBlock.Visibility = Visibility.Visible;
                ErrorBlock.Visibility = Visibility.Collapsed;

                if (collapseBlock)
                {
                    new Task(() =>
                    {
                        Thread.Sleep(2500);
                        Dispatcher.Invoke(() => { SuccessBlock.Visibility = Visibility.Collapsed; });
                    }).Start();
                }
            });
        }

        string TimespanToString(TimeSpan time)
        {
            StringBuilder result = new StringBuilder();

            if (time.Days > 0)
                result.Append($"{time.Days}д. ");

            if (time.Hours > 0)
                result.Append($"{time.Hours}ч. ");

            if (time.Minutes > 0)
                result.Append($"{time.Minutes}м. ");

            result.Append($"{time.Seconds}с. ");

            return result.ToString().TrimEnd();
        }

        void ShowBanStatus(DateTime bannedDate)
        {
            var token = new CancellationTokenSource();
            Task.Run(() =>
            {
                TimeSpan bannedTime;
                do
                {
                    bannedTime = bannedDate + Settings.BanTime - DateTime.Now;
                    Thread.Sleep(1000);

                    if (token.IsCancellationRequested)
                        return;
                    
                    ShowErrorMessage($"Вы заблокированы на {TimespanToString(bannedTime)}");
                } 
                while (bannedTime.Seconds > 0 && !token.IsCancellationRequested);
                
                Dispatcher.Invoke(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    ErrorBlock.Visibility = Visibility.Collapsed;
                });
            }, 
                token.Token);

            void Handler(object sender, TextChangedEventArgs args)
            {
                token.Cancel();
                Dispatcher.Invoke(() => { ErrorBlock.Visibility = Visibility.Collapsed; });
                LoginBox.TextChanged -= Handler;
            }

            LoginBox.TextChanged += Handler;
        }

        private List<Task> AuthTasks { get; } = new List<Task>();
        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var loginBox = LoginBox.Text;
            var passwordBox = PasswordBox.Password;

            AuthButton.IsEnabled = false;
            var authTask = new Task(async () =>
            {
                foreach (var task in AuthTasks)
                    await task;
                AuthTasks.Clear();

                using ApplicationContext Db = new ApplicationContext();

                var user = Db.Users
                    .Where(user1 => user1.Login == loginBox)
                    .Include(user1 => user1.AuthStory
                        .OrderByDescending(info => info.Date)
                        .Take(Settings.FailedLoginAttempt))
                    .FirstOrDefault();

                if (!(user is null))
                {
                    var lastBannedInfo = Db.UserAuthInfos
                        .OrderByDescending(info => info.Date)
                        .Include(info => info.User)
                        .FirstOrDefault(info => info.Status == LoginStatus.Banned && info.User.Login == user.Login);

                    var isAlreadyBanned = lastBannedInfo?.Date + Settings.BanTime > DateTime.Now;

                    if (!isAlreadyBanned)
                    {
                        bool isLoginSuccessful = user.Password == passwordBox;

                        var lastLoginAttempts = Db.UserAuthInfos
                            .OrderByDescending(info => info.Date)
                            .Include(info => info.User)
                            .Where(info => info.User.Login == user.Login)
                            .Take(Settings.FailedLoginAttempt - 1);

                        var isAttemptsExhausted = lastLoginAttempts
                            .All(info => info.Status == LoginStatus.Erred) &&
                                                  !isLoginSuccessful;


                        if (!isAttemptsExhausted)
                        {
                            if (isLoginSuccessful)
                            {
                                DataStore.Instance().CurrentUser = user;

                                ShowSuccessMessage("авторизация прошла успешно");
                            }
                            else
                            {
                                await Global.UpdateJournal($"Неудачная авторизация {user.Login}", JournalEvent.FailedLoginAttempt);
                                ShowErrorMessage($"Вы ввели неправильный пароль {lastLoginAttempts.ToList().TakeWhile(info => info.Status == LoginStatus.Erred).Count() + 1}р.");
                            }
                        }
                        else
                        {
                            ShowErrorMessage(
                                $"Вы ввели неправильный пароль {Settings.FailedLoginAttempt}р. и были забанены на {TimespanToString(Settings.BanTime)}");
                            ShowBanStatus( DateTime.Now);

                        }

                        user.AuthStory.Add(new UserAuthInfo
                        {
                            Date = DateTime.Now,
                            Status = isAttemptsExhausted ?
                                    LoginStatus.Banned : isLoginSuccessful ?
                                        LoginStatus.Succeed :
                                        LoginStatus.Erred
                        });

                        Db.SaveChanges();
                    }
                    else
                    {
                        ShowErrorMessage($"Вы заблокированы на {  TimespanToString(lastBannedInfo.Date + Settings.BanTime - DateTime.Now) }");
                        ShowBanStatus(lastBannedInfo.Date);
                    }
                }
                else
                    ShowErrorMessage("Вы ввели неправильный логин или пароль");

                Dispatcher.Invoke(() =>
                {
                    AuthButton.IsEnabled = true;
                });
            });
            AuthTasks.Add(authTask);
            authTask.Start();
        }
    }
}

