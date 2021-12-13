using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using AccessSystem_IB.Data.database;

namespace AccessSystem_IB.util
{
    class JournalPageHelper
    {
        private JournalPageHelper() { }
        private static JournalPageHelper _instance;
        private bool _canClose;
        public static JournalPageHelper GetInstance() => _instance ??= new JournalPageHelper();

        public ObservableCollection<JournalEntry> DataList { get; } = new ObservableCollection<JournalEntry>();

        public delegate void CloseAppContext(bool newState);

        public event CloseAppContext CloseAppContextEvent;

        public bool CanClose
        {
            get => _canClose;
            set
            {
                _canClose = value;
                CloseAppContextEvent?.Invoke(value); 
            }
        }
    }
}
