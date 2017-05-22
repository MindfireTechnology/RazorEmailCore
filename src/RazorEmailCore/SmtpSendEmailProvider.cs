using RazorEmailCore.SMTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class SmtpSendEmailProvider : ISendEmailProvider
	{
		public void SendMessage(Email message, ConfigSettings settings)
		{
			var client = new SmtpClient(settings.Server, settings.Username, settings.Password);
			client.SendMessage(message);
		}

		public Task SendMessageAsync(Email message, ConfigSettings settings)
		{
			var client = new SmtpClient(settings.Server, settings.Username, settings.Password);
			return client.SendMessageAsync(message);
		}
	}
}
