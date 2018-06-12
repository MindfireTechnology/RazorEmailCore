using RazorLight;
using RazorLight.Compilation;
using RazorLight.Extensions;
using RazorLight.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class RazorMessageGenerator : IMessageGenerator
	{
		protected static readonly string HtmlExtension = ".razor";
		protected static readonly string TextExtension = ".text";

		public string GenerateHtmlMessageBody(string basePath, string templateName, object model) => GenerateMessageBody(basePath, templateName, HtmlExtension, model);
		public string GenerateHtmlMessageBody<T>(string basePath, string templateName, T model) => GenerateMessageBody(basePath, templateName, HtmlExtension, model);

		public string GenerateTextMessageBody(string basePath, string templateName, object model) => GenerateMessageBody(basePath, templateName, TextExtension, model);

		public string GenerateTextMessageBody<T>(string basePath, string templateName, T model) => GenerateMessageBody(basePath, templateName, TextExtension, model);

		protected static string GenerateMessageBody(string basePath, string templateName, string extension, object model)
		{
			string result = null;
			try
			{
				var template = File.ReadAllText(Path.Combine(basePath, templateName, Path.ChangeExtension(templateName, extension)));

				var engine = EngineFactory.CreateEmbedded(model.GetType());

				ITemplateSource source = new LoadedTemplateSource(template);
				var modelInfo = new ModelTypeInfo(model.GetType());
				var compiled = engine.Core.CompileSource(source, modelInfo);
				compiled.EnsureSuccessful();

				var page = engine.Activate(compiled.CompiledType);
				page.PageContext = new PageContext { ModelTypeInfo = modelInfo };

				result = engine.RunTemplate(page, model);
			}
			catch (TemplateCompilationException tex)
			{
				throw new MessageGenerationException($"Error generating message from template! {tex.CompilationErrors.FirstOrDefault()}", tex);
			}
			catch (Exception ex)
			{
				throw new MessageGenerationException("Unexpected error generating message from template!", ex);
			}

			return result;
		}
	}
}
