using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AccessSystem_IB.Data;
using AccessSystem_IB.Data.database;
using Microsoft.EntityFrameworkCore;

namespace AccessSystem_IB.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public App Global { get; } = (Application.Current as App);
        private Thread KickThread { get; }
        private bool CanStopKickThread { get; set; }
        public DataStore Data { get; set; } = DataStore.Instance();
        public UserPage()
        {
            InitializeComponent();

            KickThread = new Thread(async () =>
             {
                 while (!CanStopKickThread)
                 {
                     var canReturn = false;
                     Thread.Sleep(1000);
                     await AccessSystem_IB.Data.database.DbContext.GetInstance().SendAsync(async context =>
                     {
                         var journalEntry = context.Journal
                             .Include(entry => entry.User)
                             .Where(entry => entry.User.Login == DataStore.Instance().CurrentUser.Login)
                             .OrderByDescending(entry => entry.Date)
                             .Take(1)
                             .SingleOrDefault();

                         if (DateTime.Now - journalEntry?.Date > Settings.GetSettings().AwaitKickTime)
                         {
                             canReturn = true;
                             DataStore.Instance().CurrentUser = null;
                         }
                     });

                     if (canReturn) return;
                 }
             });

            KickThread.Start();
        }
        private void UserPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            (Application.Current as App)?.ClearNavStack(NavigationService);
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            await Global.UpdateJournal("Пользователь совершил действие", JournalEvent.UserAnyAction);
            MessageBox.Show("Кнопка нажата");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CanStopKickThread = true;
        }
    }
}
