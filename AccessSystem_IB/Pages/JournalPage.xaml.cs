using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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
using AccessSystem_IB.util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DbContext = AccessSystem_IB.Data.database.DbContext;
using JournalEntry = AccessSystem_IB.Data.database.JournalEntry;

namespace AccessSystem_IB.Pages
{
    /// <summary>
    /// Логика взаимодействия для JournalPage.xaml
    /// </summary>
    public partial class JournalPage : Page
    {
        public ObservableCollection<JournalEntry> JournalList => JournalPageHelper.GetInstance().DataList;
        public ObservableCollection<JournalEntry> FilteredJournalList { get; } = new ObservableCollection<JournalEntry>();

        public JournalPage()
        {
            InitializeComponent();

            if (!JournalList.Any())
                DbContext.GetInstance().Send(async context =>
                {
                //FilteredJournalList.Clear();
                JournalList.Clear();

                    await foreach (var journalNote in context.Journal.Include(entry => entry.User).AsAsyncEnumerable())
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            JournalList.Add(journalNote);
                            FilteredJournalList.Add(journalNote);
                        });
                    }
                });
            else
            {
                foreach (var journalEntry in JournalList)
                    FilteredJournalList.Add(journalEntry);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dateConverted = DateTime.TryParse(SearchBox.Text, out var date);
            var idConverted = int.TryParse(SearchBox.Text, out var id);
            var jeConverted = Enum.TryParse(SearchBox.Text, out JournalEvent journalEvent);


            var journalEntries = JournalList.Where(entry =>
                (entry.Date == date && dateConverted) ||
                (entry.EventType == journalEvent && jeConverted) ||
                entry.User?.Login.Contains(SearchBox.Text) == true ||
                entry.EventDescription.Contains(SearchBox.Text) ||
                (entry.Id == id && idConverted) ||
                entry.WorkstationName.Contains(SearchBox.Text)
            ).ToList();

            FilteredJournalList.Clear();

            if (journalEntries.Any())
                foreach (var journalEntry in journalEntries)
                    FilteredJournalList.Add(journalEntry);
            else
                foreach (var journalEntry in JournalList)
                    FilteredJournalList.Add(journalEntry);
        }

        private void ArchiveButton_OnClick(object sender, RoutedEventArgs e)
        {
            using StreamWriter fs = new StreamWriter(new FileStream(Directory.GetCurrentDirectory() + "\\temp.txt", FileMode.OpenOrCreate));
            fs.Flush();
            fs.Write(JsonSerializer.Serialize(JournalList.Select(entry => new JournalEntry
            {
                User = new User
                {
                    Login = entry?.User?.Login,
                    Password = entry?.User?.Password, 
                    Role = entry?.User?.Role??Roles.User,
                    AuthStory = new List<UserAuthInfo>(), 
                    JournalEntries = new List<JournalEntry>()
                },
                Date = entry?.Date??DateTime.Now,
                EventDescription = entry?.EventDescription,
                EventType = entry?.EventType??JournalEvent.UserAnyAction,
                Id = entry?.Id??0,
                WorkstationName = entry?.WorkstationName
            })));
            using FileStream targetStream = File.Create($"{Directory.GetCurrentDirectory()}\\{DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace('.', '_').Replace(':', '_').Replace('/', '_')}.gz");
            using GZipStream gz = new GZipStream(targetStream, CompressionMode.Compress);
            fs.BaseStream.CopyTo(gz);

            MessageBox.Show("Сжатие завершено");
        }

        private void DeleteArchiveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.gz");
            foreach (var file in files)
                File.Delete(file);
            MessageBox.Show("Очистка архивов завершена");
        }
    }
}
