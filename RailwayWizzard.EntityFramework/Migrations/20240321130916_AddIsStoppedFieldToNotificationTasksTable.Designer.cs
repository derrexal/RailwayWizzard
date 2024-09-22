﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RailwayWizzard.EntityFrameworkCore;

#nullable disable

namespace RailwayWizzard.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(RailwayWizzardAppContext))]
    [Migration("20240321130916_AddIsStoppedFieldToNotificationTasksTable")]
    partial class AddIsStoppedFieldToNotificationTasksTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RailwayWizzard.Core.NotificationTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ArrivalStation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("DateFrom")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DepartureStation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsActual")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsStopped")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsWorked")
                        .HasColumnType("boolean");

                    b.Property<string>("TimeFrom")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("AppNotificationTasks");
                });

            modelBuilder.Entity("RailwayWizzard.Core.StationInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("ExpressCode")
                        .HasColumnType("bigint");

                    b.Property<string>("StationName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("AppStationInfo");
                });

            modelBuilder.Entity("RailwayWizzard.Core.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("IdTg")
                        .HasColumnType("bigint");

                    b.Property<string>("Username")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("Id");

                    b.ToTable("AppUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
