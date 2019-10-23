using Penguin.Configuration.Abstractions.Interfaces;
using System;
using System.Collections.Generic;

namespace Penguin.Email
{
    internal class MailConfigurationProvider : IProvideConfigurations
    {
        public Dictionary<string, string> AllConfigurations { get; protected set; } = new Dictionary<string, string>();

        public Dictionary<string, string> AllConnectionStrings => throw new NotImplementedException();
        bool IProvideConfigurations.CanWrite => false;
        public bool ErrorOnMissingKey { get; set; }

        public MailConfigurationProvider(string From, string Login, string Password, string Server, int Port = 25, string ConfigurationName = "Default")
        {
            AddConfiguration(BuildConfiguration(From, Login, Password, Server, Port, ConfigurationName));
        }

        public MailConfigurationProvider(Dictionary<string, string> Configurations, bool errorOnMissingKey = false)
        {
            AllConfigurations = Configurations;
            ErrorOnMissingKey = errorOnMissingKey;
        }

        public static (string Name, string Value) BuildConfiguration(string From, string Login, string Password, string Server, int Port = 25, string ConfigurationName = "Default")
        {
            return ($"Email.{ConfigurationName}", $"From={From};Port={Port};Password={Password};Server={Server};Login={Login}");
        }

        public void AddConfiguration(string Name, string Value)
        {
            AddConfiguration((Name, Value));
        }

        public void AddConfiguration((string Name, string Value) toAdd)
        {
            AllConfigurations.Add(toAdd.Name, toAdd.Value);
        }

        public void AddConfiguration(string From, string Login, string Password, string Server, int Port = 25, string ConfigurationName = "Default")
        {
            AddConfiguration(BuildConfiguration(From, Login, Password, Server, Port, ConfigurationName));
        }

        public string GetConfiguration(string Key)
        {
            if (AllConfigurations.ContainsKey(Key))
            {
                return AllConfigurations[Key];
            }
            else if (ErrorOnMissingKey)
            {
                throw new KeyNotFoundException($"The requested configuration {Key} was not found in the underlying dictionary");
            }
            else
            {
                return null;
            }
        }

        public string GetConnectionString(string Name)
        {
            throw new NotImplementedException();
        }

        bool IProvideConfigurations.SetConfiguration(string Name, string Value) => throw new NotImplementedException();
    }
}