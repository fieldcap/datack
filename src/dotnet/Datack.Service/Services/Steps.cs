using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Data.Data;
using Datack.Data.Models.Data;

namespace Datack.Service.Services
{
    public class Steps
    {
        private readonly StepData _stepData;

        public Steps(StepData stepData)
        {
            _stepData = stepData;
        }

        public async Task<IList<Step>> GetForJob(Guid jobId, CancellationToken cancellationToken)
        {
            return await _stepData.GetForJob(jobId, cancellationToken);
        }

        public async Task<Step> GetById(Guid stepId, CancellationToken cancellationToken)
        {
            return await _stepData.GetById(stepId, cancellationToken);
        }

        public async Task Update(Step step)
        {
            if (String.IsNullOrWhiteSpace(step.Name))
            {
                throw new Exception($"Step name cannot be empty");
            }

            await _stepData.Update(step);
        }
    }
}
