﻿#nullable enable
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.HotReload;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/View.xml" path="Type[@FullName='Microsoft.Maui.Controls.View']/Docs/*" />
	public partial class View : IView, IPropertyMapperView, IHotReloadableView, IControlsView
	{
		Thickness IView.Margin => Margin;
		partial void HandlerChangedPartial();
		GestureManager _gestureManager;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			HandlerChangedPartial();
		}

		protected PropertyMapper propertyMapper;

		internal protected PropertyMapper<T> GetRendererOverrides<T>() where T : IView =>
			(PropertyMapper<T>)(propertyMapper as PropertyMapper<T> ?? (propertyMapper = new PropertyMapper<T>()));

		PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() => propertyMapper;

		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => HorizontalOptions.ToCore();
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => VerticalOptions.ToCore();

		#region HotReload

		IView IReplaceableView.ReplacedView =>
			MauiHotReloadHelper.GetReplacedView(this) ?? this;

		IReloadHandler IHotReloadableView.ReloadHandler { get; set; }

		void IHotReloadableView.TransferState(IView newView)
		{
			//TODO: LEt you hot reload the the ViewModel
			if (newView is View v)
				v.BindingContext = BindingContext;
		}

		void IHotReloadableView.Reload()
		{
			Dispatcher.Dispatch(() =>
			{
				this.CheckHandlers();
				//Handler = null;
				var reloadHandler = ((IHotReloadableView)this).ReloadHandler;
				reloadHandler?.Reload();
				//TODO: if reload handler is null, Do a manual reload?
			});
		}

		#endregion
	}
}