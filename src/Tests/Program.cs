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
			emailBuilder.CreateEmail("NewUserTemplate", new { });


			//string content = "Hello @Model.Name. Welcome to @Model.Title repository";

			//var model = new
			//{
			//	Name = "John Doe",
			//	Title = "RazorLight"
			//};

			//var engine = EngineFactory.CreateEmbedded(model.GetType());//, new EngineConfiguration(new MyActivator(), new RoslynCompilerService(new UseEntryAssemblyMetadataResolver()), new DefaultRazorTemplateCompiler()));
			//var result = engine.ParseString(content, model);
			////string result = engine.ParseString(content, model); //Output: Hello John Doe, Welcome to RazorLight repository


			////var rte = new RazorTemplateEngine(new RazorEngineHost(new CSharpRazorCodeLanguage()));
			////string program = "<h1>This is a Razor file, really!</h1>";
			////var ms = new MemoryStream(Encoding.UTF8.GetBytes(program));
			////var tr = new StreamReader(ms);
			////var str = new Microsoft.AspNetCore.Razor.Text.SeekableTextReader(tr);
			////var result = rte.GenerateCode(tr);
			//////var result = rte.ParseTemplate(str);

			////AssemblyLoadContext.Default.LoadFromStream()

			//Console.WriteLine(result);
			//Console.ReadLine();
		}
	}
}
