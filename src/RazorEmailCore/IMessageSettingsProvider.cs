using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	/// <summary>
	/// Class used to pull configuration settings for a given template
	/// </summary>
	public interface IMessageSettingsProvider
	{
		string BasePath { get; set; }

		/// <summary>
		/// Retreives the config settings based on the template name
		/// </summary>
		/// <param name="templateName">The name of the template we require settings for</param>
		/// <returns>ConfigSettings (or inherited) object</returns>
		ConfigSettings LoadByTemplateName(string templateName);
	}
}
