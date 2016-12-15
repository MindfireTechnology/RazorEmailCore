using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class NullSendEmailProvider : ISendEmailProvider

	{
		public void SendMessage(Email message, ConfigSettings settings)
		{
		}

		public Task SendMessageAsync(Email message, ConfigSettings settings)
		{
			return Task.FromResult(0);
		}
	}
}
