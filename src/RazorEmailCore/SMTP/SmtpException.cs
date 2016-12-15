using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore.SMTP
{
	public class SmtpException : RazorEmailCoreException
	{
		public SmtpException() { }
		public SmtpException(string message) : base(message) { }
		public SmtpException(string message, Exception inner) : base(message, inner) { }
	}
}
