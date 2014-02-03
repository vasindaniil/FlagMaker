﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FlagMaker.Localization;

namespace FlagMaker.Overlays.OverlayTypes.PathTypes
{
	public class OverlayPath : Overlay
	{
		private readonly string _name;
		private readonly Vector _pathSize;
		private readonly string _path;

		protected OverlayPath(string name, string path, Vector pathSize, int maximumX, int maximumY)
			: base(new List<Attribute>
			       {
				       new Attribute(strings.X, true, 1, true),
				       new Attribute(strings.Y, true, 1, false),
				       new Attribute(strings.Size, true, 1, true),
				       new Attribute(strings.Rotation, true, 0, true)
			       }, maximumX, maximumY)
		{
			_name = name;
			_path = path;
			_pathSize = pathSize;
		}

		protected OverlayPath(Color color, string name, string path, Vector pathSize, int maximumX, int maximumY)
			: base(color, new List<Attribute>
			       {
				       new Attribute(strings.X, true, 1, true),
				       new Attribute(strings.Y, true, 1, false),
				       new Attribute(strings.Size, true, 1, true),
				       new Attribute(strings.Rotation, true, 0, true)
			       }, maximumX, maximumY)
		{
			_name = name;
			_path = path;
			_pathSize = pathSize;
		}

		public override string Name
		{
			get { return _name; }
		}

		public override void Draw(Canvas canvas)
		{
			double xGridSize = canvas.Width / MaximumX;
			double yGridSize = canvas.Height / MaximumY;

			double x = Attributes.Get(strings.X).Value;
			double y = Attributes.Get(strings.Y).Value;

			var finalCenterPoint = new Point(x * xGridSize, y * yGridSize);

			var idealPixelSize = Attributes.Get(strings.Size).Value / MaximumX * Math.Max(canvas.Width, canvas.Height);

			var scaleFactor = idealPixelSize / _pathSize.X;

			var transformGroup = new TransformGroup();
			var rotateTransform = new RotateTransform((Attributes.Get(strings.Rotation).Value / MaximumX) * 360);
			transformGroup.Children.Add(rotateTransform);
			var scaleTransform = new ScaleTransform(scaleFactor, scaleFactor);
			transformGroup.Children.Add(scaleTransform);

			var path = new Path
			{
				Fill = new SolidColorBrush(Color),
				RenderTransform = transformGroup,
				Data = Geometry.Parse(_path),
				SnapsToDevicePixels = true
			};

			canvas.Children.Add(path);

			Canvas.SetLeft(path, finalCenterPoint.X);
			Canvas.SetTop(path, finalCenterPoint.Y);
		}

		public override void SetValues(List<double> values)
		{
			Attributes.Get(strings.X).Value = values[0];
			Attributes.Get(strings.Y).Value = values[1];
			Attributes.Get(strings.Size).Value = values[2];
			Attributes.Get(strings.Rotation).Value = values[3];
		}

		public override string ExportSvg(int width, int height)
		{
			double xGridSize = (double)width / MaximumX;
			double yGridSize = (double)height / MaximumY;

			double x = Attributes.Get(strings.X).Value;
			double y = Attributes.Get(strings.Y).Value;

			var finalCenterPoint = new Point(x * xGridSize, y * yGridSize);

			var idealPixelSize = Attributes.Get(strings.Size).Value / MaximumX * Math.Max(width, height);
			var scaleFactor = idealPixelSize / _pathSize.X;
			var rotate = (Attributes.Get(strings.Rotation).Value / MaximumX) * 360;

			return string.Format(CultureInfo.InvariantCulture, "<g transform=\"translate({2},{3}) rotate({0}) scale({1})\"><path d=\"{4}\" fill=\"#{5}\" /></g>",
					rotate, scaleFactor, finalCenterPoint.X, finalCenterPoint.Y, _path, Color.ToHexString());
		}

		public override IEnumerable<Shape> Thumbnail
		{
			get
			{
				const double thumbSize = 30.0;
				double scale = thumbSize / Math.Max(_pathSize.X, _pathSize.Y);
				return new List<Shape>
				       {
					       new Path
					       {
						       RenderTransform = new TransformGroup
						                         {
							                         Children = new TransformCollection
							                                    {
								                                    new ScaleTransform(scale, scale),
								                                    new TranslateTransform(thumbSize / 2, thumbSize / 2)
							                                    }
						                         },
						       Data = Geometry.Parse(_path),
						       SnapsToDevicePixels = true
					       }
				       };
			}
		}
	}
}
