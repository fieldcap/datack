using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class Steps
    {
        private readonly DataContextFactory _dataContextFactory;

        public Steps(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task UpdateSteps(IList<Step> steps, Guid serverId)
        {
            await using var context = _dataContextFactory.Create();

            var dbSteps = await context.Steps.ToListAsync();

            steps = steps.Where(m => m.ServerId == serverId).ToList();

            foreach (var dbStep in dbSteps)
            {
                var newStep = dbSteps.FirstOrDefault(m => m.StepId == dbStep.JobId);

                if (newStep == null)
                {
                    context.Remove(dbStep);
                    await context.SaveChangesAsync();
                }
                else
                {
                    dbStep.Name = newStep.Name;
                    dbStep.Description = newStep.Description;
                    dbStep.Settings = newStep.Settings;
                    
                    steps = steps.Where(m => m.JobId != dbStep.JobId).ToList();
                }
            }

            foreach (var step in steps)
            {
                await context.Steps.AddAsync(step);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IList<Step>> GetForJob(Guid jobId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.Steps.AsNoTracking().Where(m => m.JobId == jobId).ToListAsync();
        }
    }
}
