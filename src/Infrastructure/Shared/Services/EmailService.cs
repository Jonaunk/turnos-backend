﻿using Application.Common.Interfaces;
using Application.Common.Mailing;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Shared.Mailing;


namespace Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _settings;

        public EmailService(IOptions<MailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(MailRequest request, CancellationToken cancellationToken = default)
        {
            //var emailClient = new SmtpClient("smtp.gmail.com");
            //emailClient.EnableSsl = true;
            ////utvj ggdz lvwb kilf
            //// Get your Gmail username and password.
            //var userName = "1994elmaty@gmail.com";
            //var password = "felv maor vlck vjtd";

            //// Create a NetworkCredential object.
            //var networkCredential = new NetworkCredential(userName, password);

            //// Set the credentials on the SmtpClient instance.
            //emailClient.Credentials = networkCredential;

            //var message = new MailMessage
            //{
            //    From = new MailAddress(request.From),
            //    Subject = request.Subject,
            //    Body = request.Body
            //};
            //message.To.Add(new MailAddress(request.To));
            //await emailClient.SendMailAsync(message);

            try
            {
                var email = new MimeMessage();

                // From
                email.From.Add(new MailboxAddress(_settings.DisplayName, request.From ?? _settings.From));

                // To
                foreach (string address in request.To)
                    email.To.Add(MailboxAddress.Parse(address));

                // Reply To
                if (!string.IsNullOrEmpty(request.ReplyTo))
                    email.ReplyTo.Add(new MailboxAddress(request.ReplyToName, request.ReplyTo));

                // Bcc
                if (request.Bcc != null)
                {
                    foreach (string address in request.Bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                        email.Bcc.Add(MailboxAddress.Parse(address.Trim()));
                }

                // Cc
                if (request.Cc != null)
                {
                    foreach (string? address in request.Cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                        email.Cc.Add(MailboxAddress.Parse(address.Trim()));
                }

                // Headers
                if (request.Headers != null)
                {
                    foreach (var header in request.Headers)
                        email.Headers.Add(header.Key, header.Value);
                }

                // Content
                var builder = new BodyBuilder();
                email.Sender = new MailboxAddress(request.DisplayName ?? _settings.DisplayName, request.From ?? _settings.From);
                email.Subject = request.Subject;
                builder.HtmlBody = request.Body;

                // Create the file attachments for this e-mail message
                if (request.AttachmentData != null)
                {
                    foreach (var attachmentInfo in request.AttachmentData)
                        builder.Attachments.Add(attachmentInfo.Key, attachmentInfo.Value);
                }

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await smtp.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
                await smtp.SendAsync(email, cancellationToken);
                await smtp.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, ex.Message);
            }
        }
    }

}
