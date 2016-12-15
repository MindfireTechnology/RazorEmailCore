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
		public bool SendMessage(Email message, ConfigSettings settings)
		{
			try
			{
				var client = new SmtpClient(settings.Server, settings.Username, settings.Password);
				return client.SendMessage(message);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error sending message! \r\n {ex}");
				return false;
			}
		}

		public Task<bool> SendMessageAsync(Email message, ConfigSettings settings)
		{
			// This is just faked right now, but one day I'd like to do this async!
			return Task.Run(() => SendMessage(message, settings));
		}
	}
}
