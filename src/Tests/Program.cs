using Microsoft.AspNetCore.Razor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Loader;
using RazorLight.Extensions;
using RazorLight;
using RazorLight.Compilation;
using RazorEmailCore;

namespace Tests
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var emailBuilder = new RazorEmail();
			bool sent = emailBuilder.CreateAndSendEmail("nzaugg@gmail.com", "NewUserTemplate", new { Name = "Nate Zaugg" });

			if (sent)
				Console.WriteLine("Message sent successfully!");
			else
				Console.WriteLine("Message failed to send!");

			Console.ReadLine();
		}
	}
}
