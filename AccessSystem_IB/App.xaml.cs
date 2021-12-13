using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using AccessSystem_IB.Data;
using AccessSystem_IB.Data.database;
using AccessSystem_IB.Pages;
using Microsoft.EntityFrameworkCore;
using DbContext = AccessSystem_IB.Data.database.DbContext;
using JournalEntry = AccessSystem_IB.Data.database.JournalEntry;

namespace AccessSystem_IB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public ApplicationContext DataBase = new ApplicationContext();
        public App()
        {
            DataStore.Instance().UserChangedEvent += App_UserChangedEvent;
        }

        public void ClearNavStack(NavigationService nav)
        {
            while (nav.CanGoBack)
            {
                nav.RemoveBackEntry();
            }
        }

        private void App_UserChangedEvent(User oldUser, User newUser)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                if (MainWindow?.Content is DependencyObject host)
                {
                    var navigation = NavigationService.GetNavigationService(host);
                    switch (newUser)
                    {
                        case null:
                            {
                                await UpdateJournal($"{oldUser?.Login} вышел из аккаунта", JournalEvent.UserIsLoggedOut);

                                navigation?
                                    .Navigate(new Uri($"Pages\\{nameof(AuthPage)}.xaml",
                                        UriKind.RelativeOrAbsolute));
                            }
                            break;

                        default:
                            {
                                await UpdateJournal($"{newUser.Login} авторизован", JournalEvent.UserIsLoggedIn);

                                switch (newUser.Role)
                                {
                                    case Roles.User:
                                        navigation?.Navigate(
                                            new Uri($"Pages\\{nameof(UserPage)}.xaml",
                                                UriKind.RelativeOrAbsolute));
                                        break;

                                    case Roles.Admin:
                                        navigation?.Navigate(
                                            new Uri($"Pages\\{nameof(AdminPage)}.xaml",
                                                UriKind.RelativeOrAbsolute));
                                        break;
                                }
                            }
                            break;
                    }

                }
            });
        }

        public async Task UpdateJournal(string description, JournalEvent type, DateTime date = default)
        {
            var userLogin = DataStore.Instance().CurrentUser?.Login;
            //await using var dataBase = new ApplicationContext();
            await DbContext.GetInstance().SendAsync(async context =>
            {
                context.Journal.Add(new JournalEntry
                {
                    Date = date == default ? DateTime.Now : date,
                    User = context.Users.FirstOrDefault(user => user.Login == userLogin),
                    EventDescription = description,
                    EventType = type,
                    WorkstationName = Environment.MachineName
                });
                context.SaveChanges();
            });
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            //await using (var context = new ApplicationContext())
            //{
            //    var r = context.UserAuthInfos;
            //    var r1 = context.Journal;
            //    var r2 = context.Users;
            //}
            await UpdateJournal("Запуск приложения", JournalEvent.SystemStart);
        }
        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            await UpdateJournal("Выход из приложения", JournalEvent.SystemStop);
        }
    }
}
