using RazorLight;
using RazorLight.Compilation;
using RazorLight.Extensions;
using RazorLight.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class RazorMessageGenerator : IMessageGenerator
	{
		public string GenerateMessageBody(string template, object model)
		{
			string result = null;
			try
			{
				var engine = EngineFactory.CreateEmbedded(model.GetType());
				//result = engine.ParseString(template, model);

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
