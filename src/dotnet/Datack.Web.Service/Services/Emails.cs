using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;

namespace Datack.Web.Service.Services
{
    public class Emails
    {
        private readonly Settings _settings;

        public Emails(Settings settings)
        {
            _settings = settings;
        }

        public async Task SendComplete(JobRun jobRun, CancellationToken cancellationToken)
        {
            if ((!jobRun.Job.Settings.EmailOnError || !jobRun.IsError) && !jobRun.Job.Settings.EmailOnSuccess)
            {
                return;
            }

            var subject = jobRun.IsError ? $"Job {jobRun.Job.Name} failed with error" : $"Job {jobRun.Job.Name} succesfully completed";

            await Send(jobRun.Job.Settings.EmailTo, subject, subject, cancellationToken);
        }

        public async Task SendTest(String to, CancellationToken cancellationToken)
        {
            await Send(to, "Datack test email", "This is a test email from Datack", cancellationToken);
        }

        private async Task Send(String to, String subject, String body, CancellationToken cancellationToken)
        {
            var smtpHost = await _settings.Get<String>("Email:Smtp:Host", cancellationToken);
            var smtpPort = await _settings.Get<Int32>("Email:Smtp:Port", cancellationToken);
            var smtpUserName = await _settings.Get<String>("Email:Smtp:UserName", cancellationToken);
            var smtpPassword = await _settings.Get<String>("Email:Smtp:Password", cancellationToken);
            var smtpUseSsl = await _settings.Get<Boolean>("Email:Smtp:UseSsl", cancellationToken);
            var smtpFrom = await _settings.Get<String>("Email:Smtp:From", cancellationToken);

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
    }
}
