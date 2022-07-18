using System.Reflection;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class JobTasks
{
    private readonly Agents _agents;
    private readonly JobRunTaskLogRepository _jobRunTaskLogRepository;
    private readonly JobRunTaskRepository _jobRunTaskRepository;
    private readonly JobTaskRepository _jobTaskRepository;
    private readonly RemoteService _remoteService;

    public JobTasks(JobTaskRepository jobTaskRepository,
                    JobRunTaskRepository jobRunTaskRepository,
                    JobRunTaskLogRepository jobRunTaskLogRepository,
                    Agents agents,
                    RemoteService remoteService)
    {
        _jobTaskRepository = jobTaskRepository;
        _jobRunTaskRepository = jobRunTaskRepository;
        _jobRunTaskLogRepository = jobRunTaskLogRepository;
        _agents = agents;
        _remoteService = remoteService;
    }

    public async Task<IList<JobTask>> GetForJob(Guid jobId, CancellationToken cancellationToken)
    {
        return await _jobTaskRepository.GetForJob(jobId, cancellationToken);
    }

    public async Task<JobTask> GetById(Guid jobTaskId, CancellationToken cancellationToken)
    {
        return await _jobTaskRepository.GetById(jobTaskId, cancellationToken);
    }

    public async Task<JobTask> Add(JobTask jobTask, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(jobTask.Name))
        {
            throw new Exception("Name cannot be empty");
        }

        var jobTasks = await _jobTaskRepository.GetForJob(jobTask.JobId, cancellationToken);
        var sameNameTasks = jobTasks.Any(m => String.Equals(m.Name, jobTask.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameTasks)
        {
            throw new Exception($"A task with this name for this job already exists");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Type))
        {
            throw new Exception("Task type cannot be empty");
        }

        if (jobTask.AgentId == Guid.Empty)
        {
            throw new Exception("Agent cannot be empty");
        }

        if (jobTask.Parallel < 0)
        {
            throw new Exception($"Parallel cannot be smaller than 0");
        }

        return await _jobTaskRepository.Add(jobTask, cancellationToken);
    }

    public async Task Update(JobTask jobTask, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(jobTask.Name))
        {
            throw new Exception("Name cannot be empty");
        }

        var jobTasks = await _jobTaskRepository.GetForJob(jobTask.JobId, cancellationToken);
        var sameNameTasks = jobTasks.Any(m => m.JobTaskId != jobTask.JobTaskId && String.Equals(m.Name, jobTask.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameTasks)
        {
            throw new Exception($"A task with this name for this job already exists");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Type))
        {
            throw new Exception("Task type cannot be empty");
        }

        if (jobTask.AgentId == Guid.Empty)
        {
            throw new Exception("Agent cannot be empty");
        }

        if (jobTask.Parallel < 0)
        {
            throw new Exception($"Parallel cannot be smaller than 0");
        }

        var agent = await _agents.GetById(jobTask.AgentId, cancellationToken);
        var dbJobTask = await GetById(jobTask.JobTaskId, cancellationToken);

        await EncryptSettings(agent, jobTask.Settings, dbJobTask.Settings, cancellationToken);

        await _jobTaskRepository.Update(jobTask, cancellationToken);
    }

    public async Task ReOrder(Guid jobId, IList<Guid> jobTaskIds, CancellationToken cancellationToken)
    {
        await _jobTaskRepository.ReOrder(jobId, jobTaskIds, cancellationToken);
    }

    private async Task EncryptSettings(Agent agent, JobTaskSettings newJobTaskSettings, JobTaskSettings currentJobTaskSettings, CancellationToken cancellationToken)
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

                var encryptedSettingValue = await _remoteService.Encrypt(agent, newSettingValueString, cancellationToken);

                settingKey.SetValue(newSetting, encryptedSettingValue);
            }
        }
    }

    public async Task DeleteForJob(Guid jobId, CancellationToken cancellationToken)
    {
        await _jobTaskRepository.DeleteForJob(jobId, cancellationToken);
    }

    public async Task Delete(Guid jobTaskId, CancellationToken cancellationToken)
    {
        await _jobRunTaskLogRepository.DeleteForTask(jobTaskId, cancellationToken);
        await _jobRunTaskRepository.DeleteForTask(jobTaskId, cancellationToken);
        await _jobTaskRepository.Delete(jobTaskId, cancellationToken);
    }
}