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
using System.Text.Encodings.Web;

namespace RazorEmailCore
{
	public class RazorMessageGenerator : IMessageGenerator
	{
		protected static readonly string TextExtension = ".text";
		protected static readonly string HtmlExtension = ".cshtml";

		public string GenerateHtmlMessageBody(string basePath, string templateName, object model) => GetHtmlView(basePath, templateName, CreateViewDataDictionary(model));
		public string GenerateHtmlMessageBody<T>(string basePath, string templateName, T model) => GetHtmlView(basePath, templateName, CreateViewDataDictionary<T>(model));
		public string GenerateTextMessageBody(string basePath, string templateName, object model) => GetTextView(templateName, CreateViewDataDictionary(model));
		public string GenerateTextMessageBody<T>(string basePath, string templateName, T model) => GetTextView(templateName, CreateViewDataDictionary<T>(model));

		public static string GenerateMessageBody(IView view, ViewDataDictionary viewDataDictionary, ActionContext actionContext, ITempDataProvider templateDataProvider)
		{
			using (var output = new StringWriter())
			{
				var viewContext = new ViewContext(
					actionContext,
					view,
					viewDataDictionary,
					new TempDataDictionary(
						actionContext.HttpContext,
						templateDataProvider),
					output,
					new HtmlHelperOptions());

				view.RenderAsync(viewContext).Wait();

				return output.ToString();
			}
		}

		protected static string GetTextView(string templateName, ViewDataDictionary viewDataDictionary)
		{
			var scopeFactory = InitializeServices();

			using (var serviceScope = scopeFactory.CreateScope())
			{
				var razorViewEngine = serviceScope.ServiceProvider.GetService<IRazorViewEngine>();
				var templateDataProvider = serviceScope.ServiceProvider.GetService<ITempDataProvider>();
				var pageActivator = serviceScope.ServiceProvider.GetService<IRazorPageActivator>();
				var pageFactory = serviceScope.ServiceProvider.GetService<IRazorPageFactoryProvider>();
				var htmlEncoder = serviceScope.ServiceProvider.GetService<HtmlEncoder>();
				var diagnosticSource = serviceScope.ServiceProvider.GetService<DiagnosticSource>();

				var emptyPath = $@"RazorEmail\{templateName}\{templateName}.text";
				var viewsPath = $@"RazorEmail\{templateName}\Views\{templateName}.text";

				var razorPageFactory = pageFactory.CreateFactory(emptyPath).RazorPageFactory;

				if (razorPageFactory == null)
				{
					razorPageFactory = pageFactory.CreateFactory(viewsPath).RazorPageFactory;

					if (razorPageFactory == null)
						throw new InvalidOperationException($"Unable to find view '{templateName}'. The following locations were searched: {emptyPath}\n{viewsPath}");
				}

				IRazorPage page = razorPageFactory.Invoke();

				IView view = new RazorView(razorViewEngine, pageActivator, new List<IRazorPage>(), page, htmlEncoder, diagnosticSource);


				var actionContext = GetActionContext(serviceScope.ServiceProvider);

				return GenerateMessageBody(view, viewDataDictionary, actionContext, templateDataProvider);
			}
		}

		protected static string GetHtmlView(string basePath, string templateName, ViewDataDictionary viewDataDictionary)
		{
			var scopeFactory = InitializeServices();

			using (var serviceScope = scopeFactory.CreateScope())
			{
				var razorViewEngine = serviceScope.ServiceProvider.GetService<IRazorViewEngine>();
				var templateDataProvider = serviceScope.ServiceProvider.GetService<ITempDataProvider>();

				var actionContext = GetActionContext(serviceScope.ServiceProvider);


				IView view = null;

				var messages = new List<string>();
				var extension = ".cshtml";

				var viewName = Path.Combine(basePath, templateName, Path.ChangeExtension(templateName, extension));
				var viewNameInViews = Path.Combine(basePath, templateName, "Views", Path.ChangeExtension(templateName, extension));

				// Try to get the view with the exact name
				// It could be in this folder or in the "Views" folder, so we need to try twice
				var result = LoadView(viewName, out view);

				if (!result.success)
				{
					messages.Add(result.message);
					result = LoadView(viewNameInViews, out view);

					if (!result.success)
					{
						messages.Add(result.message);

						// Didn't find it with the full path, so let Razor do its ting
						result = LoadView(templateName, out view);

						if (!result.success)
						{
							throw new InvalidOperationException(string.Join(Environment.NewLine, messages));
						}
					}
				}

				return GenerateMessageBody(view, viewDataDictionary, actionContext, templateDataProvider);

				(string message, bool success) LoadView(string name, out IView viewInstance)
				{
					viewInstance = null;

					try
					{
						view = FindView(razorViewEngine, actionContext, name);
						return (string.Empty, true);
					}
					catch (InvalidOperationException ioe)
					{
						return (ioe.Message, false);
					}
				}
			}

		}

		protected static ViewDataDictionary CreateViewDataDictionary<T>(T model)
		{
			var viewDataDictionary = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
			{
				Model = model
			};

			return viewDataDictionary;
		}

		protected static ViewDataDictionary CreateViewDataDictionary(object model)
		{
			var vc = typeof(ViewDataDictionary<>);
			var vcm = vc.MakeGenericType(model.GetType());
			var viewDataDictionary = Activator.CreateInstance(vcm, new object[] { new EmptyModelMetadataProvider(), new ModelStateDictionary() });

			viewDataDictionary.GetType().GetProperties().Single(n => n.Name == "Model" && n.PropertyType != typeof(object)).SetValue(viewDataDictionary, model);

			return (ViewDataDictionary)viewDataDictionary;
		}

		private static IServiceScopeFactory InitializeServices()
		{
			// Initialize the necessary services
			var services = new ServiceCollection();
			ConfigureDefaultServices(services, customApplicationBasePath: null);

			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		/// <summary>
		/// Find and instance the specified view with the Razor engine
		/// </summary>
		/// <param name="viewEngine">The Razor engine to use</param>
		/// <param name="actionContext"></param>
		/// <param name="viewName">The path to the desired view</param>
		/// <returns></returns>
		private static IView FindView(IRazorViewEngine viewEngine, ActionContext actionContext, string viewName)
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
				new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));

			throw new InvalidOperationException(errorMessage);
		}

		private static ActionContext GetActionContext(IServiceProvider serviceProvider)
		{
			var httpContext = new DefaultHttpContext
			{
				RequestServices = serviceProvider
			};
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
				options.ViewLocationFormats.Add(@"~/RazorEmail/{0}/{0}.cshtml");
				options.ViewLocationFormats.Add(@"~/RazorEmail/{0}/Views/{0}.cshtml");
			});
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