using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Web.Service.Services;

namespace Datack.Web.Service.Tasks;

/// <summary>
///     This task downloads a file from Azure.
/// </summary>
public class DownloadAzureFileTask : IBaseTask
{
    private readonly RemoteService _remoteService;

    public DownloadAzureFileTask(RemoteService remoteService)
    {
        _remoteService = remoteService;
    }

    public async Task<List<JobRunTask>> Setup(Job job, JobTask jobTask, IList<JobRunTask> previousJobRunTasks, Guid jobRunId, CancellationToken cancellationToken)
    {
        if (jobTask.Settings?.DownloadAzure == null)
        {
            throw new($"Job task {jobTask.Name} does not have settings");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Settings.DownloadAzure.ConnectionString))
        {
            throw new($"Job task {jobTask.Name} does not have a valid connection string set");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Settings.DownloadAzure.ContainerName))
        {
            throw new($"Job task {jobTask.Name} does not have a valid container name set");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Settings.DownloadAzure.Blob))
        {
            throw new($"Job task {jobTask.Name} does not have a valid blob name set");
        }

        var allFiles = await _remoteService.GetFileList(jobTask.Agent,
                                                        "azure",
                                                        jobTask.Settings.DownloadAzure.ConnectionString,
                                                        jobTask.Settings.DownloadAzure.ContainerName,
                                                        jobTask.Settings.DownloadAzure.Blob,
                                                        null,
                                                        cancellationToken);

        var filteredFiles = FileHelper.FilterFiles(allFiles,
                                                   jobTask.Settings.DownloadAzure.RestoreDefaultExclude,
                                                   jobTask.Settings.DownloadAzure.RestoreIncludeRegex,
                                                   jobTask.Settings.DownloadAzure.RestoreExcludeRegex,
                                                   jobTask.Settings.DownloadAzure.RestoreIncludeManual,
                                                   jobTask.Settings.DownloadAzure.RestoreExcludeManual);

        var index = 0;

        var results = new List<JobRunTask>();

        foreach (var file in filteredFiles.Where(m => m.Include))
        {
            var files = await _remoteService.GetFileList(jobTask.Agent,
                                                         "azure",
                                                         jobTask.Settings.DownloadAzure.ConnectionString,
                                                         jobTask.Settings.DownloadAzure.ContainerName,
                                                         jobTask.Settings.DownloadAzure.Blob,
                                                         file.DatabaseName,
                                                         cancellationToken);

            if (files.Count == 0)
            {
                continue;
            }

            var fileName = files.OrderByDescending(m => m.DateTime).First();

            results.Add(new()
            {
                JobRunTaskId = Guid.NewGuid(),
                JobTaskId = jobTask.JobTaskId,
                JobRunId = jobRunId,
                Type = jobTask.Type,
                ItemName = fileName.FileName,
                ItemOrder = index++,
                IsError = false,
                Result = null,
                Settings = jobTask.Settings
            });
        }

        return results;
    }
}