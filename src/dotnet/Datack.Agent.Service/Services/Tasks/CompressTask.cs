using System.Diagnostics;
using System.Text;
using ByteSizeLib;
using CliWrap;
using CliWrap.EventStream;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task compresses files with 7z.
/// </summary>
public class CompressTask : BaseTask
{
    private readonly DataProtector _dataProtector;

    public CompressTask(DataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public override async Task Run(JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();

            if (previousTask == null)
            {
                throw new("No previous task found");
            }

            if (jobRunTask.Settings.Compress == null)
            {
                throw new("No settings set");
            }

            var sourceFileName = previousTask.ResultArtifact;

            OnProgress(jobRunTask.JobRunTaskId, $"Starting compression task for file {sourceFileName}");

            if (String.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new($"No source file found");
            }

            if (!File.Exists(sourceFileName))
            {
                throw new($"Source file '{sourceFileName}' not found");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started,
                FileName = Path.GetFileName(jobRunTask.ItemName),
                FileNameWithoutExtension = Path.GetFileNameWithoutExtension(jobRunTask.ItemName)
            };

            var rawFileName = Path.GetFileName(jobRunTask.Settings.Compress.FileName);

            if (String.IsNullOrWhiteSpace(rawFileName))
            {
                throw new($"Invalid filename '{jobRunTask.Settings.Compress.FileName}'");
            }

            var fileName = rawFileName.FormatFromObject(tokenValues);

            var rawFilePath = Path.GetDirectoryName(jobRunTask.Settings.Compress.FileName);

            if (String.IsNullOrWhiteSpace(rawFilePath))
            {
                throw new($"Invalid file path '{jobRunTask.Settings.Compress.FileName}'");
            }

            var filePath = rawFilePath.FormatFromObject(tokenValues);

            var storePath = Path.Combine(filePath, fileName);

            OnProgress(jobRunTask.JobRunTaskId, $"Testing path {storePath}");

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            try
            {
                await File.WriteAllTextAsync(storePath, "Test write file", cancellationToken);
            }
            finally
            {
                File.Delete(storePath);
            }

            var archiveType = jobRunTask.Settings.Compress.ArchiveType;
            var compressionLevel = jobRunTask.Settings.Compress.CompressionLevel;
            var multithreadMode = jobRunTask.Settings.Compress.MultithreadMode;
            var password = jobRunTask.Settings.Compress.Password;

            var arguments = new List<String>
            {
                // Add files to archive
                "a",
                // Set output streams
                "-bso1 -bse2 -bsp1",
                // Compression method
                $"-mx={compressionLevel}",
                // Multithread mode
                $"-mmt={multithreadMode}",
                // Compression type
                $"-t{archiveType}",
            };

            if (!String.IsNullOrWhiteSpace(password))
            {
                var decryptedPassword = _dataProtector.Decrypt(password);

                // Encrypt header
                arguments.Add("-mhe");

                // Password
                arguments.Add(@$"-p""{decryptedPassword}""");
            }

            arguments.Add($"\"{storePath}\"");
            arguments.Add($"\"{sourceFileName}\"");

            var argumentsString = String.Join(" ", arguments);

            var executable = $"{AppDomain.CurrentDomain.BaseDirectory}7zip{Path.DirectorySeparatorChar}7za.exe";

            var logCmd = @$"""{executable}"" {argumentsString}";
                
            if (!String.IsNullOrWhiteSpace(password))
            {
                var decryptedPassword = _dataProtector.Decrypt(password);

                logCmd = logCmd.Replace(decryptedPassword, "****");
            }

            OnProgress(jobRunTask.JobRunTaskId, $"Starting compress cmd {jobRunTask.ItemName} with parameters {logCmd}");

            var stdErrBuffer = new StringBuilder();

            try
            {
                var cmd = Cli.Wrap(executable).WithArguments(arguments, false);

                await foreach (var cmdEvent in cmd.ListenAsync(cancellationToken))
                {
                    switch (cmdEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                        {
                            var line = stdOut.Text.Trim();

                            if (!String.IsNullOrWhiteSpace(line))
                            {
                                OnProgress(jobRunTask.JobRunTaskId, line, line.Contains('%'));
                            }

                            break;
                        }
                        case StandardErrorCommandEvent stdErr:
                        {
                            var line = stdErr.Text.Trim();

                            if (!String.IsNullOrWhiteSpace(line))
                            {
                                stdErrBuffer.AppendLine(line);
                            }
                            
                            break;
                        }
                    }
                }
            }
            catch
            {
                throw new(stdErrBuffer.ToString());
            }

            sw.Stop();

            var fileSize = new FileInfo(sourceFileName).Length;
            var finalFileSize = new FileInfo(storePath).Length;
                
            var message = $"Completed compression of {jobRunTask.ItemName} from {ByteSize.FromBytes(fileSize):0.00} to {ByteSize.FromBytes(finalFileSize):0.00} ({(Int32) ((Double)finalFileSize / fileSize * 100.0) }%) in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";
                
            OnComplete(jobRunTask.JobRunTaskId, message, storePath, false);
        }
        catch (Exception ex)
        {
            var message = $"Compression {jobRunTask.ItemName} resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}