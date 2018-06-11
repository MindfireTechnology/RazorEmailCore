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
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace RazorEmailCore
{
	public class RazorMessageGenerator : IMessageGenerator
	{

		public string GenerateMessageBody(string basePath, string templateName, object model)
		{
			throw new NotImplementedException();
		}

		public string GenerateMessageBody<T>(string basePath, string templateName, T model)
		{
			throw new NotImplementedException();
		}

		public string GenerateMessageBody(string template, object model)
		{
			string viewName = "EmailTemplate"; // @"D:\Projects\Entropy\samples\Mvc.RenderViewToString\Views\EmailTemplate.cshtml"; // @"D:\Projects\Entropy\samples\Mvc.RenderViewToString\Views";

			var scopeFactory = InitializeServices();
			using (var serviceScope = scopeFactory.CreateScope())
			{
				var razorViewEngine = serviceScope.ServiceProvider.GetService<IRazorViewEngine>();
				var templateDataProvider = serviceScope.ServiceProvider.GetService<ITempDataProvider>();

				var actionContext = GetActionContext(serviceScope.ServiceProvider);
				var view = FindView(razorViewEngine, actionContext, viewName);

				using (var output = new StringWriter())
				{
					// Create a generic object
					var vc = typeof(ViewDataDictionary<>);
					var vcm = vc.MakeGenericType(model.GetType());
					var viewDataDictionary = Activator.CreateInstance(vcm, new object[] { new EmptyModelMetadataProvider(), new ModelStateDictionary() });

					viewDataDictionary.GetType().GetProperties().Single(n => n.Name == "Model" && n.PropertyType != typeof(object)).SetValue(viewDataDictionary, model);

					var viewContext = new ViewContext(
						actionContext,
						view,
						(ViewDataDictionary)viewDataDictionary,
						//new ViewDataDictionary(
						//	metadataProvider: new EmptyModelMetadataProvider(),
						//	modelState: new ModelStateDictionary())
						//{
						//	Model = model
						//},
						new TempDataDictionary(
							actionContext.HttpContext,
							templateDataProvider),
						output,
						new HtmlHelperOptions());

					view.RenderAsync(viewContext).Wait();

					return output.ToString();
				}
			}
		}

		private IServiceScopeFactory InitializeServices()
		{
			// Initialize the necessary services
			var services = new ServiceCollection();
			ConfigureDefaultServices(services, customApplicationBasePath: null); // @"D:\Projects\Entropy\samples\Mvc.RenderViewToString\Views");


			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		private IView FindView(IRazorViewEngine viewEngine, ActionContext actionContext, string viewName)
		{
			var getViewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
			if (getViewResult.Success)
			{
				return getViewResult.View;
			}

			var findViewResult = viewEngine.FindView(actionContext, viewName, isMainPage: true);
			if (findViewResult.Success)
			{
				return findViewResult.View;
			}

			var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
			var errorMessage = string.Join(
				Environment.NewLine,
				new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations)); ;

			throw new InvalidOperationException(errorMessage);
		}

		private ActionContext GetActionContext(IServiceProvider serviceProvider)
		{
			var httpContext = new DefaultHttpContext();
			httpContext.RequestServices = serviceProvider;
			return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
		}

		private static void ConfigureDefaultServices(IServiceCollection services, string customApplicationBasePath)
		{
			string applicationName;
			IFileProvider fileProvider;
			if (!string.IsNullOrEmpty(customApplicationBasePath))
			{
				applicationName = Path.GetFileName(customApplicationBasePath);
				fileProvider = new PhysicalFileProvider(customApplicationBasePath);
			}
			else
			{
				applicationName = Assembly.GetEntryAssembly().GetName().Name;
				fileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."));
			}

			services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
			{
				ApplicationName = applicationName,
				WebRootFileProvider = fileProvider,
			});
			services.Configure<RazorViewEngineOptions>(options =>
			{
				options.FileProviders.Clear();
				options.FileProviders.Add(fileProvider);
			});
			var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
			services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
			services.AddSingleton<DiagnosticSource>(diagnosticSource);
			services.AddLogging();
			services.AddMvc();



			services.Configure<RazorViewEngineOptions>(options =>
			{
				//options.ViewLocationFormats.Add(@"D:\Projects\Entropy\samples\Mvc.RenderViewToString\Views\{0}.cshtml");
				options.ViewLocationFormats.Clear();
				options.ViewLocationFormats.Add(@"{0}.cshtml");
				options.ViewLocationFormats.Add(@"~/{0}.cshtml");
				options.ViewLocationFormats.Add(@"~/RazorEmail/{0}.cshtml");
				options.ViewLocationFormats.Add(@"~/RazorEmail/Views/{0}.cshtml");
			});
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