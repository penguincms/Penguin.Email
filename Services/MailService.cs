using MailKit.Net.Smtp;
using MimeKit;
using Penguin.Configuration.Abstractions.Extensions;
using Penguin.Configuration.Abstractions.Interfaces;
using Penguin.DependencyInjection.Abstractions.Interfaces;
using Penguin.Email.Abstractions.Interfaces;
using Penguin.Files.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Penguin.Email.Services
{
    /// <summary>
    /// A Service intended on sending out emails defined by an interface, pulling information required from a provided configuration provider
    /// </summary>
    public partial class MailService : ISelfRegistering, ISendMail
    {
        /// <summary>
        /// A configuration provider to retrieve configurations from
        /// </summary>
        protected IProvideConfigurations ConfigurationProvider { get; set; }

        /// <summary>
        /// Constructs a new instance of this mail service with "hardcoded" values instead of using a dynamic provider
        /// </summary>
        /// <param name="From">The address to send the mail from if not specified on the IMessage</param>
        /// <param name="Login">The SMTP server username for logging in</param>
        /// <param name="Password">The SMTP server password</param>
        /// <param name="Server">The STMP server address</param>
        /// <param name="Port">The STMP server port</param>
        /// <param name="ConfigurationName">The outgoing email address that these configurations will apply to. Should be left default unless you know what you're doing</param>
        public MailService(string From, string Login, string Password, string Server, int Port = 25, string ConfigurationName = "Default") : this(new MailConfigurationProvider(From, Login, Password, Server, Port, ConfigurationName))
        {
        }

        /// <summary>
        /// Constructs a new instance of this mail service
        /// </summary>
        /// <param name="configurationProvider">The configuration provider to get email configurations from</param>
        public MailService(IProvideConfigurations configurationProvider)
        {
            ConfigurationProvider = configurationProvider;
        }

        /// <summary>
        /// Builds a configuration name/value pair that can be inserted into a configuration provider to set up an instance of this service
        /// </summary>
        /// <param name="From">The address to send the mail from if not specified on the IMessage</param>
        /// <param name="Login">The SMTP server username for logging in</param>
        /// <param name="Password">The SMTP server password</param>
        /// <param name="Server">The STMP server address</param>
        /// <param name="Port">The STMP server port</param>
        /// <param name="ConfigurationName">The outgoing email address that these configurations will apply to. Should be left default unless you know what you're doing</param>
        /// <returns>a configuration name/value pair that can be inserted into a configuration provider to set up an instance of this service</returns>
        public static (string Name, string Value) BuildConfigurationString(string From, string Login, string Password, string Server, int Port = 25, string ConfigurationName = "Default")
        {
            return MailConfigurationProvider.BuildConfiguration(From, Login, Password, Server, Port, ConfigurationName);
        }

        /// <summary>
        /// Sends an email generated from the provided email definition
        /// </summary>
        /// <param name="message">The email definition to send</param>
        public void Send(IEmailMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Dictionary<string, string> EmailSettings = ConfigurationProvider.GetDictionary($"Email.{message.From}") ?? ConfigurationProvider.GetDictionary($"Email.Default");

            if (EmailSettings is null)
            {
                throw new NullReferenceException("Email settings not found");
            }

            string From;
            if (!string.IsNullOrWhiteSpace(message.From))
            {
                From = message.From;
            }
            else if (!EmailSettings.TryGetValue("From", out From))
            {
                throw new Exception("Email configurations not found in provider");
            }

            int Port = int.Parse(EmailSettings["Port"], NumberStyles.Integer, CultureInfo.CurrentCulture);

            using SmtpClient client = new();
            client.Connect(EmailSettings["Server"], Port, MailKit.Security.SecureSocketOptions.Auto);

            if (EmailSettings.TryGetValue("Password", out string value))
            {
                string Password = value;
                string Login = EmailSettings.TryGetValue("Login", out string login) ? login : From;

                client.Authenticate(Login, Password);
            }

            MimeMessage mailMessage = new()
            {
                Sender = MailboxAddress.Parse(From.Trim()),
                Subject = message.Subject
            };

            TextPart body = new(message.IsHtml ? MimeKit.Text.TextFormat.Html : MimeKit.Text.TextFormat.Plain) { Text = message.Body };

            mailMessage.From.Add(MailboxAddress.Parse(From.Trim()));

            foreach (string Recipient in message.Recipients)
            {
                mailMessage.To.Add(MailboxAddress.Parse(Recipient.Trim()));
            }

            if (message.CCRecipients != null)
            {
                foreach (string CCRecipient in message.CCRecipients)
                {
                    mailMessage.Cc.Add(MailboxAddress.Parse(CCRecipient.Trim()));
                }
            }

            if (message.BCCRecipients != null)
            {
                foreach (string BCCRecipient in message.BCCRecipients)
                {
                    mailMessage.Bcc.Add(MailboxAddress.Parse(BCCRecipient.Trim()));
                }
            }

            if (message.Attachments != null && message.Attachments.Any())
            {
                Multipart bodyPart = new("mixed");

                mailMessage.Body = bodyPart;

                bodyPart.Add(body);

                foreach (IFile file in message.Attachments)
                {
                    string mimeType = Penguin.Web.Data.MimeMappings.GetMimeType(Path.GetExtension(file.FullName));

                    MimePart attachment = new(mimeType)
                    {
                        Content = new MimeContent(new MemoryStream(file.Data)),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(file.FullName)
                    };

                    bodyPart.Add(attachment);
                }
            }
            else
            {
                mailMessage.Body = body;
            }

            client.Send(mailMessage);
        }
    }
}