using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class Email
	{
		public Encoding TextEncoding { get; set; } = Encoding.UTF8;

		public EmailAddress Sender { get; set; }

		public ICollection<EmailAddress> To { get; protected set; } = new List<EmailAddress>();
		public ICollection<EmailAddress> Cc { get; protected set; } = new List<EmailAddress>();
		public ICollection<EmailAddress> Bcc { get; protected set; } = new List<EmailAddress>();

		public string Subject { get; set; }

		public string PlainTextBody { get; set; }
		public string HtmlBody { get; set; }

		public bool MessageComplete
		{
			get
			{
				return !string.IsNullOrWhiteSpace((string)Sender)
					&& To.Count > 0
					&& !string.IsNullOrWhiteSpace(Subject)
					&& (!string.IsNullOrWhiteSpace(PlainTextBody) || !string.IsNullOrWhiteSpace(HtmlBody));
			}
		}

		public Email() { }

		public Email(EmailAddress to, string subject, string htmlBody = null, string textBody = null)
		{
			To.Add(to);
			Subject = subject;
			HtmlBody = htmlBody;
			PlainTextBody = PlainTextBody;
		}
	}
}
