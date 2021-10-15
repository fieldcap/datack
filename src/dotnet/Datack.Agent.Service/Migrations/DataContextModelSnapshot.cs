﻿// <auto-generated />
using System;
using Datack.Agent.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Datack.Agent.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("Datack.Common.Models.Data.Job", b =>
                {
                    b.Property<Guid>("JobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Settings")
                        .HasColumnType("TEXT");

                    b.HasKey("JobId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobRun", b =>
                {
                    b.Property<Guid>("JobRunId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("BackupType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("Completed")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsError")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("JobId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Result")
                        .HasColumnType("TEXT");

                    b.Property<string>("Settings")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Started")
                        .HasColumnType("TEXT");

                    b.HasKey("JobRunId");

                    b.HasIndex("JobId");

                    b.ToTable("JobRuns");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobRunTask", b =>
                {
                    b.Property<Guid>("JobRunTaskId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Completed")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsError")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ItemName")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemOrder")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("JobRunId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("JobTaskId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Result")
                        .HasColumnType("TEXT");

                    b.Property<string>("ResultArtifact")
                        .HasColumnType("TEXT");

                    b.Property<string>("Settings")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Started")
                        .HasColumnType("TEXT");

                    b.Property<int>("TaskOrder")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("JobRunTaskId");

                    b.HasIndex("JobRunId");

                    b.HasIndex("JobTaskId");

                    b.ToTable("JobRunTasks");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobRunTaskLog", b =>
                {
                    b.Property<long>("JobRunTaskLogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsError")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("JobRunTaskId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Message")
                        .HasColumnType("TEXT");

                    b.HasKey("JobRunTaskLogId");

                    b.HasIndex("JobRunTaskId");

                    b.ToTable("JobRunTaskLogs");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobTask", b =>
                {
                    b.Property<Guid>("JobTaskId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("JobId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Parallel")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ServerId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Settings")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("UsePreviousTaskArtifactsFromJobTaskId")
                        .HasColumnType("TEXT");

                    b.HasKey("JobTaskId");

                    b.HasIndex("JobId");

                    b.HasIndex("ServerId");

                    b.HasIndex("UsePreviousTaskArtifactsFromJobTaskId");

                    b.ToTable("JobTasks");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.Server", b =>
                {
                    b.Property<Guid>("ServerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("DbSettings")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Settings")
                        .HasColumnType("TEXT");

                    b.HasKey("ServerId");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.Setting", b =>
                {
                    b.Property<string>("SettingId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("SettingId");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobRun", b =>
                {
                    b.HasOne("Datack.Common.Models.Data.Job", "Job")
                        .WithMany()
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobRunTask", b =>
                {
                    b.HasOne("Datack.Common.Models.Data.JobRun", "JobRun")
                        .WithMany()
                        .HasForeignKey("JobRunId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Datack.Common.Models.Data.JobTask", "JobTask")
                        .WithMany()
                        .HasForeignKey("JobTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("JobRun");

                    b.Navigation("JobTask");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobRunTaskLog", b =>
                {
                    b.HasOne("Datack.Common.Models.Data.JobRunTask", "JobRunTask")
                        .WithMany()
                        .HasForeignKey("JobRunTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("JobRunTask");
                });

            modelBuilder.Entity("Datack.Common.Models.Data.JobTask", b =>
                {
                    b.HasOne("Datack.Common.Models.Data.Job", "Job")
                        .WithMany()
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Datack.Common.Models.Data.Server", "Server")
                        .WithMany()
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Datack.Common.Models.Data.JobTask", "UsePreviousTaskArtifactsFromJobTask")
                        .WithMany()
                        .HasForeignKey("UsePreviousTaskArtifactsFromJobTaskId");

                    b.Navigation("Job");

                    b.Navigation("Server");

                    b.Navigation("UsePreviousTaskArtifactsFromJobTask");
                });
#pragma warning restore 612, 618
        }
    }
}
