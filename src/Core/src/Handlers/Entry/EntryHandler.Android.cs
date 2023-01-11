﻿using System;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Microsoft.Maui.Platform;
using static Android.Views.View;
using static Android.Widget.TextView;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiAppCompatEditText>
	{
		Drawable? _clearButtonDrawable;
		bool _clearButtonVisible;
		bool _set;

		protected override MauiAppCompatEditText CreatePlatformView()
		{
			var nativeEntry = new MauiAppCompatEditText(Context);
			return nativeEntry;
		}

		// Returns the default 'X' char drawable in the AppCompatEditText.
		protected virtual Drawable? GetClearButtonDrawable() =>
			_clearButtonDrawable ??= ContextCompat.GetDrawable(Context, Resource.Drawable.abc_ic_clear_material);

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (!_set)
				PlatformView.SelectionChanged += OnSelectionChanged;

			_set = true;
		}

		protected override void ConnectHandler(MauiAppCompatEditText platformView)
		{
			platformView.TextChanged += OnTextChanged;
			platformView.FocusChange += OnFocusedChange;
			platformView.Touch += OnTouch;
			platformView.EditorAction += OnEditorAction;
		}

		protected override void DisconnectHandler(MauiAppCompatEditText platformView)
		{
			_clearButtonDrawable = null;
			platformView.TextChanged -= OnTextChanged;
			platformView.FocusChange -= OnFocusedChange;
			platformView.Touch -= OnTouch;
			platformView.EditorAction -= OnEditorAction;

			if (_set)
				platformView.SelectionChanged -= OnSelectionChanged;

			_set = false;
		}

		public static void MapBackground(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateBackground(entry);

		public static void MapText(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateText(entry);

		public static void MapTextColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapIsPassword(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry)
		{
			if (handler is EntryHandler platformHandler)
				handler.PlatformView?.UpdatePlaceholderColor(entry);
		}

		public static void MapFont(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapReturnType(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCharacterSpacing(entry);

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry)
		{
			if (handler is EntryHandler platformHandler)
				handler.PlatformView?.UpdateClearButtonVisibility(entry);
		}

		void OnTextChanged(object? sender, TextChangedEventArgs e)
		{
			if (VirtualView == null)
			{
				return;
			}

			VirtualView.UpdateText(e);
			MapClearButtonVisibility(this, VirtualView);
		}

		void OnFocusedChange(object? sender, FocusChangeEventArgs e)
		{
			if (VirtualView == null)
			{
				return;
			}

			MapClearButtonVisibility(this, VirtualView);
		}

		// Check whether the touched position inbounds with clear button.
		void OnTouch(object? sender, TouchEventArgs e) =>
			e.Handled =
				_clearButtonVisible && VirtualView != null &&
				PlatformView.HandleClearButtonTouched(e, GetClearButtonDrawable);

		void OnEditorAction(object? sender, EditorActionEventArgs e)
		{
			var returnType = VirtualView?.ReturnType;

			if (returnType != null)
			{
				var currentInputImeFlag = returnType.Value.ToPlatform();

				if (e.IsCompletedAction(currentInputImeFlag))
				{
					VirtualView?.Completed();
				}
			}

			e.Handled = false;
		}

		private void OnSelectionChanged(object? sender, EventArgs e)
		{
			var cursorPosition = PlatformView.GetCursorPosition();
			var selectedTextLength = PlatformView.GetSelectedTextLength();

			if (VirtualView.CursorPosition != cursorPosition)
				VirtualView.CursorPosition = cursorPosition;

			if (VirtualView.SelectionLength != selectedTextLength)
				VirtualView.SelectionLength = selectedTextLength;
		}

		internal void ShowClearButton()
		{
			if (_clearButtonVisible)
			{
				return;
			}

			var drawable = GetClearButtonDrawable();

			if (PlatformView.LayoutDirection == LayoutDirection.Rtl)
			{
				PlatformView.SetCompoundDrawablesWithIntrinsicBounds(drawable, null, null, null);
			}
			else
			{
				PlatformView.SetCompoundDrawablesWithIntrinsicBounds(null, null, drawable, null);
			}

			_clearButtonVisible = true;
		}

		internal void HideClearButton()
		{
			if (!_clearButtonVisible)
			{
				return;
			}

			PlatformView.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
			_clearButtonVisible = false;
		}
	}
}
