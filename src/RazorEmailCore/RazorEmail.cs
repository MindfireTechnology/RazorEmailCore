using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class RazorEmail : IRazorEmail
	{
		public IMessageSettingsProvider MessageSettingsProvider { get; set; }
		public IMessageGenerator MessageGenerator { get; set; }
		public ISendEmailProvider SendEmailProvider { get; set; }

		public RazorEmail()
		{
			// Use defaults
			MessageSettingsProvider = new DefaultMessageSettingsProvider();
		}

		public Email CreateEmail()
		{
			throw new NotImplementedException();
		}

		public Email CreateEmail(string templateName, object model)
		{
			throw new NotImplementedException();
		}
	}
}
