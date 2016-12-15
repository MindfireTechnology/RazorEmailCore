using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	[DataContract]
	public class ConfigSettings
	{
		[DataMember(Name ="from")]
		public string From { get; set; }
		[DataMember(Name = "subject")]
		public string Subject { get; set; }
		[DataMember(Name = "cc")]
		public string Cc { get; set; }
		[DataMember(Name = "bcc")]
		public string Bcc { get; set; }

		[DataMember(Name = "server")]
		public string Server { get; set; }
		[DataMember(Name = "username")]
		public string Username { get; set; }
		[DataMember(Name = "password")]
		public string Password { get; set; }

		public string PlainTextEmailTemplate { get; set; }
		public string HtmlEmailTemplate { get; set; }
	}
}
