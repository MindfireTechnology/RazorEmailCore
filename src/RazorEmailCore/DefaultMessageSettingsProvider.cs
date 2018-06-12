using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace RazorEmailCore
{
	public class DefaultMessageSettingsProvider : IMessageSettingsProvider
	{
		public string BasePath { get; set; }

		public DefaultMessageSettingsProvider()
		{
			var path = Environment.GetEnvironmentVariable("BaseTemplatePath");
			BasePath = Path.Combine(Directory.GetCurrentDirectory(), path);
		}

		public virtual ConfigSettings LoadByTemplateName(string templateName)
		{
			ConfigSettings baseSettings = null;
			ConfigSettings templateSettings = null;

			// Check to see if there is a directory with the template name
			var dirpath = BasePath;
			if (Directory.Exists(Path.Combine(BasePath, templateName)))
				dirpath = Path.Combine(BasePath, templateName);

			// Look for the correct files (.json, .razor, .text)
			string baseSettingsFile;
			string templateSettingsFile;

			try
			{
				baseSettingsFile = Directory.EnumerateFiles(BasePath, "*.json").Single();
				templateSettingsFile = Directory.EnumerateFiles(dirpath, "*.json").SingleOrDefault();
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

			return baseSettings;
		}

		private static ConfigSettings ParseSettings(string jsonFile)
		{
			try
			{
				return JsonConvert.DeserializeObject<ConfigSettings>(File.ReadAllText(jsonFile));
			}
			catch (Exception ex)
			{
				throw new RazorEmailCoreConfigurationException($"Error reading configuration file: {jsonFile}", ex);
			}
		}
	}
}