using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;

namespace RazorEmailCore
{
	public class DefaultMessageSettingsProvider : IMessageSettingsProvider
	{
		public string BasePath { get; set; }



		public string BCC { get; set; }

		public string CC { get; set; }

		public string DisplayName { get; set; }

		public string From { get; set; }

		public string HtmlEmailTemplate { get; set; }

		public string PlainTextEmailTemplate { get; set; }

		public string Subject { get; set; }

		public DefaultMessageSettingsProvider()
		{
			string path = Environment.GetEnvironmentVariable("BaseTemplatePath");
			BasePath = Path.Combine(Directory.GetCurrentDirectory(), path);
		}

		public void LoadByName(string templateName)
		{
			// Check to see if there is a directory with the template name
			string dirpath = BasePath;
			if (Directory.Exists(Path.Combine(BasePath, templateName)))
				dirpath = Path.Combine(BasePath, templateName);

			// Look for the correct files (.json, .razor, .text)

			string jsonFile;
			string razorFile;
			string textFile;

			try
			{
				jsonFile = Directory.EnumerateFiles(dirpath, "*.json").Single();
				razorFile = Directory.EnumerateFiles(dirpath, "*.razor").SingleOrDefault();
				textFile = Directory.EnumerateFiles(dirpath, "*.text").SingleOrDefault();
			}
			catch (InvalidOperationException ioex)
			{
				throw new RazorEmailCoreConfigurationException("One or more expected configuration files are missing or invalid (possibly duplicated).", ioex);
			}

			// Read the configuration in the json file
			ConfigSettings settings;
			try
			{
				var serializer = new DataContractJsonSerializer(typeof(ConfigSettings));
				using (var fs = File.OpenRead(jsonFile))
					settings = (ConfigSettings)serializer.ReadObject(fs);

				MapSettings(settings);
			}
			catch (Exception ex)
			{
				throw new RazorEmailCoreConfigurationException($"Error reading configuration file: {jsonFile}", ex);
			}

			if (!string.IsNullOrWhiteSpace(razorFile))
				HtmlEmailTemplate = File.ReadAllText(razorFile);

			if (!string.IsNullOrWhiteSpace(textFile))
				PlainTextEmailTemplate = File.ReadAllText(textFile);
		}

		private void MapSettings(ConfigSettings settings)
		{
			From = settings.from;
			DisplayName = settings.displayName;
			Subject = settings.subject;
			CC = settings.cc;
			BCC = settings.bcc;
		}
	}

	public class ConfigSettings
	{
		public string from { get; set; }
		public string displayName { get; set; }
		public string subject { get; set; }
		public string cc { get; set; }
		public string bcc { get; set; }
	}
}
