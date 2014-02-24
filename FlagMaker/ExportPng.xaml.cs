﻿using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FlagMaker
{
	public partial class ExportPng
	{
		private readonly Ratio _ratio;
		private int _width;
		private int _height;
		private bool _update;

		public ExportPng(Ratio ratio)
		{
			InitializeComponent();

			const int multiplier = 100;
			_ratio = ratio;
			PngWidth = ratio.Width * multiplier;
			PngHeight = ratio.Height * multiplier;
			_update = true;
		}

		public int PngWidth
		{
			get { return _width; }
			private set
			{
				_width = value;
				TxtWidth.Text = _width.ToString(CultureInfo.InvariantCulture);
			}
		}

		public int PngHeight
		{
			get { return _height; }
			private set
			{
				_height = value;
				TxtHeight.Text = _height.ToString(CultureInfo.InvariantCulture);
			}
		}

		private void WidthChanged(object sender, TextChangedEventArgs e)
		{
			if (!_update) return;
			_update = false;
			int newWidth;

			if (int.TryParse(TxtWidth.Text, out newWidth))
			{
				_width = newWidth;
				PngHeight = (int)((_ratio.Height / (double)_ratio.Width) * _width);
			}
			else
			{
				TxtWidth.Text = _width.ToString(CultureInfo.InvariantCulture);
			}
			_update = true;
		}

		private void HeightChanged(object sender, TextChangedEventArgs e)
		{
			if (!_update) return;
			_update = false;
			int newHeight;

			if (int.TryParse(TxtHeight.Text, out newHeight))
			{
				_width = newHeight;
				PngWidth = (int)((_ratio.Width / (double)_ratio.Height) * _height);
			}
			else
			{
				TxtHeight.Text = _height.ToString(CultureInfo.InvariantCulture);
			}
			_update = true;
		}

		private void OkClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
