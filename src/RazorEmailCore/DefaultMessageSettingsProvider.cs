using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;

namespace RazorEmailCore
{
	public class DefaultMessageSettingsProvider : IMessageSettingsProvider
	{
		public string BasePath { get; set; }

		public DefaultMessageSettingsProvider()
		{
			string path = Environment.GetEnvironmentVariable("BaseTemplatePath");
			BasePath = Path.Combine(Directory.GetCurrentDirectory(), path);
		}

		public ConfigSettings LoadByTemplateName(string templateName)
		{
			ConfigSettings baseSettings = null;
			ConfigSettings templateSettings = null;

			// Check to see if there is a directory with the template name
			string dirpath = BasePath;
			if (Directory.Exists(Path.Combine(BasePath, templateName)))
				dirpath = Path.Combine(BasePath, templateName);

			// Look for the correct files (.json, .razor, .text)
			string baseSettingsFile;
			string templateSettingsFile;
			string razorFile;
			string textFile;

			try
			{
				baseSettingsFile = Directory.EnumerateFiles(BasePath, "*.json").Single();
				templateSettingsFile = Directory.EnumerateFiles(dirpath, "*.json").SingleOrDefault();
				razorFile = Directory.EnumerateFiles(dirpath, "*.razor").SingleOrDefault();
				textFile = Directory.EnumerateFiles(dirpath, "*.text").SingleOrDefault();
			}
			catch (InvalidOperationException ioex)
			{
				throw new RazorEmailCoreConfigurationException("One or more expected configuration files are missing or invalid (possibly duplicated).", ioex);
			}

			// Read the configuration in the json file for base and template (if it exists)
			baseSettings = ParseSettings(baseSettingsFile);
			if (!string.IsNullOrEmpty(templateSettingsFile))
				templateSettings = ParseSettings(templateSettingsFile);

			// if a template file exists, merge the two config files,
			// with the template overriding settings in the base,
			// if they're not already the same
			if (templateSettings != null && !string.Equals(baseSettingsFile, templateSettingsFile))
				ConfigSettings.ReplacePublicStringProperties(source: templateSettings, target: baseSettings);

			// get the template text from the template files, if possible
			if (!string.IsNullOrWhiteSpace(razorFile))
				baseSettings.HtmlEmailTemplate = File.ReadAllText(razorFile);

			if (!string.IsNullOrWhiteSpace(textFile))
				baseSettings.PlainTextEmailTemplate = File.ReadAllText(textFile);

			return baseSettings;
		}

		private static ConfigSettings ParseSettings(string jsonFile)
		{
			try
			{
				var serializer = new DataContractJsonSerializer(typeof(ConfigSettings));
				using (var fs = File.OpenRead(jsonFile))
					return (ConfigSettings)serializer.ReadObject(fs);
			}
			catch (Exception ex)
			{
				throw new RazorEmailCoreConfigurationException($"Error reading configuration file: {jsonFile}", ex);
			}
		}
	}
}