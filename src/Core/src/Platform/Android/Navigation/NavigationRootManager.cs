﻿using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public class NavigationRootManager
	{
		IMauiContext _mauiContext;
		AView? _rootView;
		Fragment? _viewFragment;
		IToolbarElement? _toolbarElement;

		// TODO MAUI: temporary event to alert when rootview is ready
		// handlers and various bits use this to start interacting with rootview
		internal event EventHandler? RootViewChanged;

		LayoutInflater LayoutInflater => _mauiContext?.GetLayoutInflater()
			?? throw new InvalidOperationException($"LayoutInflater missing");

		internal FragmentManager FragmentManager => _mauiContext?.GetFragmentManager()
			?? throw new InvalidOperationException($"FragmentManager missing");

		public AView? RootView => _rootView;

		internal DrawerLayout? DrawerLayout { get; private set; }

		internal IToolbarElement? ToolbarElement => _toolbarElement;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		internal void SetToolbarElement(IToolbarElement toolbarElement)
		{
			_toolbarElement = toolbarElement;
		}

		internal void Connect(IView view, IMauiContext? mauiContext = null)
		{
			ClearPlatformParts();

			mauiContext = mauiContext ?? _mauiContext;
			CoordinatorLayout? navigationLayout = null;

			if (view is IFlyoutView)
			{
				var containerView = view.ToContainerView(mauiContext);
				navigationLayout = containerView.FindViewById<CoordinatorLayout>(Resource.Id.navigation_layout);

				if (containerView is DrawerLayout dl)
				{
					_rootView = dl;
					DrawerLayout = dl;
				}
				else if (containerView is ContainerView cv && cv.MainView is DrawerLayout dlc)
				{
					_rootView = cv;
					DrawerLayout = dlc;
				}
			}
			else
			{
				navigationLayout ??=
				   LayoutInflater
					   .Inflate(Resource.Layout.navigationlayout, null)
					   .JavaCast<CoordinatorLayout>();

				_rootView = navigationLayout;
			}

			// if the incoming view is a Drawer Layout then the Drawer Layout
			// will be the root view and internally handle all if its view management
			// this is mainly used for FlyoutView
			//
			// if it's not a drawer layout then we just use our default CoordinatorLayout inside navigationlayout
			// and place the content there
			if (DrawerLayout == null)
			{
				SetContentView(view);
			}
			else
			{
				SetContentView(null);
			}
		}

		// this is called after the Window.Content is created by
		// the fragment. We can't just create views on demand
		// need to let the fragments fall
		void OnWindowContentPlatformViewCreated()
		{
			RootViewChanged?.Invoke(this, EventArgs.Empty);

			// Toolbars are added dynamically to the layout, but this can't be done until the full base
			// layout has been set on the view.
			// This is mainly a problem because the toolbar native view is created during the 'ToContainerView'
			// and at this point the View that's going to house the Toolbar doesn't have access to
			// the AppBarLayout that's part of the RootView
			_toolbarElement?.Toolbar?.Parent?.Handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
		}

		public virtual void Disconnect()
		{
			ClearPlatformParts();
			SetContentView(null);
		}

		void ClearPlatformParts()
		{
			DrawerLayout = null;
			_rootView = null;
			_toolbarElement = null;
		}

		void SetContentView(IView? view)
		{
			if (view == null)
			{
				if (_viewFragment != null && !FragmentManager.IsDestroyed())
				{
					FragmentManager
						.BeginTransaction()
						.Remove(_viewFragment)
						.SetReorderingAllowed(true)
						.Commit();
				}

				_viewFragment = null;
			}
			else
			{
				_viewFragment =
					new ElementBasedFragment(
						view,
						_mauiContext!,
						OnWindowContentPlatformViewCreated);

				FragmentManager
					.BeginTransaction()
					.Replace(Resource.Id.navigationlayout_content, _viewFragment)
					.SetReorderingAllowed(true)
					.Commit();
			}
		}

		class ElementBasedFragment : ScopedFragment
		{
			public ElementBasedFragment(
				IView view,
				IMauiContext mauiContext,
				Action viewCreated) : base(view, mauiContext)
			{
				ViewCreated = viewCreated;
			}

			public Action ViewCreated { get; }

			public override void OnViewCreated(AView view, Bundle? savedInstanceState)
			{
				base.OnViewCreated(view, savedInstanceState);
				ViewCreated.Invoke();
			}
		}
	}
}
