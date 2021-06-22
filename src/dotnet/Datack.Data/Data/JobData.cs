﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Data.Data
{
    public class JobData
    {
        private readonly DataContext _dataContext;

        public JobData(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Job>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs.Where(m => m.ServerId == serverId).OrderBy(m => m.Name).ToListAsync(cancellationToken);
        }

        public async Task<Job> GetById(Guid jobId, CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs.Include(m => m.Server).FirstOrDefaultAsync(m => m.JobId == jobId, cancellationToken);
        }
    }
}
