using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class JobTasks
    {
        private readonly JobTaskRepository _jobTaskRepository;
        private readonly Servers _servers;
        private readonly RemoteService _remoteService;

        public JobTasks(JobTaskRepository jobTaskRepository, Servers servers, RemoteService remoteService)
        {
            _jobTaskRepository = jobTaskRepository;
            _servers = servers;
            _remoteService = remoteService;
        }

        public async Task<IList<JobTask>> GetForJob(Guid jobId, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.GetForJob(jobId, cancellationToken);
        }

        public async Task<IList<JobTask>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.GetForServer(serverId, cancellationToken);
        }

        public async Task<JobTask> GetById(Guid jobTaskId, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.GetById(jobTaskId, cancellationToken);
        }

        public async Task<JobTask> Add(JobTask jobTask, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.Add(jobTask, cancellationToken);
        }

        public async Task Update(JobTask jobTask, CancellationToken cancellationToken)
        {
            var server = await _servers.GetById(jobTask.ServerId, cancellationToken);
            var dbJobTask = await GetById(jobTask.JobTaskId, cancellationToken);

            await EncryptSettings(server, jobTask.Settings, dbJobTask.Settings, cancellationToken);

            await _jobTaskRepository.Update(jobTask, cancellationToken);
        }

        public async Task ReOrder(Guid jobId, IList<Guid> jobTaskIds, CancellationToken cancellationToken)
        {
            await _jobTaskRepository.ReOrder(jobId, jobTaskIds, cancellationToken);
        }

        private async Task EncryptSettings(Server server, JobTaskSettings newJobTaskSettings, JobTaskSettings currentJobTaskSettings, CancellationToken cancellationToken)
        {
            var properties = typeof(JobTaskSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var newSetting = property.GetValue(newJobTaskSettings);
                var currentSettings = property.GetValue(currentJobTaskSettings);

                if (newSetting == null)
                {
                    continue;
                }

                currentSettings ??= Activator.CreateInstance(newSetting.GetType());

                var newSettingKeys = newSetting.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var settingKey in newSettingKeys)
                {
                    if (!Attribute.IsDefined(settingKey, typeof(ProtectedAttribute)))
                    {
                        continue;
                    }

                    var newSettingValue = settingKey.GetValue(newSetting);
                    var currentSettingValue = settingKey.GetValue(currentSettings);

                    if (newSettingValue == null && currentSettingValue == null)
                    {
                        continue;
                    }

                    if (newSettingValue == null)
                    {
                        settingKey.SetValue(newSetting, currentSettingValue);
                        continue;
                    }

                    var newSettingValueString = newSettingValue.ToString();

                    if (newSettingValueString == "******")
                    {
                        settingKey.SetValue(newSetting, currentSettingValue);
                        continue;
                    }

                    var encryptedSettingValue = await _remoteService.Encrypt(server, newSettingValueString, cancellationToken);

                    settingKey.SetValue(newSetting, encryptedSettingValue);
                }
            }
        }
    }
}
