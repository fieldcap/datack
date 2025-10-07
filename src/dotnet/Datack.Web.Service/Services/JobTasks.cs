using System.Reflection;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class JobTasks(
    JobTaskRepository jobTaskRepository,
    JobRunTaskRepository jobRunTaskRepository,
    JobRunTaskLogRepository jobRunTaskLogRepository,
    Agents agents,
    RemoteService remoteService)
{
    public async Task<IList<JobTask>> GetForJob(Guid jobId, CancellationToken cancellationToken)
    {
        return await jobTaskRepository.GetForJob(jobId, cancellationToken);
    }

    public async Task<JobTask?> GetById(Guid jobTaskId, CancellationToken cancellationToken)
    {
        return await jobTaskRepository.GetById(jobTaskId, cancellationToken);
    }

    public async Task<JobTask> Add(JobTask jobTask, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(jobTask.Name))
        {
            throw new("Name cannot be empty");
        }

        var jobTasks = await jobTaskRepository.GetForJob(jobTask.JobId, cancellationToken);
        var sameNameTasks = jobTasks.Any(m => String.Equals(m.Name, jobTask.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameTasks)
        {
            throw new($"A task with this name for this job already exists");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Type))
        {
            throw new("Task type cannot be empty");
        }

        if (jobTask.AgentId == Guid.Empty)
        {
            throw new("Agent cannot be empty");
        }

        if (jobTask.Parallel < 0)
        {
            throw new($"Parallel cannot be smaller than 0");
        }

        return await jobTaskRepository.Add(jobTask, cancellationToken);
    }

    public async Task Update(JobTask jobTask, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(jobTask.Name))
        {
            throw new("Name cannot be empty");
        }

        var dbJobTask = await GetById(jobTask.JobTaskId, cancellationToken);
        var agent = await agents.GetById(jobTask.AgentId, cancellationToken);

        if (dbJobTask == null)
        {
            throw new($"Cannot find job task with ID {jobTask.JobTaskId}");
        }

        if (agent == null)
        {
            throw new($"Cannot find agent with ID {jobTask.JobTaskId}");
        }

        var jobTasks = await jobTaskRepository.GetForJob(jobTask.JobId, cancellationToken);
        var sameNameTasks = jobTasks.Any(m => m.JobTaskId != jobTask.JobTaskId && String.Equals(m.Name, jobTask.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameTasks)
        {
            throw new($"A task with this name for this job already exists");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Type))
        {
            throw new("Task type cannot be empty");
        }

        if (jobTask.AgentId == Guid.Empty)
        {
            throw new("Agent cannot be empty");
        }

        if (jobTask.Parallel < 0)
        {
            throw new($"Parallel cannot be smaller than 0");
        }
        
        await EncryptSettings(agent, jobTask.Settings, dbJobTask.Settings, cancellationToken);
        

        await jobTaskRepository.Update(jobTask, cancellationToken);
    }

    public async Task ReOrder(Guid jobId, IList<Guid> jobTaskIds, CancellationToken cancellationToken)
    {
        await jobTaskRepository.ReOrder(jobId, jobTaskIds, cancellationToken);
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

                var newSettingValueString = newSettingValue.ToString()!;

                if (newSettingValueString == "******")
                {
                    settingKey.SetValue(newSetting, currentSettingValue);

                    continue;
                }

                var encryptedSettingValue = await remoteService.Encrypt(agent, newSettingValueString, cancellationToken);

                settingKey.SetValue(newSetting, encryptedSettingValue);
            }
        }
    }

    public async Task DeleteForJob(Guid jobId, CancellationToken cancellationToken)
    {
        await jobTaskRepository.DeleteForJob(jobId, cancellationToken);
    }

    public async Task Delete(Guid jobTaskId, CancellationToken cancellationToken)
    {
        await jobRunTaskLogRepository.DeleteForTask(jobTaskId, cancellationToken);
        await jobRunTaskRepository.DeleteForTask(jobTaskId, cancellationToken);
        await jobTaskRepository.Delete(jobTaskId, cancellationToken);
    }
}