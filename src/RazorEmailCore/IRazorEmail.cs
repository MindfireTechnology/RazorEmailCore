using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public interface IRazorEmail
	{
		/// <summary>
		/// Generate an email based off a Razor template
		/// </summary>
		/// <param name="templateName">The name of the template directory</param>
		/// <param name="model">The model to pass into the template</param>
		/// <returns>Email Message with generated content</returns>
		/// <exception cref="RazorEmailCoreConfigurationException">Invalid Configuration Exception</exception>
		/// <exception cref="MessageGenerationException">Error in razor template, message generation failed.</exception>
		Email CreateEmail(string templateName, object model);

		/// <summary>
		/// Generate an email based off a Razor template and sends the email.
		/// </summary>
		/// <param name="sendTo">The email address of the person to send the email to</param>
		/// <param name="templateName">The name of the template directory</param>
		/// <param name="model">The model to pass into the template</param>
		/// <exception cref="RazorEmailCoreConfigurationException">Invalid Configuration Exception</exception>
		/// <exception cref="MessageGenerationException">Error in razor template, message generation failed.</exception>
		/// <exception cref="SmtpException">Error sending the mail message</exception>
		void CreateAndSendEmail(EmailAddress sendTo, string templateName, object model);

		/// <summary>
		/// Sends an email message using the settings configured via the email generation.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <exception cref="SmtpException">Error sending the mail message</exception>
		void SendEmail(Email message);
	}
}
