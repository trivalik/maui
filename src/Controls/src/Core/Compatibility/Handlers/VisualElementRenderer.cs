﻿#if WINDOWS || ANDROID || IOS || TIZEN

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
#if WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif IOS
using PlatformView = UIKit.UIView;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#endif

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
#if WINDOWS
	public abstract partial class VisualElementRenderer<TElement, TPlatformElement> : IPlatformViewHandler
		where TElement : VisualElement
		where TPlatformElement : PlatformView
#else
	public abstract partial class VisualElementRenderer<TElement> : IPlatformViewHandler
		where TElement : Element, IView
#endif
	{
		public static IPropertyMapper<TElement, IPlatformViewHandler> VisualElementRendererMapper = new PropertyMapper<TElement, IPlatformViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IView.AutomationId)] = MapAutomationId,
			[nameof(IView.Background)] = MapBackground,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
			[nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
			[AutomationProperties.IsInAccessibleTreeProperty.PropertyName] = MapAutomationPropertiesIsInAccessibleTree,
#if WINDOWS
			[AutomationProperties.NameProperty.PropertyName] = MapAutomationPropertiesName,
			[AutomationProperties.HelpTextProperty.PropertyName] = MapAutomationPropertiesHelpText,
			[AutomationProperties.LabeledByProperty.PropertyName] = MapAutomationPropertiesLabeledBy,
#endif
		};

		public static CommandMapper<TElement, IPlatformViewHandler> VisualElementRendererCommandMapper = new CommandMapper<TElement, IPlatformViewHandler>(ViewHandler.ViewCommandMapper);

		TElement? _virtualView;
		IMauiContext? _mauiContext;
		internal IPropertyMapper _mapper;
		internal readonly CommandMapper? _commandMapper;
		internal readonly IPropertyMapper _defaultMapper;
		protected IMauiContext MauiContext => _mauiContext ?? throw new InvalidOperationException("MauiContext not set");
		public TElement? Element => _virtualView;
		protected bool AutoPackage { get; set; } = true;

#if ANDROID
		public VisualElementRenderer(Android.Content.Context context) : this(context, VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{
		}
#else
		public VisualElementRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{
		}
#endif


#if ANDROID
		internal VisualElementRenderer(Android.Content.Context context, IPropertyMapper mapper, CommandMapper? commandMapper = null)
#else
		internal VisualElementRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
#endif

#if ANDROID
			: base(context)
#elif IOS
			: base(CoreGraphics.CGRect.Empty)
#else
			: base()
#endif
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
			_commandMapper = commandMapper;
		}

		public event EventHandler<ElementChangedEventArgs<TElement>>? ElementChanged;
		public event EventHandler<PropertyChangedEventArgs>? ElementPropertyChanged;

		public void SetElement(IView view)
		{
			((IPlatformViewHandler)this).SetVirtualView(view);
		}

		partial void ElementChangedPartial(ElementChangedEventArgs<TElement> e);
		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			ElementChanged?.Invoke(this, e);
			ElementChangedPartial(e);
		}

		partial void ElementPropertyChangedPartial(object sender, PropertyChangedEventArgs e);

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Element != null && e.PropertyName != null)
				_mapper.UpdateProperty(this, Element, e.PropertyName);

			ElementPropertyChanged?.Invoke(sender, e);
			ElementPropertyChangedPartial(sender, e);
		}


		internal static Size GetDesiredSize(IPlatformViewHandler handler, double widthConstraint, double heightConstraint, Size? minimumSize)
		{
			var size = handler.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

			if (minimumSize != null)
			{
				var minSize = minimumSize.Value;

				if (size.Height < minSize.Height || size.Width < minSize.Width)
				{
					return new Size(
							size.Width < minSize.Width ? minSize.Width : size.Width,
							size.Height < minSize.Height ? minSize.Height : size.Height
						);
				}
			}

			return size;
		}

		Size IViewHandler.GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var sizeRequest = GetDesiredSize(widthConstraint, heightConstraint);
			var minSize = sizeRequest.Minimum;
			var size = sizeRequest.Request;

			if (size.Height < minSize.Height || size.Width < minSize.Width)
			{
				return new Size(
						size.Width < minSize.Width ? minSize.Width : size.Width,
						size.Height < minSize.Height ? minSize.Height : size.Height
					);
			}

			return size;
		}

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var minSize = MinimumSize();
			var size = GetDesiredSize(this, widthConstraint, heightConstraint, minSize);
			return new SizeRequest(size, minSize);
		}

#if TIZEN
		protected new virtual Size MinimumSize()
#else
		protected virtual Size MinimumSize()
