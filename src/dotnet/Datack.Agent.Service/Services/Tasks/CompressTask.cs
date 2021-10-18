using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks
{
    /// <summary>
    /// This task compresses files with 7z.
    /// </summary>
    public class CompressTask : BaseTask
    {
        public override async Task Run(Server server, JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                if (previousTask == null)
                {
                    throw new Exception("No previous task found");
                }

                var sourceFileName = previousTask.ResultArtifact;

                OnProgress(jobRunTask.JobRunTaskId, $"Starting compression task for file {sourceFileName}");

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
                    DatabaseName = jobRunTask.ItemName
                };

                var rawFileName = Path.GetFileName(jobRunTask.Settings.Compress.FileName);

                if (String.IsNullOrWhiteSpace(rawFileName))
                {
                    throw new Exception($"Invalid filename '{jobRunTask.Settings.Compress.FileName}'");
                }

                var fileName = rawFileName.FormatToken(tokenValues);
                fileName = String.Format(fileName, jobRunTask.JobRun.Started);

                var rawFilePath = Path.GetDirectoryName(jobRunTask.Settings.Compress.FileName);

                if (String.IsNullOrWhiteSpace(rawFilePath))
                {
                    throw new Exception($"Invalid file path '{jobRunTask.Settings.Compress.FileName}'");
                }

                var filePath = rawFilePath.FormatToken(tokenValues);
                filePath = String.Format(filePath, jobRunTask.JobRun.Started);

                var storePath = Path.Combine(filePath, fileName);

                var resultArtifact = storePath;

                OnProgress(jobRunTask.JobRunTaskId, $"Testing path {storePath}");

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                if (File.Exists(fileName))
                {
                    throw new Exception($"Cannot create file, file '{storePath}' already exists");
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
                    // Encrypt header
                    arguments.Add("-mhe");

                    // Password
                    arguments.Add(@$"-p""{password}""");
                }

                arguments.Add($"\"{storePath}\"");
                arguments.Add($"\"{sourceFileName}\"");

                var argumentsString = String.Join(" ", arguments);

                var executable = $"{AppDomain.CurrentDomain.BaseDirectory}7zip{Path.DirectorySeparatorChar}7za.exe";

                var cmd = @$"""{executable}"" {argumentsString}";
                
                if (!String.IsNullOrWhiteSpace(password))
                {
                    cmd = cmd.Replace(password, "****");
                }

                OnProgress(jobRunTask.JobRunTaskId, $"Starting compress cmd {jobRunTask.ItemName} with parameters {cmd}");
                
                var stdErr = new StringBuilder();

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = executable,
                        Arguments = argumentsString,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = false,
                    };

                    process.EnableRaisingEvents = true;

                    process.OutputDataReceived += (_, args) =>
                    {
                        var line = args.Data?.Trim();

                        if (String.IsNullOrWhiteSpace(line))
                        {
                            return;
                        }

                        OnProgress(jobRunTask.JobRunTaskId, line, true);
                    };

                    process.ErrorDataReceived += (_, args) =>
                    {
                        var line = args.Data?.Trim();

                        if (String.IsNullOrWhiteSpace(line))
                        {
                            return;
                        }

                        stdErr.AppendLine(line);
                    };

                    process.Start();

                    process.BeginOutputReadLine();

                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync(cancellationToken);
                }

                if (stdErr.Length > 0)
                {
                    throw new Exception(stdErr.ToString());
                }

                sw.Stop();

                var fileSize = new FileInfo(sourceFileName).Length;
                var finalFileSize = new FileInfo(storePath).Length;
                
                var message = $"Completed compression of {jobRunTask.ItemName} from {ByteSize.FromBytes(fileSize):0.00} to {ByteSize.FromBytes(finalFileSize):0.00} ({(Int32) ((Double)finalFileSize / fileSize * 100.0) }%) in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";
                
                OnComplete(jobRunTask.JobRunTaskId, message, resultArtifact, false);
            }
            catch (Exception ex)
            {
                var message = $"Compression {jobRunTask.ItemName} resulted in an error: {ex.Message}";

                OnComplete(jobRunTask.JobRunTaskId, message, null, true);
            }
        }
    }
}
