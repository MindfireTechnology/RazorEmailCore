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

		public virtual bool SendMessage(Email message)
		{
			var uri = new Uri(Server);
			using (socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
			{
				socket.Connect(uri.Host, uri.Port == -1 ? 25 : uri.Port);

				// 220 smtp.whatever.com ESMTP
				string response = ReceiveMessage();
				CheckMessageStatus(response);

				// HELO mydomain.com
				Send($"HELO {HostName ?? Environment.MachineName}\r\n");
				CheckMessageStatus(ReceiveMessage());

				// Auth Login
				if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password))
				{
					Send("AUTH LOGIN\r\n");
					CheckMessageStatus(ReceiveMessage(), "334");

					Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)) + "\r\n");
					CheckMessageStatus(ReceiveMessage(), "334");

					Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)) + "\r\n");
					CheckMessageStatus(ReceiveMessage());
				}

				// MAIL FROM
				Send($"MAIL FROM: {message.Sender}\r\n");
				CheckMessageStatus(ReceiveMessage());

				// RCPT TO
				foreach (EmailAddress address in message.To.Concat(message.Cc).Concat(message.Bcc).Distinct())
				{
					Send($"RCPT TO: {address}\r\n");
					CheckMessageStatus(ReceiveMessage());
				}

				// DATA
				Send("DATA\r\n");
				CheckMessageStatus(ReceiveMessage(), "354");

				// Send Headers
				SendMessageHeaders(message);

				// Send Message Body
				SendMessageBody(message);

				// Send CRLF.CRLF
				Send("\r\n.\r\n");
				CheckMessageStatus(ReceiveMessage());

				// QUIT
				Send("QUIT\r\n");

				return true;
			}
		}
		private void SendMessageHeaders(Email message)
		{
			Send($"FROM: {message.Sender}\r\n");
			string recipients = string.Join("; ", message.To);
			Send($"TO: {recipients}\r\n");
			recipients = string.Join("; ", message.Cc);
			Send($"Cc: {recipients}\r\n");
			Send($"Date: {DateTime.Now.ToUniversalTime()}\r\n");
			Send($"Subject: {message.Subject}\r\n");
			Send($"X-Mailer: RazorEmailCore\r\n");
			Send($"MIME-Version: 1.0\r\n");
			Send($"Content-Type: multipart/alternative; boundary=\"XxxxBoundaryText{message.GetHashCode()}\"\r\n");
			Send("\r\n"); // Done with headers
		}

		private void SendMessageBody(Email message)
		{
			//Send("This is a multipart message in MIME format.\r\n");

			// Send PlainText first (if we have it)
			if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
			{
				// Send begin content boundary
				Send($"--XxxxBoundaryText{message.GetHashCode()}\r\n");
				Send($"Content-Type: text/plain; charset=\"utf-8\"\r\n");
				Send($"Content-Transfer-Encoding: 8bit\r\n");
				Send("\r\n"); // End headers

				Send(message.PlainTextBody);
			}

			if (!string.IsNullOrWhiteSpace(message.HtmlBody))
			{
				// Send begin content boundary
				Send($"--XxxxBoundaryText{message.GetHashCode()}\r\n");
				Send($"Content-Type: text/html; charset=\"utf-8\"\r\n");
				Send($"Content-Transfer-Encoding: 8bit\r\n");
				Send("\r\n"); // End headers

				Send(message.HtmlBody);
			}

			// Send End Content Boundary
			Send("\r\n");
			Send($"--XxxxBoundaryText{message.GetHashCode()}--\r\n");
		}


		private void Send(string value)
		{
			Debug.WriteLine($"SENDING --> {value.Replace("\r", "\\r").Replace("\n", "\\n")}");
			socket.Send(Encoding.UTF8.GetBytes(value));
		}

		private void CheckMessageStatus(string response, string expectedCode = "2")
		{
			if (!response.StartsWith(expectedCode))
				throw new SmtpException($"Received SMTP Error: {response}");
		}

		public string ReceiveMessage()
		{
			byte[] buffer = new byte[2048];
			int read = socket.Receive(buffer);

			string result = Encoding.UTF8.GetString(buffer, 0, read);
			Debug.WriteLine($"RECEIVED<-- {result.Replace("\r", "\\r").Replace("\n", "\\n")}");
			return result;
		}
	}
}
