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
        public DbSet<JobLog> JobLogs { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<StepLog> StepLogs { get; set; }
        public DbSet<StepLogMessage> StepLogMessages { get; set; }

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

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                                         .SelectMany(t => t.GetForeignKeys())
                                         .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<StepLogMessage>()
                        .Property(e => e.StepLogMessageId)
                        .ValueGeneratedOnAdd();

            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new ServerConfiguration());
            modelBuilder.ApplyConfiguration(new ServerDbConfiguration());
            modelBuilder.ApplyConfiguration(new StepDbConfiguration());
            modelBuilder.ApplyConfiguration(new JobLogDbConfiguration());
            modelBuilder.ApplyConfiguration(new StepLogDbConfiguration());
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
                    ServerId = Guid.NewGuid(),
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
                    JobId = Guid.NewGuid(),
                    Description = "Create backup job",
                    Name = "Create backup",
                    Settings = new JobSettings
                    {
                        CronFull = "5 4 * * *",
                        CronDiff = "5 * * * *",
                        CronLog = "*/5 * * * *"
                    }
                };

                var step = new Step
                {
                    StepId= Guid.NewGuid(),
                    JobId = job.JobId,
                    ServerId = server.ServerId,
                    Description = "Creates a backup",
                    Name = "Create backup",
                    Order = 0,
                    Type = "create_backup",
                    Settings = new StepSettings
                    {
                        CreateBackup = new StepCreateDatabaseSettings
                        {
                            BackupDefaultExclude = false,
                            BackupExcludeManual = "",
                            BackupExcludeRegex = "",
                            BackupExcludeSystemDatabases = true,
                            BackupIncludeManual = "",
                            BackupIncludeRegex = ""
                        }
                    }
                };

                await Servers.AddAsync(server);
                await Jobs.AddAsync(job);
                await Steps.AddAsync(step);

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
        
        public class StepDbConfiguration : IEntityTypeConfiguration<Step>
        {
            public void Configure(EntityTypeBuilder<Step> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<StepSettings>(v, SerializerOptions));
            }
        }

        public class JobLogDbConfiguration : IEntityTypeConfiguration<JobLog>
        {
            public void Configure(EntityTypeBuilder<JobLog> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<JobSettings>(v, SerializerOptions));
            }
        }
        
        public class StepLogDbConfiguration : IEntityTypeConfiguration<StepLog>
        {
            public void Configure(EntityTypeBuilder<StepLog> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<StepSettings>(v, SerializerOptions));
            }
        }
    }

    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(String[] args)
        {
            var builder = new DbContextOptionsBuilder<DataContext>();
            var connectionString = $"Data Source=test.db";
            builder.UseSqlite(connectionString);
            return new DataContext(builder.Options);
        }
    }
}
