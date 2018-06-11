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
		public string GenerateMessageBody(string basePath, string templateName, object model)
		{
			return GenerateMessageBody(basePath, templateName, model);
		}

		public string GenerateMessageBody<T>(string basePath, string templateName, T model)
		{
			string result = null;
			try
			{
				// TODO: Please grab the right template here! -- Are we the HTML or Plain Text template?
				string template = File.ReadAllText(Path.Combine(basePath, templateName, "TODO: FIX ME!"));

				var engine = EngineFactory.CreateEmbedded(model.GetType());

				ITemplateSource source = new LoadedTemplateSource(template);
				ModelTypeInfo modelInfo = new ModelTypeInfo(model.GetType());
				CompilationResult compiled = engine.Core.CompileSource(source, modelInfo);
				compiled.EnsureSuccessful();

				TemplatePage page = engine.Activate(compiled.CompiledType);
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
