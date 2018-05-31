using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using RazorEmailCore;

namespace RazorEmailCore
{
	public class RazorMessageGenerator : IMessageGenerator
	{
		public string GenerateMessageBody(string template, object model)
		{
			string filename = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()), Path.ChangeExtension(Path.GetFileName(Path.GetTempFileName()), "cshtml"));
			Directory.CreateDirectory(Path.GetDirectoryName(filename));
			File.WriteAllText(filename, template);

			// customize the default engine a little bit

			var config = RazorConfiguration.Create(RazorLanguageVersion.Latest, string.Empty, new RazorExtension[] { });
			var fs = RazorProjectFileSystem.Create(Path.GetDirectoryName(filename));

			var engine = RazorProjectEngine.Create(config, fs,
			b =>
			{
				InheritsDirective.Register(b); // make sure the engine understand the @inherits directive in the input templates
				b.SetNamespace("RazorEmailCore"); // define a namespace for the Template class
				b.Build();
			});


			var item = fs.GetItem(filename);

			var document = engine.Process(item);

			var cs = document.GetCSharpDocument();

			var tree = CSharpSyntaxTree.ParseText(cs.GeneratedCode);

			// define the dll
			const string dllName = "RazorEmail.Generated";
			var compilation = CSharpCompilation.Create(dllName, new[] { tree },
				new[]
				{
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // include corlib
					MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location), // this file (that contains the MyTemplate base class)
					MetadataReference.CreateFromFile(typeof(RazorEmailTemplate).Assembly.Location), // Base class

					MetadataReference.CreateFromFile(typeof(RazorCompiledItemAttribute).Assembly.Location), // Needed because of the code that is generated

					//// error CS0656: Missing compiler required member 'Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create'
					//MetadataReference.CreateFromFile(typeof(CSharpArgumentInfo).Assembly.Location),

					//// C:\Users\nzaugg\AppData\Local\Temp\tmp3607.cshtml(5,12): error CS0656: Missing compiler required member 'Microsoft.CSharp.RuntimeBinder.Binder.GetMember'
					//MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location),

					MetadataReference.CreateFromFile(@"C:\Users\nzaugg\.nuget\packages\microsoft.csharp\4.5.0\lib\netstandard2.0\Microsoft.CSharp.dll"),

					// for some reason on .NET core, I need to add this... this is not needed with .NET framework
					MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),

					// as found out by @Isantipov, for some other reason on .NET Core for Mac and Linux, we need to add this... this is not needed with .NET framework
					MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll"))
				},
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)); // we want a dll


			// compile the dll
			string path = Path.Combine(Path.GetDirectoryName(filename), dllName + ".dll");
			var result = compilation.Emit(path);
			//if (!result.Success)
			//{
			//	Console.WriteLine(string.Join(Environment.NewLine, result.Diagnostics));
			//	return;
			//}

			// load the built dll
			Console.WriteLine(path);
			var asm = Assembly.LoadFile(path);

			// the generated type is defined in our custom namespace, as we asked. "Template" is the type name that razor uses by default.
			var target = (RazorEmailTemplate)Activator.CreateInstance(asm.GetType("RazorEmailCore.Template"));

			// run the code.
			// should display "Hello Killroy, welcome to Razor World!"
			//template.ExecuteAsync().Wait();
			target.ExecuteAsync().Wait();


			throw new NotImplementedException();

			//string result = null;
			//try
			//{
			//	var engine = EngineFactory.CreateEmbedded(model.GetType());
			//	//result = engine.ParseString(template, model);

			//	ITemplateSource source = new LoadedTemplateSource(template);
			//	ModelTypeInfo modelInfo = new ModelTypeInfo(model.GetType());
			//	CompilationResult compiled = engine.Core.CompileSource(source, modelInfo);
			//	compiled.EnsureSuccessful();

			//	TemplatePage page = engine.Activate(compiled.CompiledType);
			//	page.PageContext = new PageContext { ModelTypeInfo = modelInfo };

			//	result = engine.RunTemplate(page, model);
			//}
			//catch (TemplateCompilationException tex)
			//{
			//	throw new MessageGenerationException($"Error generating message from template! {tex.CompilationErrors.FirstOrDefault()}", tex);
			//}
			//catch (Exception ex)
			//{
			//	throw new MessageGenerationException("Unexpected error generating message from template!", ex);
			//}

			//return result;
		}

		// the sample base template class. It's not mandatory but I think it's much easier.

	}
	public abstract class RazorEmailTemplate
	{
		// this will map to @Model (property name)
		public dynamic Model { get; set; }

		public void WriteLiteral(string literal)
		{
			// replace that by a text writer for example
			Console.Write(literal);
		}

		public void Write(object obj)
		{
			// replace that by a text writer for example
			Console.Write(obj);
		}

		public async virtual Task ExecuteAsync()
		{
			await Task.Yield(); // whatever, we just need something that compiles...
		}
	}


}