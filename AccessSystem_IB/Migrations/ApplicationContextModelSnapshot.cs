﻿// <auto-generated />
using System;
using AccessSystem_IB.Data.database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AccessSystem_IB.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.9")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AccessSystem_IB.Data.database.JournalEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("EventDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<string>("UserLogin")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("WorkstationName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("UserLogin");

                    b.ToTable("Journal");
                });

            modelBuilder.Entity("AccessSystem_IB.Data.database.User", b =>
                {
                    b.Property<string>("Login")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("Login");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AccessSystem_IB.Data.database.UserAuthInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("UserLogin")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserLogin");

                    b.ToTable("UserAuthInfos");
                });

            modelBuilder.Entity("AccessSystem_IB.Data.database.JournalEntry", b =>
                {
                    b.HasOne("AccessSystem_IB.Data.database.User", "User")
                        .WithMany("JournalEntries")
                        .HasForeignKey("UserLogin")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("User");
                });

            modelBuilder.Entity("AccessSystem_IB.Data.database.UserAuthInfo", b =>
                {
                    b.HasOne("AccessSystem_IB.Data.database.User", "User")
                        .WithMany("AuthStory")
                        .HasForeignKey("UserLogin")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("User");
                });

            modelBuilder.Entity("AccessSystem_IB.Data.database.User", b =>
                {
                    b.Navigation("AuthStory");

                    b.Navigation("JournalEntries");
                });
#pragma warning restore 612, 618
        }
    }
}