#endif
		{
			return new Size();
		}


#if IOS
		protected virtual void SetBackgroundColor(Color? color)
#else
		protected virtual void UpdateBackgroundColor()
#endif
		{
			if (Element != null)
				ViewHandler.MapBackground(this, Element);
		}

#if IOS
		protected virtual void SetBackground(Brush brush)
#else
		protected virtual void UpdateBackground()
#endif
		{
			if (Element != null)
				ViewHandler.MapBackground(this, Element);
		}


		protected virtual void SetAutomationId(string id)
		{
			if (Element != null)
				ViewHandler.MapAutomationId(this, Element);
		}

		protected virtual void SetIsEnabled()
		{
			if (Element != null)
				ViewHandler.MapIsEnabled(this, Element);
		}

#if WINDOWS
		protected virtual void SetAutomationPropertiesAccessibilityView()
#else
		protected virtual void SetImportantForAccessibility()
#endif
		{
			if (Element != null)
				VisualElement.MapAutomationPropertiesIsInAccessibleTree(this, Element);
		}


		bool IViewHandler.HasContainer { get => true; set { } }

		object? IViewHandler.ContainerView => this;

		IView? IViewHandler.VirtualView => Element;

		Maui.IElement? IElementHandler.VirtualView => Element;

		IMauiContext? IElementHandler.MauiContext => _mauiContext;

		PlatformView? IPlatformViewHandler.PlatformView
		{
			get => ((Element?.Handler)?.PlatformView as PlatformView) ?? this;
		}

		PlatformView? IPlatformViewHandler.ContainerView => this;

		void IViewHandler.PlatformArrange(Rect rect) =>
			this.PlatformArrangeHandler(rect);

		void IElementHandler.SetMauiContext(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		internal static void SetVirtualView(
			Maui.IElement view,
			IPlatformViewHandler nativeViewHandler,
			Action<ElementChangedEventArgs<TElement>> onElementChanged,
			ref TElement? currentVirtualView,
			ref IPropertyMapper _mapper,
			IPropertyMapper _defaultMapper,
			bool autoPackage)
		{
			if (currentVirtualView == view)
				return;

			var oldElement = currentVirtualView;
			currentVirtualView = view as TElement;
			onElementChanged?.Invoke(new ElementChangedEventArgs<TElement>(oldElement, currentVirtualView));

			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (oldElement?.Handler != null)
				oldElement.Handler = null;

			currentVirtualView = (TElement)view;

			if (currentVirtualView.Handler != nativeViewHandler)
				currentVirtualView.Handler = nativeViewHandler;

			_mapper = _defaultMapper;

			if (currentVirtualView is IPropertyMapperView imv)
			{
				var map = imv.GetPropertyMapperOverrides();
				if (map is not null)
				{
					map.Chained = new[] { _defaultMapper };
					_mapper = map;
				}
			}

			if (autoPackage)
			{
				ProcessAutoPackage(view);
			}

			_mapper.UpdateProperties(nativeViewHandler, currentVirtualView);
		}

		static partial void ProcessAutoPackage(Maui.IElement element);

		void IElementHandler.SetVirtualView(Maui.IElement view) =>
			SetVirtualView(view, this, OnElementChanged, ref _virtualView, ref _mapper, _defaultMapper, AutoPackage);

		void IElementHandler.UpdateValue(string property)
		{
			if (Element != null)
			{
				OnElementPropertyChanged(Element, new PropertyChangedEventArgs(property));
			}
		}

		void IElementHandler.Invoke(string command, object? args)
		{
			_commandMapper?.Invoke(this, Element, command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			DisconnectHandlerCore();
			if (Element != null && Element.Handler == (IPlatformViewHandler)this)
				Element.Handler = null;

			_virtualView = null;
		}

		private protected virtual void DisconnectHandlerCore()
		{

		}

		public static void MapAutomationPropertiesIsInAccessibleTree(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
				ver.SetAutomationPropertiesAccessibilityView();
#else
			if (handler is VisualElementRenderer<TElement> ver)
				ver.SetImportantForAccessibility();
#endif
		}

		public static void MapAutomationId(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
				ver.SetAutomationId(view.AutomationId);
		}

		public static void MapBackgroundColor(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
#if IOS
				ver.SetBackgroundColor(view.Background?.ToColor());
#else
				ver.UpdateBackgroundColor();
#endif
		}

		public static void MapBackground(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
#if IOS
				ver.SetBackground(view.Background);
#else
				ver.UpdateBackground();
#endif
		}

		public static void MapIsEnabled(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TPlatformElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
				ver.SetIsEnabled();
		}
	}
}
#endif