using Penguin.Email.Abstractions.Interfaces;
using Penguin.Files.Abstractions;
using System.Collections.Generic;

namespace Penguin.Email
{
    /// <summary>
    /// A simple default mail message implementation
    /// </summary>
    public class MailMessage : IEmailMessage
    {
        /// <summary>
        /// Any file attachments
        /// </summary>
        public IEnumerable<IFile> Attachments { get; set; } = new List<IFile>();

        /// <summary>
        /// People to BCC
        /// </summary>
        public IEnumerable<string> BCCRecipients { get; set; } = new List<string>();

        /// <summary>
        /// The message body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// People to CC
        /// </summary>
        public IEnumerable<string> CCRecipients { get; set; } = new List<string>();

        /// <summary>
        /// The email address that the message is coming from
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Whether or not the body contains HTML content
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// Any recipients
        /// </summary>
        public IEnumerable<string> Recipients { get; set; } = new List<string>();

        /// <summary>
        /// The message subject
        /// </summary>
        public string Subject { get; set; }
    }
}