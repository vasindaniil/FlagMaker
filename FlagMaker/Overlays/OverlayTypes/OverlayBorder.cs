﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlagMaker.Overlays.OverlayTypes
{
	internal class OverlayBorder : Overlay
	{
		public OverlayBorder(int maximumX, int maximumY)
			: base(new List<Attribute>
			       {
				       new Attribute("Thickness", true, 1, true)
			       }, maximumX, maximumY)
		{
		}

		public OverlayBorder(Color color, int thickness, int maximumX, int maximumY)
			: base(color, new List<Attribute>
			              {
				              new Attribute("Thickness", true, thickness, true)
			              }, maximumX, maximumY)
		{
		}

		public override string Name { get { return "border"; } }

		public override void Draw(Canvas canvas)
		{
			double thickness = (canvas.Width / 2) * ((Attributes.Get("Thickness").Value + 1) / (MaximumX * 2));

			var path = new Path
			{
				Fill = new SolidColorBrush(Color),
				Data = new CombinedGeometry(GeometryCombineMode.Exclude,
					new RectangleGeometry(new Rect(0, 0, canvas.Width, canvas.Height)),
					new RectangleGeometry(new Rect(thickness, thickness, canvas.Width - thickness * 2, canvas.Height - thickness * 2))),
				SnapsToDevicePixels = true
			};

			canvas.Children.Add(path);
		}

		public override void SetValues(List<double> values)
		{
			Attributes.Get("Thickness").Value = values[0];
		}

		public override string ExportSvg(int width, int height)
		{
			double thickness = (width / 2.0) * ((Attributes.Get("Thickness").Value + 1) / (MaximumX * 2));

			return string.Format("<path d=\"M 0,0 {0},0 {0},{1} 0,{1} Z M {2},{2} {3},{2} {3},{4} {2},{4} Z\" fill=\"#{5}\" fill-rule=\"evenodd\" />",
				width, height,
				thickness,
				width - thickness,
				height - thickness,
				Color.ToHexString());
		}

		public override IEnumerable<Shape> Thumbnail
		{
			get
			{
				return new List<Shape>
				       {
						   new Rectangle
						   {
						       Width = 30,
						       Height = 20,
							   Stroke = Brushes.Black,
							   StrokeThickness = 3,
							   Fill = Brushes.Transparent
						   }
				       };
			}
		}
	}
}
