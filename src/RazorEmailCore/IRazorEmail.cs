using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public interface IRazorEmail
	{
		Email CreateEmail(string templateName, object model);
		Email CreateEmail();
	}
}
