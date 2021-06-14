using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Datack.Data.Models.Data;
using Datack.Data.Models.Internal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Datack.Data.Data
{
    public class DataContext : IdentityDbContext
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
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

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                                         .SelectMany(t => t.GetForeignKeys())
                                         .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.ApplyConfiguration(new JobConfiguration());
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

            /*var server = new Server
            {
                DbSettings = new ServerDbSettings
                {
                    Server = "127.0.0.1",
                    UserName = "Backup",
                    Password = "backup"
                },
                Settings = new ServerSettings
                {
                    TempPath = @"C:\Temp"
                },
                Name = "Local SQL Server",
                ServerId = Guid.NewGuid()
            };

            await Servers.AddAsync(server);
            await SaveChangesAsync();*/

        }

        public static String JsonValue(Object column, String path)
        {
            throw new NotSupportedException();
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
}
