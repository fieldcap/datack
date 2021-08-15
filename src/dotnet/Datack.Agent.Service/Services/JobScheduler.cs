using System.Collections.Generic;
using System.Linq;
using Datack.Agent.Services.Jobs;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services
{
    public class JobScheduler
    {
        private Server _server;
        
        private readonly List<JobRunner> _jobRunners = new List<JobRunner>();

        public void Update(Server server, IList<Job> jobs)
        {
            _server = server;

            foreach (var job in jobs)
            {
                var jobRunner = _jobRunners.FirstOrDefault(m => m.JobId == job.JobId);

                if (jobRunner == null)
                {
                    jobRunner = new JobRunner();
                    jobRunner.Start(_server, job);
                    _jobRunners.Add(jobRunner);
                }
                else
                {
                    jobRunner.Update(job);
                }
            }
        }
    }
}
