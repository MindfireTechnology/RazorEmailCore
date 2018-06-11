using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	/// <summary>
	/// This class is responsible for message generation
	/// </summary>
	public interface IMessageGenerator
	{
		/// <summary>
		/// Create the message based on the given template text and model.
		/// </summary>
		/// <param name="template">The template markup as a string</param>
		/// <param name="model">The model to use to generate the markup</param>
		/// <returns>The generated message</returns>
		string GenerateMessageBody(string basePath, string templateName, object model);

		/// <summary>
		/// Create the message based on the given template text and model.
		/// </summary>
		/// <param name="template">The template markup as a string</param>
		/// <param name="model">The model to use to generate the markup</param>
		/// <returns>The generated message</returns>
		string GenerateMessageBody<T>(string basePath, string templateName, T model);
	}
}
