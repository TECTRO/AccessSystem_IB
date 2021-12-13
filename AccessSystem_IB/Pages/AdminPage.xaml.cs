using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using AccessSystem_IB.Data;
using AccessSystem_IB.util;

namespace AccessSystem_IB.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();

            JournalPageHelper.GetInstance().CloseAppContextEvent += OnCloseAppContextEvent;
        }

        private void OnCloseAppContextEvent(bool newValue) => ExitButton.IsEnabled = newValue;

        public App Global { get; } = (Application.Current as App);

        //private List<Page> Pages { get; } = new List<Page>
        //{
        //    new JournalPage(),
        //    new UserCreationPage(),
        //    new SettingsPage()
        //};

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ButtonsView.SelectedIndex >= 0)
            {
                Global.ClearNavStack(ContentFrame.NavigationService);

                switch (ButtonsView.SelectedIndex)
                {
                    case 0: ContentFrame.NavigationService.Navigate(new Uri($"Pages\\{nameof(JournalPage)}.xaml", UriKind.RelativeOrAbsolute)); break;
                    case 1: ContentFrame.NavigationService.Navigate(new Uri($"Pages\\{nameof(UserCreationPage)}.xaml", UriKind.RelativeOrAbsolute)); break;
                    case 2: ContentFrame.NavigationService.Navigate(new Uri($"Pages\\{nameof(SettingsPage)}.xaml", UriKind.RelativeOrAbsolute)); break;
                }
            }
            //ContentFrame.NavigationService.Navigate(Pages[ButtonsView.SelectedIndex]);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DataStore.Instance().CurrentUser = null;
        }
    }
}
