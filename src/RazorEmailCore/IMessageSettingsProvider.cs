using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public interface IMessageSettingsProvider
	{
		ConfigSettings LoadByTemplateName(string templateName);
	}
}
