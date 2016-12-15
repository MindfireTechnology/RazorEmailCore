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
			ConfigSettings result = null;

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
			try
			{
				var serializer = new DataContractJsonSerializer(typeof(ConfigSettings));
				using (var fs = File.OpenRead(jsonFile))
					result = (ConfigSettings)serializer.ReadObject(fs);
			}
			catch (Exception ex)
			{
				throw new RazorEmailCoreConfigurationException($"Error reading configuration file: {jsonFile}", ex);
			}

			if (!string.IsNullOrWhiteSpace(razorFile))
				result.HtmlEmailTemplate = File.ReadAllText(razorFile);

			if (!string.IsNullOrWhiteSpace(textFile))
				result.PlainTextEmailTemplate = File.ReadAllText(textFile);

			return result;
		}

	}
}
