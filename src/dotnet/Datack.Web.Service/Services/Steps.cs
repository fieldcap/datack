using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Service.Services
{
    public class Steps
    {
        private readonly DataContext _dataContext;

        public Steps(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Step>> GetForJob(Guid jobId, CancellationToken cancellationToken)
        {
            return await _dataContext.Steps.AsNoTracking().Where(m => m.JobId == jobId).OrderBy(m => m.Name).ToListAsync(cancellationToken);
        }

        public async Task<IList<Step>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _dataContext.Steps.AsNoTracking().Where(m => m.ServerId == serverId).ToListAsync(cancellationToken);
        }

        public async Task<Step> GetById(Guid stepId, CancellationToken cancellationToken)
        {
            return await _dataContext.Steps.AsNoTracking().FirstOrDefaultAsync(m => m.StepId == stepId, cancellationToken);
        }

        public async Task<Guid> Add(Step step)
        {
            step.StepId = Guid.NewGuid();

            var jobStepCount = await _dataContext.Steps.AsNoTracking().CountAsync(m => m.JobId == step.JobId);
            step.Order = jobStepCount;

            await _dataContext.Steps.AddAsync(step);
            await _dataContext.SaveChangesAsync();

            return step.StepId;
        }

        public async Task Update(Step step)
        {
            var dbStep = await _dataContext.Steps.FirstOrDefaultAsync(m => m.StepId == step.StepId);

            if (dbStep == null)
            {
                throw new Exception($"Step with ID {step.StepId} not found");
            }

            dbStep.Name = step.Name;
            dbStep.Description = step.Description;
            dbStep.Order = step.Order;
            dbStep.Type = step.Type;
            dbStep.Settings = step.Settings;
            dbStep.ServerId = step.ServerId;

            await _dataContext.SaveChangesAsync();
        }
    }
}
