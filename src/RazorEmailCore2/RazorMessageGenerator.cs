using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RazorEmailCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using System.Diagnostics;

namespace RazorEmailCore
{
	public class RazorMessageGenerator : IMessageGenerator
	{
		public string GenerateMessageBody(string template, object model)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddMvc();
			services.AddTransient<ILoggerFactory, LoggerFactory>();
			services.AddSingleton<IHostingEnvironment>(n => GetHostingEnvironment(n));
			services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
			services.AddSingleton<DiagnosticSource, RazorEmailDiagnosticListener>();
			services.AddScoped<IViewRender, ViewRender>();
			var provider = services.BuildServiceProvider();

			var render = provider.GetService<IViewRender>();
			return render.RenderAsync("NewUserTemplate", model).Result;

			// 'Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions.FileProviders' must not be empty. At least one 'Microsoft.Extensions.FileProviders.IFileProvider' is required to locate a view for rendering.
		}

		private IHostingEnvironment GetHostingEnvironment(IServiceProvider services)
		{
			return new RazorEmailHostingEnvironment
			{
				WebRootFileProvider = new PhysicalFileProvider(@"D:\Projects\RazorEmailCore\src\Example2\RazorEmail\NewUserTemplate"),
				ContentRootFileProvider = new PhysicalFileProvider(@"D:\Projects\RazorEmailCore\src\Example2\RazorEmail\NewUserTemplate")
			};
		}
	}

	public class RazorEmailHostingEnvironment : Microsoft.AspNetCore.Hosting.IHostingEnvironment
	{
		public string EnvironmentName { get; set; }
		public string ApplicationName { get; set; }
		public string WebRootPath { get; set; }
		public IFileProvider WebRootFileProvider { get; set; }
		public string ContentRootPath { get; set; }
		public IFileProvider ContentRootFileProvider { get; set; }
	}

	public class RazorEmailDiagnosticListener : DiagnosticSource
	{
		public override bool IsEnabled(string name)
		{
			return false;
		}

		public override void Write(string name, object value)
		{
			throw new NotImplementedException();
		}
	}

	public class ViewRender : IViewRender
	{
		private readonly IRazorViewEngine _viewEngine;
		private readonly ITempDataProvider _tempDataProvider;
		private readonly IServiceProvider _serviceProvider;

		public ViewRender(
			IRazorViewEngine viewEngine,
			ITempDataProvider tempDataProvider,
			IServiceProvider serviceProvider)
		{
			_viewEngine = viewEngine;
			_tempDataProvider = tempDataProvider;
			_serviceProvider = serviceProvider;
		}

		public async Task<string> RenderAsync(string name)
		{
			return await RenderAsync<object>(name, null);
		}

		public async Task<string> RenderAsync<TModel>(string name, TModel model)
		{


			var actionContext = GetActionContext();

			//var viewEngineResult = _viewEngine.GetView(@"D:\Projects\RazorEmailCore\src\Example2\RazorEmail\NewUserTemplate", @"Views\NewUserTemplate.cshtml", false);

			var viewEngineResult = _viewEngine.FindView(actionContext, name, false);

			if (!viewEngineResult.Success)
			{
				throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
			}

			var view = viewEngineResult.View;

			using (var output = new StringWriter())
			{
				var viewContext = new ViewContext(
					actionContext,
					view,
					new ViewDataDictionary<TModel>(
						metadataProvider: new EmptyModelMetadataProvider(),
						modelState: new ModelStateDictionary())
					{
						Model = model
					},
					new TempDataDictionary(
						actionContext.HttpContext,
						_tempDataProvider),
					output,
					new HtmlHelperOptions());

				await view.RenderAsync(viewContext);

				return output.ToString();
			}
		}

		private ActionContext GetActionContext()
		{
			var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
			return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
		}
	}

	public interface IViewRender
	{
		Task<string> RenderAsync(string name);

		Task<string> RenderAsync<TModel>(string name, TModel model);
	}
}