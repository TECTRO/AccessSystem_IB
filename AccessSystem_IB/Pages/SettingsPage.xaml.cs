using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Accessibility;
using AccessSystem_IB.Annotations;
using AccessSystem_IB.Data;
using AccessSystem_IB.Data.database;

namespace AccessSystem_IB.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page, INotifyPropertyChanged
    {

        public Settings Settings { get; set; } = Settings.GetSettings();
        public int FailedLoginAttempt
        {
            get => Settings.FailedLoginAttempt;
            set
            {
                Settings.FailedLoginAttempt = value;
                OnPropertyChanged(nameof(FailedLoginAttempt));
            }
        }
        public TimeSpan AwaitKickTime
        {
            get => Settings.AwaitKickTime;
            set
            {
                Settings.AwaitKickTime = value;
                OnPropertyChanged(nameof(AwaitKickTime));
            }
        }

        public TimeSpan BanTime
        {
            get => Settings.BanTime;
            set
            {
                Settings.BanTime = value;
                OnPropertyChanged(nameof(BanTime));
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
            
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Save();
            SuccessBlock.Text = "Сохранение прошло успешно!";
            SuccessBlock.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                Thread.Sleep(2500);
                Dispatcher.Invoke(() =>
                {
                    SuccessBlock.Visibility = Visibility.Collapsed;
                });
            });

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
