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
					|| value.IndexOf('@') > value.LastIndexOf('.'))
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

		public static EmailAddress Parse(string value)
		{
			//Accepts the format "Name In Quotes" <email@here.com>
			if (value.StartsWith("\"") && value.IndexOf("\"", 1) > 0)
			{
				EmailAddress result = new EmailAddress();

				// Get the value between the quotes
				string name = value.Substring(1, value.LastIndexOf('"')-1);
				result.Name = name.Trim('"').Trim();

				// Get the value between the angle brackets
				int startpos = value.IndexOf('<');
				int length = value.LastIndexOf('>') - startpos;
				string email = value.Substring(startpos+1, length-1);
				result.email = email.Trim().Trim('<').Trim('>');
				return result;
			}
			else
			{
				return new EmailAddress(string.Empty, value);
			}
		}

		public static implicit operator EmailAddress(string value)
		{
			return EmailAddress.Parse(value);
		}

		public static explicit operator string(EmailAddress value)
		{
			return value?.ToString();
		}
	}
}
