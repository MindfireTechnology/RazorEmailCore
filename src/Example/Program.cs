using System;
using RazorEmailCore;
using RazorEmailCore.SMTP;

namespace Example
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Simple Example
			var razoremail = new RazorEmail();
			razoremail.CreateAndSendEmail("nate.zaugg@SomeDomain.com", "NewUserTemplate", new { Name = "Nate Zaugg" });
			Console.WriteLine("Message sent successfully!");

			// Full Example with Separate Create and send email steps
			try
			{
				var emailBuilder = new RazorEmail();
				var email = emailBuilder.CreateEmail("NewUserTemplate", new { Name = "Nate Zaugg" });

				// Modify the email
				email.Sender = "\"Nate Zaugg\" <nate.zaugg@SomeDomain.com>";
				//...

				// Send the email
				emailBuilder.SendEmail(email);
			}
			catch (RazorEmailCoreConfigurationException rece)
			{
				Console.WriteLine($"Failed to generate message because of configuration error with RazorEmailCore! {rece}");
			}
			catch (MessageGenerationException mge)
			{
				Console.WriteLine($"Failed to generate email message because of a problem with the razor template! {mge}");
			}
			catch (SmtpException smtpEx)
			{
				Console.WriteLine($"Failed to send email message because of an SMTP error! {smtpEx}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to generate or send email because of an unknown error {ex}");
			}

			Console.WriteLine("Message sent successfully!");


			Console.ReadLine();
		}
	}
}
