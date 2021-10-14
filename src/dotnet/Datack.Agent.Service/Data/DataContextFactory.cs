using System;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Data
{
    public class DataContextFactory
    {
        private readonly DbContextOptions<DataContext> _options;

        public DataContextFactory(String connectionString)
        {
            _options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connectionString).Options;
        }

        public DataContext Create()
        {
            return new DataContext(_options);
        }
    }
}
