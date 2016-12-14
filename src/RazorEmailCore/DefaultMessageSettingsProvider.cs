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

		public string HtmlEmailTemplatePath { get; set; }

		public string PlainTextEmailTemplatePath { get; set; }

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
			string jsonFile = Directory.EnumerateFiles(dirpath, "*.json").Single();
			string razorFile = Directory.EnumerateFiles(dirpath, "*.razor").SingleOrDefault();
			string textFile = Directory.EnumerateFiles(dirpath, "*.text").SingleOrDefault();

			var serializer = new DataContractJsonSerializer(this.GetType());
			serializer.ReadObject()
		}
	}
}
