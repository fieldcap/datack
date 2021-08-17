using System.Collections.Generic;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services
{
    public class StepLogs
    {
        private readonly DataContext _dataContext;

        public StepLogs(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Create(List<StepLog> stepLogs)
        {
            await _dataContext.AddRangeAsync(stepLogs);
            await _dataContext.SaveChangesAsync();
        }
    }
}
