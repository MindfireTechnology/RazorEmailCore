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
			if (settingsProvider != null)
				MessageSettingsProvider = settingsProvider;
			else
				MessageSettingsProvider = new DefaultMessageSettingsProvider();


			if (messageGenerator != null)
				MessageGenerator = messageGenerator;
			else
				MessageGenerator = new RazorMessageGenerator();


			if (sendEmailProvider != null)
				SendEmailProvider = sendEmailProvider;
			else
				SendEmailProvider = new SmtpSendEmailProvider();
		}

		public virtual Email CreateEmail(string templateName, object model)
		{
			Config = MessageSettingsProvider.LoadByTemplateName(templateName);

			string htmlMessage = null;
			string textMessage = null;

			if (!string.IsNullOrWhiteSpace(Config.HtmlEmailTemplate))
				htmlMessage = MessageGenerator.GenerateMessageBody(Config.HtmlEmailTemplate, model);

			if (!string.IsNullOrWhiteSpace(Config.PlainTextEmailTemplate))
				textMessage = MessageGenerator.GenerateMessageBody(Config.PlainTextEmailTemplate, model);

			var result = new Email { HtmlBody = htmlMessage, PlainTextBody = textMessage, Subject = Config.Subject, Sender = Config.From };

			Config.Cc?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(n => result.Cc.Add(n));
			Config.Bcc?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(n => result.Bcc.Add(n));			

			return result;
		}

		public bool CreateAndSendEmail(EmailAddress sendTo, string templateName, object model)
		{
			Email message = CreateEmail(templateName, model);
			message.To.Add(sendTo);

			if (!message.MessageComplete)
				throw new RazorEmailCoreException("Message is incomplete and cannot be sent.");

			return SendEmailProvider.SendMessage(message, Config);
		}
	}
}
