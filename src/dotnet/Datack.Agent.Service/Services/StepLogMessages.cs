using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services
{
    public class StepLogMessages
    {
        private readonly DataContextFactory _dataContextFactory;

        public StepLogMessages(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task Add(StepLogMessage message)
        {
            await using var context = _dataContextFactory.Create();

            await context.StepLogMessages.AddAsync(message);
            await context.SaveChangesAsync();
        }
    }
}
