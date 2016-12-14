using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public interface IMessageSettingsProvider
	{
		string From { get; set; }
		string DisplayName { get; set; }
		string Subject { get; set; }
		string CC { get; set; }
		string BCC { get; set; }

		string PlainTextEmailTemplate { get; set; }
		string HtmlEmailTemplate { get; set; }

		void LoadByName(string templateName);
	}
}
