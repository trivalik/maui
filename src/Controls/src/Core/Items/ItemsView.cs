#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="Type[@FullName='Microsoft.Maui.Controls.ItemsView']/Docs/*" />
	public abstract class ItemsView : View
	{
		List<Element> _logicalChildren = new List<Element>();

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='EmptyViewProperty']/Docs/*" />
		public static readonly BindableProperty EmptyViewProperty =
			BindableProperty.Create(nameof(EmptyView), typeof(object), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='EmptyView']/Docs/*" />
		public object EmptyView
		{
			get => GetValue(EmptyViewProperty);
			set => SetValue(EmptyViewProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='EmptyViewTemplateProperty']/Docs/*" />
		public static readonly BindableProperty EmptyViewTemplateProperty =
			BindableProperty.Create(nameof(EmptyViewTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='EmptyViewTemplate']/Docs/*" />
		public DataTemplate EmptyViewTemplate
		{
			get => (DataTemplate)GetValue(EmptyViewTemplateProperty);
			set => SetValue(EmptyViewTemplateProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemsSourceProperty']/Docs/*" />
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemsSource']/Docs/*" />
		public IEnumerable ItemsSource
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='RemainingItemsThresholdReachedCommandProperty']/Docs/*" />
		public static readonly BindableProperty RemainingItemsThresholdReachedCommandProperty =
			BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommand), typeof(ICommand), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='RemainingItemsThresholdReachedCommand']/Docs/*" />
		public ICommand RemainingItemsThresholdReachedCommand
		{
			get => (ICommand)GetValue(RemainingItemsThresholdReachedCommandProperty);
			set => SetValue(RemainingItemsThresholdReachedCommandProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='RemainingItemsThresholdReachedCommandParameterProperty']/Docs/*" />
		public static readonly BindableProperty RemainingItemsThresholdReachedCommandParameterProperty = BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommandParameter), typeof(object), typeof(ItemsView), default(object));

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='RemainingItemsThresholdReachedCommandParameter']/Docs/*" />
		public object RemainingItemsThresholdReachedCommandParameter
		{
			get => GetValue(RemainingItemsThresholdReachedCommandParameterProperty);
			set => SetValue(RemainingItemsThresholdReachedCommandParameterProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='HorizontalScrollBarVisibilityProperty']/Docs/*" />
		public static readonly BindableProperty HorizontalScrollBarVisibilityProperty = BindableProperty.Create(
			nameof(HorizontalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ItemsView),
			ScrollBarVisibility.Default);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='HorizontalScrollBarVisibility']/Docs/*" />
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
			set => SetValue(HorizontalScrollBarVisibilityProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='VerticalScrollBarVisibilityProperty']/Docs/*" />
		public static readonly BindableProperty VerticalScrollBarVisibilityProperty = BindableProperty.Create(
			nameof(VerticalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ItemsView),
			ScrollBarVisibility.Default);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='VerticalScrollBarVisibility']/Docs/*" />
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
			set => SetValue(VerticalScrollBarVisibilityProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='RemainingItemsThresholdProperty']/Docs/*" />
		public static readonly BindableProperty RemainingItemsThresholdProperty =
			BindableProperty.Create(nameof(RemainingItemsThreshold), typeof(int), typeof(ItemsView), -1, validateValue: (bindable, value) => (int)value >= -1);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='RemainingItemsThreshold']/Docs/*" />
		public int RemainingItemsThreshold
		{
			get => (int)GetValue(RemainingItemsThresholdProperty);
			set => SetValue(RemainingItemsThresholdProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='AddLogicalChild']/Docs/*" />
		public void AddLogicalChild(Element element)
		{
			if (element == null)
			{
				return;
			}

			_logicalChildren.Add(element);
			element.Parent = this;
			OnChildAdded(element);
			VisualDiagnostics.OnChildAdded(this, element);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='RemoveLogicalChild']/Docs/*" />
		public void RemoveLogicalChild(Element element)
		{
			if (element == null)
			{
				return;
			}

			element.Parent = null;

			if (!_logicalChildren.Contains(element))
				return;

			var oldLogicalIndex = _logicalChildren.IndexOf(element);
			_logicalChildren.Remove(element);
			OnChildRemoved(element, oldLogicalIndex);
			VisualDiagnostics.OnChildRemoved(this, element, oldLogicalIndex);
		}

		internal override IReadOnlyList<Element> LogicalChildrenInternal => _logicalChildren.AsReadOnly();

		internal static readonly BindableProperty InternalItemsLayoutProperty =
			BindableProperty.Create(nameof(ItemsLayout), typeof(IItemsLayout), typeof(ItemsView),
				LinearItemsLayout.Vertical, propertyChanged: OnInternalItemsLayoutPropertyChanged);

		static void OnInternalItemsLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is BindableObject boOld)
				SetInheritedBindingContext(boOld, null);

			if (newValue is BindableObject boNew)
				SetInheritedBindingContext(boNew, bindable.BindingContext);
		}

		protected IItemsLayout InternalItemsLayout
		{
			get => (IItemsLayout)GetValue(InternalItemsLayoutProperty);
			set => SetValue(InternalItemsLayoutProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemTemplateProperty']/Docs/*" />
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemsView));

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemTemplate']/Docs/*" />
		public DataTemplate ItemTemplate
		{
			get => (DataTemplate)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemsUpdatingScrollModeProperty']/Docs/*" />
		public static readonly BindableProperty ItemsUpdatingScrollModeProperty =
			BindableProperty.Create(nameof(ItemsUpdatingScrollMode), typeof(ItemsUpdatingScrollMode), typeof(ItemsView),
				default(ItemsUpdatingScrollMode));

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemsUpdatingScrollMode']/Docs/*" />
		public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
		{
			get => (ItemsUpdatingScrollMode)GetValue(ItemsUpdatingScrollModeProperty);
			set => SetValue(ItemsUpdatingScrollModeProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ScrollTo'][1]/Docs/*" />
		public void ScrollTo(int index, int groupIndex = -1,
			ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			OnScrollToRequested(new ScrollToRequestEventArgs(index, groupIndex, position, animate));
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ScrollTo'][2]/Docs/*" />
		public void ScrollTo(object item, object group = null,
			ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			OnScrollToRequested(new ScrollToRequestEventArgs(item, group, position, animate));
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='SendRemainingItemsThresholdReached']/Docs/*" />
		public void SendRemainingItemsThresholdReached()
		{
			RemainingItemsThresholdReached?.Invoke(this, EventArgs.Empty);

			if (RemainingItemsThresholdReachedCommand?.CanExecute(RemainingItemsThresholdReachedCommandParameter) == true)
				RemainingItemsThresholdReachedCommand?.Execute(RemainingItemsThresholdReachedCommandParameter);

			OnRemainingItemsThresholdReached();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='SendScrolled']/Docs/*" />
		public void SendScrolled(ItemsViewScrolledEventArgs e)
		{
			Scrolled?.Invoke(this, e);

			OnScrolled(e);
		}

		public event EventHandler<ScrollToRequestEventArgs> ScrollToRequested;

		public event EventHandler<ItemsViewScrolledEventArgs> Scrolled;

		public event EventHandler RemainingItemsThresholdReached;

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			// TODO hartez 2018-05-22 05:04 PM This 40,40 is what LV1 does; can we come up with something less arbitrary?
			var minimumSize = new Size(40, 40);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			var maxWidth = Math.Min(scaled.Width, widthConstraint);
			var maxHeight = Math.Min(scaled.Height, heightConstraint);

			Size request = new Size(maxWidth, maxHeight);

			return new SizeRequest(request, minimumSize);
		}

		protected virtual void OnScrollToRequested(ScrollToRequestEventArgs e)
		{
			ScrollToRequested?.Invoke(this, e);
		}

		protected virtual void OnRemainingItemsThresholdReached()
		{

		}

		protected virtual void OnScrolled(ItemsViewScrolledEventArgs e)
		{

		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			if (InternalItemsLayout is BindableObject bo)
				SetInheritedBindingContext(bo, BindingContext);
		}
	}
}
