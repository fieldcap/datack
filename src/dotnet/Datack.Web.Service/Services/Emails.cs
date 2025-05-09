﻿using System.Net;
using System.Net.Mail;
using Datack.Common.Models.Data;

namespace Datack.Web.Service.Services;

public class Emails
{
    private readonly Settings _settings;

    public Emails(Settings settings)
    {
        _settings = settings;
    }

    public async Task SendComplete(JobRun jobRun, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(jobRun.Job.Settings.EmailTo))
        {
            return;
        }

        if ((!jobRun.Job.Settings.EmailOnError || !jobRun.IsError) && !jobRun.Job.Settings.EmailOnSuccess)
        {
            return;
        }

        var subject = jobRun.IsError ? $"Job {jobRun.Job.Name} failed with errors" : $"Job {jobRun.Job.Name} succesfully completed";

        var body = $"Started: {jobRun.Started.ToLocalTime():f}<br/>" +
                   $"Completed: {jobRun.Completed?.ToLocalTime():f}<br/>" +
                   $"Result: {jobRun.Result}";

        await Send(jobRun.Job.Settings.EmailTo, subject, body, cancellationToken);
    }

    public async Task SendTest(String to, CancellationToken cancellationToken)
    {
        await Send(to, "Datack test email", "This is a test email from Datack", cancellationToken);
    }

    private async Task Send(String to, String subject, String body, CancellationToken cancellationToken)
    {
        try
        {
            var smtpHost = await _settings.Get<String>("Email:Smtp:Host", cancellationToken);
            var smtpPort = await _settings.Get<Int32>("Email:Smtp:Port", cancellationToken);
            var smtpUserName = await _settings.Get<String>("Email:Smtp:UserName", cancellationToken);
            var smtpPassword = await _settings.Get<String>("Email:Smtp:Password", cancellationToken);
            var smtpUseSsl = await _settings.Get<Boolean>("Email:Smtp:UseSsl", cancellationToken);
            var smtpFrom = await _settings.Get<String>("Email:Smtp:From", cancellationToken);

            if (String.IsNullOrWhiteSpace(smtpHost))
            {
                throw new($"No e-mail host defined");
            }

            if (smtpPort == 0)
            {
                smtpPort = 25;
            }

            if (String.IsNullOrWhiteSpace(smtpFrom))
            {
                smtpFrom = "noreply@datack.local";
            }

            var smtpClient = new SmtpClient(smtpHost, smtpPort);

            if (!String.IsNullOrWhiteSpace(smtpUserName))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
            }

            smtpClient.EnableSsl = smtpUseSsl;

            var message = new MailMessage(smtpFrom, to);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;

            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                throw new($"{ex.Message}{Environment.NewLine}{ex.InnerException.Message}");
            }

            throw;
        }
    }
}