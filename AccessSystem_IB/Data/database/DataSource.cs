using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AccessSystem_IB.Data.database
{
    public class User
    {
        [Key]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public Roles Role { get; set; }

        public List<UserAuthInfo> AuthStory { get; set; } = new List<UserAuthInfo>();
        public List<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

        public override string ToString() => Login;
    }
    public enum Roles
    {
        User,
        Admin
    }

    public class UserAuthInfo
    {
        [Key]
        public int Id { get; set; }
        public User User { get; set; }
        public LoginStatus Status { get; set; }
        public DateTime Date { get; set; }
    }

    public enum LoginStatus
    {
        Succeed,
        Erred,
        Banned
    }

    public class JournalEntry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public JournalEvent EventType { get; set; }
        public string EventDescription { get; set; }
        public User User { get; set; }
        [Required]
        public string WorkstationName { get; set; }
    }

    public enum JournalEvent
    {
        SystemStart,
        SystemStop,
        UserIsLoggedIn,
        UserIsLoggedOut,
        FailedLoginAttempt,
        //AttemptToChangeJournalSettings,
        AttemptToArchiveJournal,
        AttemptToClearJournal,
        UserAnyAction,
        CreateUserAccount,
        UpdateUserAccount,
        DeleteUserAccount,
    }

    public class GlobalSettings
    {
        public int MaxFailedLoginAttempt { get; set; }
        public TimeSpan BlockedTime { get; set; }
        public TimeSpan BeforeKickDelay { get; set; }
    }

    public sealed class ApplicationContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private string ConnectionString { get; } =
            @"Server=(localdb)\mssqllocaldb;Database=AccessSystemDB_IB;Trusted_Connection=True;";
        public DbSet<User> Users { get; set; }
        public DbSet<UserAuthInfo> UserAuthInfos { get; set; }
        public DbSet<JournalEntry> Journal { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlServer(ConnectionString).EnableSensitiveDataLogging();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasMany(p => p.JournalEntries)
                .WithOne(t => t.User)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(p => p.AuthStory)
                .WithOne(t => t.User)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
