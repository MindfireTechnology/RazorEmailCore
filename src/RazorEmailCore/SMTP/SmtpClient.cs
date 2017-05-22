using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RazorEmailCore.SMTP
{
	public class SmtpClient
	{
		protected Socket socket; 

		public string Server { get; set; }
		public string Username { get; set; }
		public string Password { protected get; set; }

		public string HostName { get; set; }

		public SmtpClient() { }

		public SmtpClient(string server, string username = null, string password = null)
		{
			Server = server;
			Username = username;
			Password = password;
		}

		public virtual void SendMessage(Email message)
		{
			SendMessageAsync(message).Wait();
		}

		public virtual async Task SendMessageAsync(Email message)
		{
			var uri = new Uri(Server);
			using (socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
			{
				await socket.ConnectAsync(uri.Host, uri.Port == -1 ? 25 : uri.Port);

				// 220 smtp.whatever.com ESMTP
				CheckMessageStatus(await ReceiveMessageAsync());

				// HELO mydomain.com
				await SendAsync($"HELO {HostName ?? Environment.MachineName}\r\n");
				CheckMessageStatus(await ReceiveMessageAsync());

				// Auth Login
				if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password))
				{
					await SendAsync("AUTH LOGIN\r\n");
					CheckMessageStatus(await ReceiveMessageAsync(), "334");

					await SendAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)) + "\r\n");
					CheckMessageStatus(await ReceiveMessageAsync(), "334");

					await SendAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)) + "\r\n");
					CheckMessageStatus(await ReceiveMessageAsync());
				}

				// MAIL FROM
				await SendAsync($"MAIL FROM: {message.Sender.Email}\r\n");
				CheckMessageStatus(await ReceiveMessageAsync());

				// RCPT TO
				foreach (EmailAddress address in message.To.Concat(message.Cc).Concat(message.Bcc).Distinct())
				{
					await SendAsync($"RCPT TO: {address.Email}\r\n");
					CheckMessageStatus(await ReceiveMessageAsync());
				}

				// DATA
				await SendAsync("DATA\r\n");
				CheckMessageStatus(await ReceiveMessageAsync(), "354");

				// Send Headers
				await SendMessageHeadersAsync(message);

				// Send Message Body
				await SendMessageBodyAsync(message);

				// Send CRLF.CRLF
				await SendAsync("\r\n.\r\n");
				CheckMessageStatus(await ReceiveMessageAsync());

				// QUIT
				await SendAsync("QUIT\r\n");
			}
		}

		protected async Task SendMessageHeadersAsync(Email message)
		{
			await SendAsync($"FROM: {message.Sender}\r\n");
			string recipients = string.Join("; ", message.To);
			await SendAsync($"TO: {recipients}\r\n");
			recipients = string.Join("; ", message.Cc);
			await SendAsync($"Cc: {recipients}\r\n");
			await SendAsync($"Date: {DateTime.Now.ToUniversalTime()}\r\n");
			await SendAsync($"Subject: {message.Subject}\r\n");
			await SendAsync($"X-Mailer: RazorEmailCore\r\n");
			await SendAsync($"MIME-Version: 1.0\r\n");
			await SendAsync($"Content-Type: multipart/alternative; boundary=\"XxxxBoundaryText{message.GetHashCode()}\"\r\n");
			await SendAsync("\r\n"); // Done with headers
		}

		protected async Task SendMessageBodyAsync(Email message)
		{
			//Send("This is a multipart message in MIME format.\r\n");

			// Send PlainText first (if we have it)
			if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
			{
				// Send begin content boundary
				await SendAsync($"--XxxxBoundaryText{message.GetHashCode()}\r\n");
				await SendAsync($"Content-Type: text/plain; charset=\"utf-8\"\r\n");
				await SendAsync($"Content-Transfer-Encoding: 8bit\r\n");
				await SendAsync("\r\n"); // End headers

				await SendAsync(message.PlainTextBody);
			}

			if (!string.IsNullOrWhiteSpace(message.HtmlBody))
			{
				// Send begin content boundary
				await SendAsync($"--XxxxBoundaryText{message.GetHashCode()}\r\n");
				await SendAsync($"Content-Type: text/html; charset=\"utf-8\"\r\n");
				await SendAsync($"Content-Transfer-Encoding: 8bit\r\n");
				await SendAsync("\r\n"); // End headers

				await SendAsync(message.HtmlBody);
			}

			// Send End Content Boundary
			await SendAsync("\r\n");
			await SendAsync($"--XxxxBoundaryText{message.GetHashCode()}--\r\n");
		}

		protected void CheckMessageStatus(string response, string expectedCode = "2")
		{
			if (!response.StartsWith(expectedCode))
				throw new SmtpException($"Received SMTP Error: {response}");
		}

		protected Task SendAsync(string value)
		{
			Debug.WriteLine($"SENDING  --> {value.Replace("\r", "\\r").Replace("\n", "\\n")}");
			return socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(value)), SocketFlags.None);
		}

		protected async Task<string> ReceiveMessageAsync()
		{
			byte[] buffer = new byte[2048];
			var array = new ArraySegment<byte>(buffer);
			int read = await socket.ReceiveAsync(array, SocketFlags.None);

			string result = Encoding.UTF8.GetString(buffer, 0, read);
			Debug.WriteLine($"RECEIVED <-- {result.Replace("\r", "\\r").Replace("\n", "\\n")}");
			return result;
		}
	}
}
