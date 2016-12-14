using System;
using System.Linq;

namespace RazorEmailCore
{
	public class EmailAddress
	{
		private string email;

		public string Name { get; set; }

		public string Email {
			get { return email; }
			set
			{
				if (string.IsNullOrWhiteSpace(value) 
					|| value.Length < 3 
					|| !value.Contains('@') 
					|| !value.Contains('.')
					|| value.IndexOf('@') < value.LastIndexOf('.'))
					throw new FormatException("Email address contains an invalid format");

				email = value;
			}
		}

		public EmailAddress() { }

		public EmailAddress(string name, string email)
		{
			Name = name;
			Email = email;
		}

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Email))
				return string.Empty;

			if (!string.IsNullOrWhiteSpace(Name))
				return $"\"{Name}\" <{Email}>";
			else
				return Email;
		}

		//public static EmailAddress Parse(string value)
		//{
			// Accepts the format "Name In Quotes" <emaail@here.com>
		//}

		public static implicit operator EmailAddress(string value)
		{
			return new EmailAddress(string.Empty, value);
		}

		public static explicit operator string(EmailAddress value)
		{
			return value.ToString();
		}
	}
}
