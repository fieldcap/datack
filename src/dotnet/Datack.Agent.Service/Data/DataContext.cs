using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Datack.Agent.Data
{
    public class DataContext : DbContext
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

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(String[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlite("Data Source=Datack.db");

            return new DataContext(optionsBuilder.Options);
        }
    }
}
