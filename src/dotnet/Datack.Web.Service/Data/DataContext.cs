using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Datack.Web.Service.Data
{
    public class DataContext : IdentityDbContext
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            IgnoreNullValues = true
        };

        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobRun> JobRuns { get; set; }
        public DbSet<JobRunTask> JobRunTasks { get; set; }
        public DbSet<JobRunTaskLog> JobRunTaskLogs { get; set; }
        public DbSet<JobTask> JobTasks { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDbFunction(typeof(DataContext).GetMethod(nameof(JsonValue)))
                        .HasTranslation(e => new SqlFunctionExpression("JSON_VALUE",
                                                                       e,
                                                                       true,
                                                                       new[]
                                                                       {
                                                                           false, false
                                                                       },
                                                                       typeof(String),
                                                                       null))
                        .HasParameter("column")
                        .HasStoreType("nvarchar(max)");
            
            modelBuilder.Entity<JobRunTaskLog>()
                        .Property(e => e.JobRunTaskLogId)
                        .ValueGeneratedOnAdd();

            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new JobRunDbConfiguration());
            modelBuilder.ApplyConfiguration(new JobRunTaskDbConfiguration());
            modelBuilder.ApplyConfiguration(new JobTaskDbConfiguration());
            modelBuilder.ApplyConfiguration(new ServerConfiguration());
            modelBuilder.ApplyConfiguration(new ServerDbConfiguration());
        }

        public async Task Seed()
        {
            var seedSettings = new List<Setting>();

            var dbSettings = await Settings.ToListAsync();

            foreach (var seedSetting in seedSettings)
            {
                var dbSetting = dbSettings.FirstOrDefault(m => m.SettingId == seedSetting.SettingId);

                if (dbSetting == null)
                {
                    await Settings.AddAsync(seedSetting);
                    await SaveChangesAsync();
                }
            }

#if DEBUG
            if (!Servers.Any())
            {
                var server = new Server
                {
                    ServerId = Guid.Parse("FC1005A3-6941-4BC2-A4C1-4A3863F3F7BA"),
                    Key = "5026d123-0b7a-4ecc-9b97-4950324f161f",
                    Name = "Local SQL Server",
                    Description = "Test Server",
                    DbSettings = new ServerDbSettings
                    {
                        Server = ".",
                        UserName = "Test",
                        Password = "test"
                    },
                    Settings = new ServerSettings
                    {
                        TempPath = @"C:\Temp"
                    }
                };
                
                var job = new Job
                {
                    JobId = Guid.Parse("6b9e6002-13ef-454a-92a2-23818a5737ac"),
                    Description = "Create backup job",
                    Name = "Backup Job",
                    Settings = new JobSettings
                    {
                        CronFull = "0 4 * * *",
                        CronDiff = "0 * * * *",
                        CronLog = "*/15 * * * *"
                    }
                };

                var jobTask1 = new JobTask
                {
                    JobTaskId = Guid.Parse("FF1F7DFF-12A5-4CAB-8EFC-181805D1BC48"),
                    JobId = job.JobId,
                    ServerId = server.ServerId,
                    Description = "Creates a backup",
                    Name = "Create Database Backups",
                    Order = 0,
                    Parallel = 2,
                    Type = "create_backup",
                    Settings = new JobTaskSettings
                    {
                        CreateBackup = new JobTaskCreateDatabaseSettings
                        {
                            FileName = @"C:\Temp\datack\backups\{DatabaseName}-{0:yyyyMMddHHmm}.bak",
                            BackupDefaultExclude = false,
                            BackupExcludeManual = "",
                            BackupExcludeRegex = "",
                            BackupExcludeSystemDatabases = true,
                            BackupIncludeManual = "",
                            BackupIncludeRegex = ""
                        }
                    }
                };

                var jobTask2 = new JobTask
                {
                    JobTaskId = Guid.Parse("D39DF0FE-7E6D-4BE5-B224-69DBDE88BE8A"),
                    JobId = job.JobId,
                    ServerId = server.ServerId,
                    Description = "Compress the backups with 7z",
                    Name = "Compress backups",
                    Order = 1,
                    Parallel = 2,
                    Type = "compress",
                    UsePreviousTaskArtifactsFromJobTaskId = Guid.Parse("FF1F7DFF-12A5-4CAB-8EFC-181805D1BC48"),
                    Settings = new JobTaskSettings
                    {
                        Compress = new JobTaskCompressSettings
                        {
                            FileName = @"C:\Temp\datack\backups\{DatabaseName}-{0:yyyyMMddHHmm}.7z",
                            ArchiveType = "7z",
                            CompressionLevel = "5",
                            MultithreadMode = "on",
                            Password = "test"
                        }
                    }
                };
                
                var jobTask3 = new JobTask
                {
                    JobTaskId = Guid.Parse("010B8915-B1B3-4864-B7B2-34DE98F7E535"),
                    JobId = job.JobId,
                    ServerId = server.ServerId,
                    Description = "Upload compressed backup to S3",
                    Name = "Upload to S3",
                    Order = 2,
                    Parallel = 2,
                    Type = "upload_s3",
                    UsePreviousTaskArtifactsFromJobTaskId = Guid.Parse("D39DF0FE-7E6D-4BE5-B224-69DBDE88BE8A"),
                    Settings = new JobTaskSettings
                    {
                        UploadS3 = new JobTaskUploadS3Settings()
                        {
                            FileName = @"{DatabaseName}/{0:yyyyMMddHHmm}.7z",
                            AccessKey = "",
                            Region = "",
                            Bucket = "",
                            Secret = ""
                        }
                    }
                };

                await Servers.AddAsync(server);
                await Jobs.AddAsync(job);
                await JobTasks.AddAsync(jobTask1);
                await JobTasks.AddAsync(jobTask2);
                await JobTasks.AddAsync(jobTask3);

                await SaveChangesAsync();
            }
#endif
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static String JsonValue(Object column, String path)
        {
            throw new NotSupportedException();
        }
#pragma warning restore IDE0060 // Remove unused parameter

        public class JobConfiguration : IEntityTypeConfiguration<Job>
        {
            public void Configure(EntityTypeBuilder<Job> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<JobSettings>(v, SerializerOptions));
            }
        }

        public class JobRunDbConfiguration : IEntityTypeConfiguration<JobRun>
        {
            public void Configure(EntityTypeBuilder<JobRun> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<JobSettings>(v, SerializerOptions));
            }
        }

        public class JobRunTaskDbConfiguration : IEntityTypeConfiguration<JobRunTask>
        {
            public void Configure(EntityTypeBuilder<JobRunTask> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<JobTaskSettings>(v, SerializerOptions));
            }
        }

        public class JobTaskDbConfiguration : IEntityTypeConfiguration<JobTask>
        {
            public void Configure(EntityTypeBuilder<JobTask> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<JobTaskSettings>(v, SerializerOptions));
            }
        }

        public class ServerConfiguration : IEntityTypeConfiguration<Server>
        {
            public void Configure(EntityTypeBuilder<Server> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<ServerSettings>(v, SerializerOptions));
            }
        }

        public class ServerDbConfiguration : IEntityTypeConfiguration<Server>
        {
            public void Configure(EntityTypeBuilder<Server> builder)
            {
                builder.Property(e => e.DbSettings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<ServerDbSettings>(v, SerializerOptions));
            }
        }
    }

    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(String[] args)
        {
            var builder = new DbContextOptionsBuilder<DataContext>();
            var connectionString = $"Data Source=Datack.db";
            builder.UseSqlite(connectionString);
            return new DataContext(builder.Options);
        }
    }
}
