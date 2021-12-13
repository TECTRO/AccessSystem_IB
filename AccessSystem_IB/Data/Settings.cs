using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using AccessSystem_IB.util;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AccessSystem_IB.Data
{
    public class Settings
    {
        private int _failedLoginAttempt = 3;

        #region save\load

        static Settings()
        {
            Serializer = new Newtonsoft.Json.JsonSerializer();
            Serializer.Converters.Add(new TimespanConverter());
        }
        private static string SettingsPath { get; } = "Globals.config";

        private static Newtonsoft.Json.JsonSerializer Serializer { get; }     //  ..privateserializer.Converters.Add(new JavaScriptDateTimeConverter());

        public static Settings GetSettings()
        {
            try
            {
                using var reader = new JsonTextReader(new StreamReader(SettingsPath));
                return Serializer.Deserialize<Settings>(reader);
                
                //using var loader = new StreamReader(new FileStream(SettingsPath, FileMode.OpenOrCreate), Encoding.Unicode);
                //var json = loader.ReadToEnd();
                //JsonSerializer.Deserialize<Settings>(json);
            }
            catch
            {
                return new Settings();
            }
        }

        public void Save()
        {
            using var writer = new JsonTextWriter(new StreamWriter(SettingsPath, false));
            Serializer.Serialize(writer,this);
            //using var saver = new StreamWriter(SettingsPath, false, Encoding.Unicode);
            // saver.WriteLineAsync(JsonSerializer.Serialize(this));
        }
        #endregion

        public int FailedLoginAttempt
        {
            get => _failedLoginAttempt;
            set
            {
                if (_failedLoginAttempt > MaxFailedLoginAttempt)
                    _failedLoginAttempt = MaxFailedLoginAttempt;
                if (_failedLoginAttempt < 0)
                    _failedLoginAttempt = 0;
                _failedLoginAttempt = value;
            }
        }
        private int MaxFailedLoginAttempt { get; } = 10;

        public TimeSpan BanTime { get; set; } = new TimeSpan(0, 0, 0, 30);
        public TimeSpan AwaitKickTime { get; set; } = new TimeSpan(0, 0, 0, 10);
    }
}