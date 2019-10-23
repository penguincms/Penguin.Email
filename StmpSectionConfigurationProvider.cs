using Penguin.Configuration.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Penguin.Email
{
    /// <summary>
    /// Pulls STMP settings from the Web.Config mail section
    /// </summary>
    public class StmpSectionConfigurationProvider : IProvideConfigurations
    {
        /// <summary>
        /// Only contains the configuration string for the email provider
        /// </summary>
        public Dictionary<string, string> AllConfigurations { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Not implemented
        /// </summary>
        public Dictionary<string, string> AllConnectionStrings { get; } = new Dictionary<string, string>();

        bool IProvideConfigurations.CanWrite => false;

        /// <summary>
        /// Constructs a new instance of this provider
        /// </summary>
        public StmpSectionConfigurationProvider()
        {
            using (SmtpClient client = new SmtpClient())
            {
                System.Net.NetworkCredential credential = (System.Net.NetworkCredential)client.Credentials;
                (string Name, string Value) = MailConfigurationProvider.BuildConfiguration(credential.UserName, credential.UserName, credential.Password, client.Host, client.Port);
                AllConfigurations.Add(Name, Value);
            }
        }

        /// <summary>
        /// Only returns the default email configuration from the web.config
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string GetConfiguration(string Key)
        {
            if (AllConfigurations.ContainsKey(Key))
            {
                return AllConfigurations[Key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public string GetConnectionString(string Name)
        {
            throw new NotImplementedException();
        }

        bool IProvideConfigurations.SetConfiguration(string Name, string Value) => throw new NotImplementedException();
    }
}