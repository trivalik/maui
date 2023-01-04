﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Devices;
using System;
using Microsoft.Maui.Controls.Platform;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Compatibility;
#endif

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.Window)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class WindowTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);

#if ANDROID || WINDOWS
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
#else
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					handlers.AddHandler(typeof(FlyoutPage), typeof(PhoneFlyoutPageRenderer));
#endif

				});
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ChangingToNewMauiContextDoesntCrash(bool useAppMainPage)
		{
			SetupBuilder();

			var rootPage = new NavigationPage(new TabbedPage() { Children = { new ContentPage() } });
			IWindow window;

			if (useAppMainPage)
			{
				var app = ApplicationServices.GetService<IApplication>() as ApplicationStub;
				app.MainPage = rootPage;
				window = (app as IApplication).CreateWindow(null);

			}
			else
				window = new Window(rootPage);

			var mauiContextStub1 = new ContextStub(ApplicationServices);
#if ANDROID
			var activity = mauiContextStub1.GetActivity();
			mauiContextStub1.Context = new Android.Views.ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);
#endif
			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(rootPage.CurrentPage);
				await OnNavigatedToAsync(rootPage.CurrentPage);
				await Task.Delay(100);

			}, mauiContextStub1);

			var mauiContextStub2 = new ContextStub(ApplicationServices);

#if ANDROID
			mauiContextStub2.Context = new Android.Views.ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);
#endif
			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(rootPage.CurrentPage);
				await OnNavigatedToAsync(rootPage.CurrentPage);
				await Task.Delay(100);

			}, mauiContextStub2);
		}

		[Theory]
		[ClassData(typeof(WindowPageSwapTestCases))]
		public async Task MainPageSwapTests(WindowPageSwapTestCase swapOrder)
		{
			SetupBuilder();

			var firstRootPage = swapOrder.GetNextPageType();
			var window = new Window(firstRootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(swapOrder.Page);
				while (!swapOrder.IsFinished())
				{
					var previousRootPage = window.Page?.GetType();
					var nextRootPage = swapOrder.GetNextPageType();
					window.Page = nextRootPage;

					try
					{
						await OnLoadedAsync(swapOrder.Page);

#if !IOS && !MACCATALYST

						var toolbar = GetToolbar(handler);

						// Shell currently doesn't create the handler on the xplat toolbar with Android
						// Because Android has lots of toolbars spread out between the viewpagers that
						var platformToolBar = GetPlatformToolbar(handler);
						Assert.Equal(platformToolBar != null, toolbar != null);

						if (platformToolBar != null)
						{
							if (DeviceInfo.Current.Platform == DevicePlatform.WinUI ||
								window.Page is not Shell)
							{
								Assert.Equal(toolbar?.Handler?.PlatformView, platformToolBar);
							}

							Assert.True(IsNavigationBarVisible(handler));
						}
#endif

					}
					catch (Exception exc)
					{
						throw new Exception($"Failed to swap to {nextRootPage} from {previousRootPage}", exc);
					}
				}
			});
		}

#if !IOS && !MACCATALYST
		// Automated Shell tests are currently broken via xharness
		[Fact(DisplayName = "Toolbar Items Update when swapping out Main Page on Handler")]
		public async Task ToolbarItemsUpdateWhenSwappingOutMainPageOnHandler()
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem() { Text = "Toolbar Item 1" };
			var firstPage = new ContentPage();

			var window = new Window(firstPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				var contentPage = new ContentPage()
				{
					ToolbarItems =
					{
						toolbarItem
					}
				};

				var shell = new Shell() { CurrentItem = contentPage };
				window.Page = shell;


				await OnLoadedAsync(shell);
				await OnLoadedAsync(shell.CurrentPage);

				ToolbarItemsMatch(handler, toolbarItem);
			});
		}
#endif

	}
}
