using Example2.TemplateModels;
using RazorEmailCore;
using RazorEmailCore.SMTP;
using System;

namespace Example2
{
	class Program
	{
		static void Main(string[] args)
		{
			string templateName = "NewUserTemplate",
				senderEmail = "nate.zaugg@SomeDomain.com",
				receiverName = "Nate Zaugg";

			// Simple Example
			var razoremail = new RazorEmail();
			razoremail.CreateAndSendEmail(senderEmail, templateName, new NewUserModel { Name = receiverName });
			Console.WriteLine("Message sent successfully!");

			// Full Example with Separate Create and send email steps
			try
			{
				var emailBuilder = new RazorEmail();
				var email = emailBuilder.CreateEmail(templateName, new NewUserModel { Name = receiverName });

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



			// Async Example
			/*await*/
			new RazorEmail().CreateAndSendEmailAsync(senderEmail, templateName, new { Name = receiverName });


			Console.ReadLine();

		}
	}
}
