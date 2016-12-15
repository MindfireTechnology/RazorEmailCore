using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class NullSendEmailProvider : ISendEmailProvider

	{
		public bool SendMessage(Email message, ConfigSettings settings)
		{
			return true;
		}

		public Task<bool> SendMessageAsync(Email message, ConfigSettings settings)
		{
			return Task.FromResult(true);
		}
	}
}
