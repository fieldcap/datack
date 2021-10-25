using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Extensions.Configuration;

namespace Datack.Web.Data
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

        public DbSet<Agent> Agents { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobRun> JobRuns { get; set; }
        public DbSet<JobRunTask> JobRunTasks { get; set; }
        public DbSet<JobRunTaskLog> JobRunTaskLogs { get; set; }
        public DbSet<JobTask> JobTasks { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                                         .SelectMany(t => t.GetForeignKeys())
                                         .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

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
            modelBuilder.ApplyConfiguration(new AgentConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public async Task Seed()
        {
            var seedSettings = new List<Setting>
            {
                new Setting
                {
                    SettingId = "LogLevel",
                    Value = "Information"
                },
                new Setting
                {
                    SettingId = "Email:Smtp:Host",
                    Value = ""
                },
                new Setting
                {
                    SettingId = "Email:Smtp:Port",
                    Value = ""
                },
                new Setting
                {
                    SettingId = "Email:Smtp:UserName",
                    Value = ""
                },
                new Setting
                {
                    SettingId = "Email:Smtp:Password",
                    Value = "",
                    Secure = true
                },
                new Setting
                {
                    SettingId = "Email:Smtp:UseSsl",
                    Value = ""
                },
                new Setting
                {
                    SettingId = "Email:Smtp:From",
                    Value = ""
                }
            };

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
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static String JsonValue(Object column, String path)
        {
            throw new NotSupportedException();
        }
#pragma warning restore IDE0060 // Remove unused parameter

        public class AgentConfiguration : IEntityTypeConfiguration<Agent>
        {
            public void Configure(EntityTypeBuilder<Agent> builder)
            {
                builder.Property(e => e.Settings)
                       .HasConversion(v => JsonSerializer.Serialize(v, SerializerOptions),
                                      v => JsonSerializer.Deserialize<AgentSettings>(v, SerializerOptions));
            }
        }

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
    }

    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(String[] args)
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory() + @"\..\Datack.Web.Web")
                                .AddJsonFile("appsettings.json")
                                .AddJsonFile("appsettings.Dev.json", true)
                                .Build();

            var builder = new DbContextOptionsBuilder<DataContext>();
            var connectionString = configuration.GetConnectionString("Datack");
            builder.UseSqlServer(connectionString);
            return new DataContext(builder.Options);
        }
    }
}
