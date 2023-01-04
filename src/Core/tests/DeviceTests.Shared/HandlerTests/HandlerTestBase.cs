using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public class HandlerTestBase : TestBase, IDisposable
	{
		MauiApp _mauiApp;
		IServiceProvider _servicesProvider;
		IMauiContext _mauiContext;
		bool _isCreated;

		public void EnsureHandlerCreated(Action<MauiAppBuilder> additionalCreationActions = null)
		{
			if (_isCreated)
			{
				return;
			}

			_isCreated = true;


			var appBuilder = MauiApp.CreateBuilder();

			appBuilder.Services.AddSingleton<IDispatcherProvider>(svc => TestDispatcher.Provider);
			appBuilder.Services.AddScoped<IDispatcher>(svc => TestDispatcher.Current);
			appBuilder.Services.AddSingleton<IApplication>((_) => new CoreApplicationStub());

			appBuilder = ConfigureBuilder(appBuilder);
			additionalCreationActions?.Invoke(appBuilder);

			_mauiApp = appBuilder.Build();
			_servicesProvider = _mauiApp.Services;

			_mauiContext = new ContextStub(_servicesProvider);
		}

		protected virtual MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
		{
			return mauiAppBuilder;
		}

		protected IMauiContext MauiContext
		{
			get
			{
				EnsureHandlerCreated();
				return _mauiContext;
			}
		}

		protected IServiceProvider ApplicationServices
		{
			get
			{
				EnsureHandlerCreated();
				return _servicesProvider;
			}
		}

		protected Task SetValueAsync<TValue, THandler>(IView view, TValue value, Action<THandler, TValue> func)
			where THandler : IElementHandler, new()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				func(handler, value);
			});
		}

		protected THandler CreateHandler<THandler>(IElement view, IMauiContext mauiContext = null)
			where THandler : IElementHandler, new()
			=> CreateHandler<THandler, THandler>(view, mauiContext);


		protected void InitializeViewHandler(IElement element, IElementHandler handler, IMauiContext mauiContext = null)
		{
			mauiContext ??= MauiContext;
			handler.SetMauiContext(mauiContext);
			handler.SetVirtualView(element);
			element.Handler = handler;

			if (element is IView view && handler is IViewHandler viewHandler)
			{
#if ANDROID
				// If the Android views don't have LayoutParams set, updating some properties (e.g., Text)
				// can run into issues when deciding whether a re-layout is required. Normally this isn't
				// an issue because the LayoutParams get set when the view is added to a ViewGroup, but 
				// since we're not doing that here, we need to ensure they have LayoutParams so that tests
				// which update properties don't crash. 

				var aView = viewHandler.PlatformView as Android.Views.View;
				if (aView.LayoutParameters == null)
				{
					aView.LayoutParameters =
						new Android.Views.ViewGroup.LayoutParams(
							Android.Views.ViewGroup.LayoutParams.WrapContent,
							Android.Views.ViewGroup.LayoutParams.WrapContent);
				}

				var size = view.Measure(view.Width, view.Height);
				var w = size.Width;
				var h = size.Height;
#elif IOS || MACCATALYST
				var size = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
				var w = size.Width;
				var h = size.Height;

				// No measure method should be returning infinite values
				Assert.False(double.IsPositiveInfinity(w));
				Assert.False(double.IsPositiveInfinity(h));

#else
				// Windows cannot measure without the view being loaded
				// iOS needs more love when I get an IDE again
				var w = view.Width.Equals(double.NaN) ? -1 : view.Width;
				var h = view.Height.Equals(double.NaN) ? -1 : view.Height;
#endif

				view.Arrange(new Rect(0, 0, w, h));
				viewHandler.PlatformArrange(view.Frame);
			}
		}

		protected TCustomHandler CreateHandler<THandler, TCustomHandler>(IElement element, IMauiContext mauiContext)
			where THandler : IElementHandler, new()
			where TCustomHandler : THandler, new()
		{
			if (element.Handler is TCustomHandler t)
				return t;

			mauiContext ??= MauiContext;
			var handler = Activator.CreateInstance<TCustomHandler>();
			InitializeViewHandler(element, handler, mauiContext);
			return handler;
		}

#if PLATFORM
		protected IPlatformViewHandler CreateHandler(IElement view, Type handlerType)
		{
			if (view.Handler is IPlatformViewHandler t)
				return t;

			var handler = (IPlatformViewHandler)Activator.CreateInstance(handlerType);
			InitializeViewHandler(view, handler, MauiContext);
			return handler;

		}

		protected Task ValidateHasColor(IView view, Color color, Type handlerType, Action action = null)
		{
#if !TIZEN
			return InvokeOnMainThreadAsync(async () =>
			{
				var plaformView = CreateHandler(view, handlerType).ToPlatform();
				action?.Invoke();
				await plaformView.AssertContainsColor(color);
			});
#else
			throw new NotImplementedException();
#endif
		}

#endif

		public void Dispose()
		{
			((IDisposable)_mauiApp)?.Dispose();
			_mauiApp = null;
			_servicesProvider = null;
			_mauiContext = null;
		}
	}
}