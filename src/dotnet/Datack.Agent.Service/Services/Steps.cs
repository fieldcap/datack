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
        private readonly DataContext _dataContext;

        public Steps(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task UpdateSteps(IList<Step> steps, Guid serverId)
        {
            var dbSteps = await _dataContext.Steps.ToListAsync();

            steps = steps.Where(m => m.ServerId == serverId).ToList();

            foreach (var dbStep in dbSteps)
            {
                var newStep = dbSteps.FirstOrDefault(m => m.StepId == dbStep.JobId);

                if (newStep == null)
                {
                    _dataContext.Remove(dbStep);
                    await _dataContext.SaveChangesAsync();
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
                await _dataContext.Steps.AddAsync(step);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<IList<Step>> GetForJob(Guid jobId)
        {
            return await _dataContext.Steps.AsNoTracking().Where(m => m.JobId == jobId).ToListAsync();
        }
    }
}
