using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class RazorEmail : IRazorEmail
	{
		public ConfigSettings Config { get; set; }
		public IMessageSettingsProvider MessageSettingsProvider { get; set; }
		public IMessageGenerator MessageGenerator { get; set; }
		public ISendEmailProvider SendEmailProvider { get; set; }

		public RazorEmail()
		{
			// Use defaults
			MessageSettingsProvider = new DefaultMessageSettingsProvider();
			MessageGenerator = new RazorMessageGenerator();
			SendEmailProvider = new SmtpSendEmailProvider();
		}

		public RazorEmail(IMessageSettingsProvider settingsProvider = null, IMessageGenerator messageGenerator = null, ISendEmailProvider sendEmailProvider = null)
		{
			MessageSettingsProvider = settingsProvider ?? new DefaultMessageSettingsProvider();
			MessageGenerator = messageGenerator ?? new RazorMessageGenerator();
			SendEmailProvider = sendEmailProvider ?? new SmtpSendEmailProvider();
		}


		/// <summary>
		/// Generate an email based off a Razor template
		/// </summary>
		/// <param name="templateName">The name of the template directory</param>
		/// <param name="model">The model to pass into the template</param>
		/// <returns>Email Message with generated content</returns>
		/// <exception cref="RazorEmailCoreConfigurationException">Invalid Configuration Exception</exception>
		/// <exception cref="MessageGenerationException">Error in razor template, message generation failed.</exception>
		public virtual Email CreateEmail(string templateName, object model)
		{
			string htmlMessage = null;
			string textMessage = null;

			if (!string.IsNullOrWhiteSpace(Config.HtmlEmailTemplate))
				htmlMessage = MessageGenerator.GenerateHtmlMessageBody(MessageSettingsProvider.BasePath, Config.HtmlEmailTemplate, model);

			if (!string.IsNullOrWhiteSpace(Config.PlainTextEmailTemplate))
				textMessage = MessageGenerator.GenerateTextMessageBody(MessageSettingsProvider.BasePath, Config.PlainTextEmailTemplate, model);

			var result = new Email { HtmlBody = htmlMessage, PlainTextBody = textMessage, Subject = Config.Subject, Sender = Config.From };

			Config.Cc?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(n => result.Cc.Add(n));
			Config.Bcc?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(n => result.Bcc.Add(n));			

			return result;
		}

		/// <summary>
		/// Generate an email based off a Razor template and sends the email.
		/// </summary>
		/// <param name="sendTo">The email address of the person to send the email to</param>
		/// <param name="templateName">The name of the template directory</param>
		/// <param name="model">The model to pass into the template</param>
		/// <exception cref="RazorEmailCoreConfigurationException">Invalid Configuration Exception</exception>
		/// <exception cref="MessageGenerationException">Error in razor template, message generation failed.</exception>
		/// <exception cref="SmtpException">Error sending the mail message</exception>
		public void CreateAndSendEmail(EmailAddress sendTo, string templateName, object model)
		{
			Email message = CreateEmail(templateName, model);
			message.To.Add(sendTo);

			if (!message.MessageComplete)
				throw new RazorEmailCoreException("Message is incomplete and cannot be sent.");

			SendEmailProvider.SendMessage(message, Config);
		}

		/// <summary>
		/// Generate an email asynchroniously based off a Razor template and sends the email.
		/// </summary>
		/// <param name="sendTo">The email address of the person to send the email to</param>
		/// <param name="templateName">The name of the template directory</param>
		/// <param name="model">The model to pass into the template</param>
		/// <exception cref="RazorEmailCoreConfigurationException">Invalid Configuration Exception</exception>
		/// <exception cref="MessageGenerationException">Error in razor template, message generation failed.</exception>
		/// <exception cref="SmtpException">Error sending the mail message</exception>
		public Task CreateAndSendEmailAsync(EmailAddress sendTo, string templateName, object model)
		{
			Email message = CreateEmail(templateName, model);
			message.To.Add(sendTo);

			if (!message.MessageComplete)
				throw new RazorEmailCoreException("Message is incomplete and cannot be sent.");

			return SendEmailProvider.SendMessageAsync(message, Config);
		}


		/// <summary>
		/// Sends an email message using the settings configured via the email generation.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <exception cref="SmtpException">Error sending the mail message</exception>
		public void SendEmail(Email message)
		{
			SendEmailProvider.SendMessage(message, Config);
		}


		/// <summary>
		/// Sends an email message asynchroniously using the settings configured via the email generation.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <exception cref="SmtpException">Error sending the mail message</exception>
		public Task SendEmailAsync(Email message)
		{
			return SendEmailProvider.SendMessageAsync(message, Config);
		}
	}
}
