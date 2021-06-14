using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Data.Data;
using Datack.Data.Models.Data;

namespace Datack.Service.Services
{
    public class Jobs
    {
        private readonly JobData _jobData;

        public Jobs(JobData jobData)
        {
            _jobData = jobData;
        }

        public async Task<IList<Job>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _jobData.GetForServer(serverId, cancellationToken);
        }
    }
}
