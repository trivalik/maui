﻿using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	public class ShapeDrawable : IDrawable
	{
		public ShapeDrawable()
		{

		}

		public ShapeDrawable(IShapeView? shape)
		{
			UpdateShapeView(shape);
		}

		internal IShapeView? ShapeView { get; set; }
		internal WindingMode WindingMode { get; set; }
		internal Matrix3x2? RenderTransform { get; set; }

		public void UpdateShapeView(IShapeView? shape)
		{
			ShapeView = shape;
		}

		public void UpdateWindingMode(WindingMode windingMode)
		{
			WindingMode = windingMode;
		}

		public void UpdateRenderTransform(Matrix3x2? renderTransform)
		{
			RenderTransform = renderTransform;
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			var rect = dirtyRect;

			IShape? shape = ShapeView?.Shape;

			if (shape == null)
				return;

			PathF? path = shape.PathForBounds(rect);

			if (path == null)
				return;

			ApplyTransform(path);

			DrawStrokePath(canvas, rect, path);
			DrawFillPath(canvas, rect, path);
		}

		void DrawStrokePath(ICanvas canvas, RectF dirtyRect, PathF path)
		{
			if (ShapeView == null || ShapeView.Shape == null || ShapeView.StrokeThickness <= 0 || ShapeView.Stroke == null)
				return;

			canvas.SaveState();

			// Set StrokeThickness
			float strokeThickness = (float)ShapeView.StrokeThickness;
			canvas.StrokeSize = strokeThickness;

			// Set Stroke
			var stroke = ShapeView.Stroke;

			// TODO: Add Paint support for Stroke in Microsoft.Maui.Graphics.
			// For now, only support a solid color.
			canvas.StrokeColor = stroke.ToColor();

			// Set StrokeLineCap
			var strokeLineCap = ShapeView.StrokeLineCap;
			canvas.StrokeLineCap = strokeLineCap;

			// Set StrokeLineJoin
			var strokeLineJoin = ShapeView.StrokeLineJoin;
			canvas.StrokeLineJoin = strokeLineJoin;

			// Set StrokeDashPattern
			var strokeDashPattern = ShapeView.StrokeDashPattern;
			canvas.StrokeDashPattern = strokeDashPattern;

			// Set StrokeDashOffset	
			var strokeDashOffset = ShapeView.StrokeDashOffset;
			canvas.StrokeDashOffset = strokeDashOffset;

			// Set StrokeMiterLimit
			var strokeMiterLimit = ShapeView.StrokeMiterLimit;
			canvas.MiterLimit = strokeMiterLimit;

			canvas.DrawPath(path);

			canvas.RestoreState();
		}

		void DrawFillPath(ICanvas canvas, RectF dirtyRect, PathF path)
		{
			if (ShapeView == null || ShapeView.Shape == null)
				return;

			canvas.SaveState();

			canvas.FillColor = Colors.Transparent;

			ClipPath(canvas, path);

			// Set Fill
			var fillPaint = ShapeView.Fill;

			if (fillPaint != null)
				canvas.SetFillPaint(fillPaint, dirtyRect);

			canvas.FillPath(path);

			canvas.RestoreState();
		}

		void ClipPath(ICanvas canvas, PathF path)
		{
			canvas.ClipPath(path, WindingMode);
		}

		void ApplyTransform(PathF path)
		{
			if (RenderTransform == null)
				return;

			path.Transform(RenderTransform.Value);
		}
	}
}