using System.Diagnostics;
using System.Text;
using ByteSizeLib;
using CliWrap;
using CliWrap.EventStream;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task extracts a 7z file.
/// </summary>
public class ExtractTask : BaseTask
{
    private readonly DataProtector _dataProtector;

    public ExtractTask(DataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public override async Task Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();

            if (previousTask == null)
            {
                throw new Exception("No previous task found");
            }

            if (jobRunTask.Settings.Extract == null)
            {
                throw new Exception("No settings set");
            }

            var sourceFileName = previousTask.ResultArtifact;

            OnProgress(jobRunTask.JobRunTaskId, $"Starting extraction task for file {sourceFileName}");

            if (String.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new Exception($"No source file found");
            }

            if (!File.Exists(sourceFileName))
            {
                throw new Exception($"Source file '{sourceFileName}' not found");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started
            };

            var rawFileName = Path.GetFileName(jobRunTask.Settings.Extract.FileName);

            if (String.IsNullOrWhiteSpace(rawFileName))
            {
                throw new Exception($"Invalid filename '{jobRunTask.Settings.Extract.FileName}'");
            }

            var fileName = rawFileName.FormatFromObject(tokenValues);

            var rawFilePath = Path.GetDirectoryName(jobRunTask.Settings.Extract.FileName);

            if (String.IsNullOrWhiteSpace(rawFilePath))
            {
                throw new Exception($"Invalid file path '{jobRunTask.Settings.Extract.FileName}'");
            }

            var filePath = rawFilePath.FormatFromObject(tokenValues);

            var storePath = Path.Combine(filePath, fileName);

            var tempPath = Path.Combine(filePath, Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempPath);

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

            var archiveType = jobRunTask.Settings.Extract.ArchiveType;
            var multithreadMode = jobRunTask.Settings.Extract.MultithreadMode;
            var password = jobRunTask.Settings.Extract.Password;

            var arguments = new List<String>
            {
                // Extract files from archive
                "e",
                // Set output streams
                "-bso1 -bse2 -bsp1",
                // Multithread mode
                $"-mmt={multithreadMode}",
                // Extraction type
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

            arguments.Add($"-o\"{tempPath}\"");
            arguments.Add($"\"{sourceFileName}\"");

            var argumentsString = String.Join(" ", arguments);

            var executable = $"{AppDomain.CurrentDomain.BaseDirectory}7zip{Path.DirectorySeparatorChar}7za.exe";

            var logCmd = @$"""{executable}"" {argumentsString}";
                
            if (!String.IsNullOrWhiteSpace(password))
            {
                var decryptedPassword = _dataProtector.Decrypt(password);

                logCmd = logCmd.Replace(decryptedPassword, "****");
            }

            OnProgress(jobRunTask.JobRunTaskId, $"Starting extract cmd {jobRunTask.ItemName} with parameters {logCmd}");

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
                throw new Exception(stdErrBuffer.ToString());
            }

            var resultingFiles = Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories);
            
            if (resultingFiles.Length == 0)
            {
                throw new Exception("No files extracted");
            }

            if (resultingFiles.Length > 1)
            {
                throw new Exception($"Multiple files extracted: {String.Join(", ", resultingFiles)}");
            }

            var resultingFile = resultingFiles[0];

            File.Move(resultingFile, storePath, true);

            Directory.Delete(tempPath, true);

            sw.Stop();

            var fileSize = new FileInfo(sourceFileName).Length;
            var finalFileSize = new FileInfo(storePath).Length;
                
            var message = $"Completed extraction of {jobRunTask.ItemName} from {ByteSize.FromBytes(fileSize):0.00} to {ByteSize.FromBytes(finalFileSize):0.00} in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";
            
            OnComplete(jobRunTask.JobRunTaskId, message, storePath, false);
        }
        catch (Exception ex)
        {
            var message = $"Extraction {jobRunTask.ItemName} resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}